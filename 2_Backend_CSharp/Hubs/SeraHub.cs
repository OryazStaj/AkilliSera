using Microsoft.AspNetCore.SignalR;

namespace AkıllıSera.API.Hubs;

/// <summary>
/// ESP32'den gelen anlık telemetri verilerini Frontend (web/mobil) istemcilerine
/// canlı olarak yayınlayan SignalR iletişim kanalı (Hub) sınıfı.
/// </summary>
public class SeraHub : Hub
{
}