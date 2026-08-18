using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using AkilliSera_API.Hubs;
using AkilliSera_API.Services;
using AkilliSera_API.Data;
using AkilliSera_API.Models;

namespace AkilliSera_API.Controllers;

/// <summary>
/// ESP32 donanımından gelen telemetri verilerini doğrulayan, 
/// Sensor_Loglari tablosuna kalıcı kaydeden, SignalR ile canlı yayına basan 
/// ve aktüatör kararları üreten denetleyici sınıfı.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TelemetryController : ControllerBase
{
    private readonly IHubContext<SeraHub> _seraHub;
    private readonly HealthCheckService _healthCheckService;
    private readonly AkilliSeraDbContext _context;
    private readonly ILogger<TelemetryController> _logger;
    private readonly FuzzyIntegrationService _fuzzyService;

    public TelemetryController(
        IHubContext<SeraHub> seraHub,
        HealthCheckService healthCheckService,
        AkilliSeraDbContext context,
        ILogger<TelemetryController> logger,
        FuzzyIntegrationService fuzzyService)
    {
        _seraHub = seraHub;
        _healthCheckService = healthCheckService;
        _context = context;
        _logger = logger;
        _fuzzyService = fuzzyService;
    }

    /// <summary>
    /// ESP32'den gelen telemetri verisini kalıcı kaydeder ve canlı yayına iletir. (POST api/telemetry)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostTelemetry([FromBody] TelemetryDto dto)
    {
        // 1. Girdi Doğrulaması (Validation)
        if (dto == null)
            return BadRequest(new { message = "Veri paketi boş olamaz." });

        int effectiveSeraId = dto.SeraId > 0 ? dto.SeraId : dto.SectionId;
        double effectiveTemp = dto.OrtamSicakligi != 0 ? dto.OrtamSicakligi : dto.Temperature;
        double effectiveHumidity = dto.OrtamNemi != 0 ? dto.OrtamNemi : dto.Humidity;
        double effectiveSoil = dto.ToprakNemi != 0 ? dto.ToprakNemi : dto.SoilMoisture;

        if (effectiveSeraId <= 0)
            return BadRequest(new { message = "Geçersiz sera ID! Pozitif bir seraId gönderilmelidir." });

        if (effectiveHumidity < 0 || effectiveHumidity > 100 || effectiveSoil < 0 || effectiveSoil > 100)
            return BadRequest(new { message = "Nem değerleri %0 ile %100 arasında olmalıdır." });

        // 2. Cihazın Canlılık Nabzını Güncelle
        _healthCheckService.UpdatePulse();

        // 3. Veritabanına Kalıcı Kayıt Ekle
        DateTime kayitZamani = DateTime.UtcNow;

        try
        {
            var logEntry = new SensorLoglari
            {
                SeraId = effectiveSeraId,
                OrtamSicakligi = (decimal)effectiveTemp,
                OrtamNemi = (decimal)effectiveHumidity,
                ToprakNemi = (decimal)effectiveSoil,
                KayitZamani = kayitZamani
            };

            await _context.SensorLoglaris.AddAsync(logEntry);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Telemetri verisi veritabanına kaydedilirken hata oluştu!");
            return StatusCode(500, new { message = "Veritabanı kayıt hatası oluştu.", detail = ex.Message });
        }

        // 4. Fuzzy Mantık ile Aktüatör Kararı
        bool normalValve, fan, heater;

        try
        {
            // Seraya ait aktif bitki evresini DB'den çek
            var sera = await _context.SeraDurums
                .Include(s => s.AktifEvre)
                .FirstOrDefaultAsync(s => s.SeraId == effectiveSeraId);

            var aktifEvre = sera?.AktifEvre;

            if (aktifEvre != null)
            {
                // Aktif evre varsa Fuzzy API'yi çağır
                var fuzzyKarar = await _fuzzyService.ProcessAndRouteAsync(
                    aktifEvre,
                    effectiveSoil,
                    effectiveHumidity,
                    effectiveTemp,
                    kayitZamani.Hour
                );

                if (fuzzyKarar != null)
                {
                    // Fuzzy kararını aktüatör komutuna dönüştür
                    normalValve = fuzzyKarar.SulamaSuresi >= 5.0;
                    fan         = fuzzyKarar.FanSeviyesi  >= 30.0;
                    heater      = fuzzyKarar.IsitmaKararMetni == "isi_yukselt";

                    _logger.LogInformation(
                        "Fuzzy karar alındı: sulama={Sulama}sn, fan=%{Fan}, isitma={Isitma}",
                        fuzzyKarar.SulamaSuresi, fuzzyKarar.FanSeviyesi, fuzzyKarar.IsitmaKararMetni);
                }
                else
                {
                    // Fuzzy API cevap vermediyse sabit eşiğe geri dön
                    _logger.LogWarning("Fuzzy API yanıt vermedi, sabit eşik kullanılıyor.");
                    normalValve = effectiveSoil < 30.0;
                    fan         = effectiveTemp > 32.0;
                    heater      = effectiveTemp < 15.0;
                }
            }
            else
            {
                // Aktif evre tanımlı değilse sabit eşiğe geri dön
                _logger.LogWarning("Sera {SeraId} için aktif bitki evresi bulunamadı, sabit eşik kullanılıyor.", effectiveSeraId);
                normalValve = effectiveSoil < 30.0;
                fan         = effectiveTemp > 32.0;
                heater      = effectiveTemp < 15.0;
            }
        }
        catch (Exception ex)
        {
            // Fuzzy servisi hatası sistemi durdurmasın, sabit eşiğe geri dön
            _logger.LogError(ex, "Fuzzy karar sürecinde hata oluştu, sabit eşik kullanılıyor.");
            normalValve = effectiveSoil < 30.0;
            fan         = effectiveTemp > 32.0;
            heater      = effectiveTemp < 15.0;
        }

        // 5. Sadece DB'ye Başarıyla Kaydedilen Veriyi SignalR ile Yayınla
        await _seraHub.Clients.All.SendAsync("ReceiveTelemetry", new
        {
            sectionId    = effectiveSeraId,
            temperature  = effectiveTemp,
            humidity     = effectiveHumidity,
            soilMoisture = effectiveSoil,
            recordedAt   = kayitZamani.ToString("o")
        });

        // 6. ESP32 için Aktüatör Yanıtı
        return Ok(new
        {
            message        = "Telemetri başarıyla kaydedildi ve yayınlandı.",
            normalValve    = normalValve,
            treatmentValve = false,
            fan            = fan,
            heater         = heater
        });
    }

    /// <summary>
    /// ESP32 cihazının canlılık durumunu döner. (GET api/telemetry/health)
    /// </summary>
    [HttpGet("health")]
    public IActionResult GetHealthStatus()
    {
        return Ok(new
        {
            isOnline = _healthCheckService.IsOnline(),
            lastSeen = _healthCheckService.GetLastSeen()
        });
    }
}

/// <summary>
/// Telemetri DTO sınıfı
/// </summary>
public class TelemetryDto
{
    public int SeraId { get; set; }
    public double OrtamSicakligi { get; set; }
    public double OrtamNemi { get; set; }
    public double ToprakNemi { get; set; }

    public int SectionId { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double SoilMoisture { get; set; }
}