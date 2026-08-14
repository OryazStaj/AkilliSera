namespace AkıllıSera.API.Services;

public class HealthCheckService
{

    // Cihazdan alınan en son verinin zamanı
 
    private DateTime _lastSeen = DateTime.MinValue;
    // Cihazın çevrimiçi olup olmadığını belirlemek için kullanılacak zaman aşımı süresi
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

    // Cihazdan alınan en son verinin zamanını güncellemek için kullanılan yöntem
    public void UpdatePulse() => _lastSeen = DateTime.UtcNow;

    // Cihazın son 30 saniye de çevrimiçi olup olmadığını kontrol etmek için kullanılan yöntem
    public bool IsOnline() => (DateTime.UtcNow - _lastSeen) < _timeout;

    // Cihazdan alınan en son verinin zamanını almak için kullanılan yöntem(Son görüldü)
    public DateTime GetLastSeen() => _lastSeen;
}