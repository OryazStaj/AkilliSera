using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using AkıllıSera.API.Hubs;
using AkıllıSera.API.Services;

namespace AkıllıSera.API.Controllers;
/// <summary>
/// ESP32 donanımından gelen telemetri verilerini karşılayan,
/// SignalR üzerinden canlı yayına basan ve aktüatör (röle) kararları üreten denetleyici sınıfı.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TelemetryController : ControllerBase
{
    private readonly IHubContext<SeraHub> _seraHub;
    private readonly HealthCheckService _healthCheckService;

    /// <summary>
    /// Bağımlılıkların (SignalR Hub ve Canlılık Takip Servisi) enjekte edildiği kurucu metot (Constructor).
    /// </summary>
    public TelemetryController(IHubContext<SeraHub> seraHub, HealthCheckService healthCheckService)
    {
        _seraHub = seraHub;
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// ESP32 cihazından gelen anlık sensör verilerini karşılar. (POST api/telemetry)
    /// </summary>
    /// <param name="dto">Sıcaklık, nem ve toprak nemi verilerini taşıyan DTO paketi.</param>
    /// <returns>ESP32'deki rölelerin/vanaların çalışma durumunu belirten karar yanıtı.</returns>
    [HttpPost]
    public async Task<IActionResult> PostTelemetry([FromBody] TelemetryDto dto)
    {
        if (dto == null) return BadRequest("Veri paketi boş olamaz.");

        // 1. ESP32'den veri geldiği için cihazın canlılık nabzını güncelle
        _healthCheckService.UpdatePulse();

        // 2. Anlık veriyi SignalR kanalı (SeraHub) üzerinden web/mobil arayüze canlı fırlat
        await _seraHub.Clients.All.SendAsync("ReceiveTelemetry", new
        {
            sectionId = dto.SectionId,
            temperature = dto.Temperature,
            humidity = dto.Humidity,
            soilMoisture = dto.SoilMoisture,
            recordedAt = DateTime.UtcNow
        });

        // 3. Sensör verilerine göre ESP32'nin vanaları/röleleri çalıştırma kararını dön
        return Ok(new
        {
            normalValve = dto.SoilMoisture < 30.0,  // Toprak nemi %30 altındaysa su pompasını çalıştır
            treatmentValve = false,                  // İlaçlama vanası varsayılan kapalı
            fan = dto.Temperature > 32.0,            // Sıcaklık 32°C üstündeyse fanı çalıştır
            heater = dto.Temperature < 15.0          // Sıcaklık 15°C altındaysa ısıtıcıyı çalıştır
        });
    }

    /// <summary>
    /// ESP32 cihazının çevrimiçi olup olmadığını kontrol eden uç nokta. (GET api/telemetry/health)
    /// </summary>
    /// <returns>Cihazın online durumunu ve son görülme tarihini döner.</returns>
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
/// ESP32'den gelen JSON veri paketini C# nesnesine dönüştüren Data Transfer Object (DTO) sınıfı.
/// </summary>
public class TelemetryDto
{
    public int SectionId { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double SoilMoisture { get; set; }
}