using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using AkilliSera_API.Data;
using AkilliSera_API.Models;
using AkilliSera_API.Hubs;

namespace AkilliSera_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalizController : ControllerBase
{
    private readonly AkilliSeraDbContext _context;
    private readonly IHubContext<SeraHub> _seraHub;
    private readonly ILogger<AnalizController> _logger;

    public AnalizController(
        AkilliSeraDbContext context,
        IHubContext<SeraHub> seraHub,
        ILogger<AnalizController> logger)
    {
        _context = context;
        _seraHub = seraHub;
        _logger = logger;
    }

    /// <summary>
    /// Python Yapay Zeka modülünden gelen anlık kamera/yaprak analiz sonucunu kaydeder.
    /// (POST /api/Analiz/goruntu-sonucu)
    /// </summary>
    [HttpPost("goruntu-sonucu")]
    public async Task<IActionResult> PostGoruntuSonucu([FromBody] GoruntuAnalizDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new { message = "Analiz verisi boş olamaz." });
        }

        int effectiveSeraId = dto.SeraId.HasValue && dto.SeraId.Value > 0 ? dto.SeraId.Value : 1;
        DateTime islemZamani = DateTime.UtcNow;

        try
        {
            // 1. Kamera loglarına durum bilgisini kaydet
            var kameraLog = new KameraLoglari
            {
                SeraId = effectiveSeraId,
                DurumBilgisi = $"Evre: {dto.BitkiEvresi} | {dto.Aciklama}",
                KayitZamani = islemZamani
            };
            await _context.KameraLoglaris.AddAsync(kameraLog);

            // 2. Eğer hastalık tespit edildiyse Bitki_Hastalik ve Bildirim tablolarına ekle
            if (dto.HastalikDetaylari != null && dto.HastalikDetaylari.Count > 0)
            {
                foreach (var hastalik in dto.HastalikDetaylari)
                {
                    var hastalikKaydi = new BitkiHastalik
                    {
                        BitkiId = effectiveSeraId,
                        HastalikAdi = hastalik.HastalikAdi,
                        HastalikOrani = (decimal)hastalik.GuvenSkoru,
                        FotografYolu = dto.FotografYolu ?? "kamera_anlik.jpg"
                    };
                    await _context.BitkiHastaliks.AddAsync(hastalikKaydi);

                    // Bildirim oluştur
                    var bildirim = new Bildirim
                    {
                        Mesaj = $"⚠️ Sera #{effectiveSeraId}: {hastalik.HastalikAdi} tespit edildi! (Güven: %{hastalik.GuvenSkoru:F1})",
                        OkunduBilgisi = false,
                        BildirimZamani = islemZamani
                    };
                    await _context.Bildirims.AddAsync(bildirim);
                }
            }

            // 3. Bitki Evresi güncellemesi (Sera_Durum)
            var sera = await _context.SeraDurums.FirstOrDefaultAsync(s => s.SeraId == effectiveSeraId);
            if (sera != null && !string.IsNullOrWhiteSpace(dto.BitkiEvresi))
            {
                var evre = await _context.BitkiEvreleris
                    .FirstOrDefaultAsync(e => e.EvreAdi.ToLower() == dto.BitkiEvresi.ToLower());
                
                if (evre != null)
                {
                    sera.AktifEvreId = evre.EvreId;
                }
                sera.SonGuncellemeZamani = islemZamani;
            }

            await _context.SaveChangesAsync();

            // 4. SignalR ile canlı yayın (Frontend anlık haberdar olsun)
            await _seraHub.Clients.All.SendAsync("ReceivePlantAnalysis", new
            {
                seraId = effectiveSeraId,
                bitkiEvresi = dto.BitkiEvresi,
                hastalikSayisi = dto.HastalikDetaylari?.Count ?? 0,
                hastaliklar = dto.HastalikDetaylari,
                domatesler = dto.DomatesDetaylari,
                aciklama = dto.Aciklama,
                fotografYolu = dto.FotografYolu,
                zaman = islemZamani.ToString("o")
            });

            return Ok(new
            {
                message = "Görüntü analizi başarıyla veritabanına kaydedildi ve canlı yayına iletildi.",
                hastalikSayisi = dto.HastalikDetaylari?.Count ?? 0,
                bitkiEvresi = dto.BitkiEvresi,
                seraId = effectiveSeraId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüntü analizi veritabanına kaydedilirken hata oluştu.");
            return StatusCode(500, new { message = "Veritabanı kayıt hatası oluştu.", detail = ex.Message });
        }
    }
}
