namespace AkilliSera_API.Services;

public class HealthCheckService
{
    // Cihazdan alınan en son verinin zamanı
    private DateTime? _lastSeen = null;

    // Cihazın çevrimiçi kabul edileceği zaman aşımı süresi (30 saniye)
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

    // Cihazdan veri geldiğinde son görülme zamanını günceller
    public void UpdatePulse() => _lastSeen = DateTime.UtcNow;

    // Son 30 saniye içinde veri geldiyse cihaz çevrimiçidir
    public bool IsOnline() => _lastSeen.HasValue && (DateTime.UtcNow - _lastSeen.Value) < _timeout;

    // Son görülme zamanını ISO formatında veya null döner
    public DateTime? GetLastSeen() => _lastSeen;
}