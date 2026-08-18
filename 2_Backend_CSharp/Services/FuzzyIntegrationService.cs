using System.Net.Http.Json;
using AkilliSera_API.Models;

namespace AkilliSera_API.Services
{
    public class FuzzyIntegrationService
    {
        private readonly HttpClient _httpClient;

        public FuzzyIntegrationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Bitki evresini, anlık sensör değerlerini ve saati alıp Fuzzy API'ye gönderir
        public async Task<FinalCommandDto?> ProcessAndRouteAsync(
            BitkiEvreleri evre,
            double toprakNemi,
            double ortamNemi,
            double sicaklik,
            int saat)
        {
            // Fuzzy sistemin beklediği formatta JSON paketi oluşturuluyor
            var fuzzyIstek = new
            {
                bitki = new
                {
                    bitkiAdi            = evre.BitkiAdi ?? "Bilinmiyor",
                    evreAdi             = evre.EvreAdi  ?? "Bilinmiyor",
                    minToprakNemi       = (double)(evre.MinToprakNemi       ?? 40),
                    maxToprakNemi       = (double)(evre.MaxToprakNemi       ?? 70),
                    minOrtamNemi        = (double)(evre.MinOrtamNemi        ?? 60),
                    maxOrtamNemi        = (double)(evre.MaxOrtamNemi        ?? 80),
                    gunduzMinSicaklik   = (double)(evre.GunduzMinSicaklik   ?? 18),
                    gunduzMaxSicaklik   = (double)(evre.GunduzMaxSicaklik   ?? 28),
                    geceMinSicaklik     = (double)(evre.GeceMinSicaklik     ?? 14),
                    geceMaxSicaklik     = (double)(evre.GeceMaxSicaklik     ?? 20)
                },
                anlikVeriler = new
                {
                    toprakNemi  = toprakNemi,
                    ortamNemi   = ortamNemi,
                    sicaklik    = sicaklik
                },
                zaman = new
                {
                    saat = saat
                }
            };

            // 1. Python Bulanık Mantık servisine (kapısına) sensör verisini yolluyoruz
            var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:5000/api/fuzzy/calculate", fuzzyIstek);

            // Eğer Python cevap vermezse sistemi çökertmemek için boş dönüyoruz
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // 2. Python'dan gelen JSON kararını bizim yazdığımız DTO sınıfına çeviriyoruz
            var fuzzyDecision = await response.Content.ReadFromJsonAsync<FuzzyDecisionDto>();
            if (fuzzyDecision?.Kararlar == null)
            {
                return null;
            }

            // 3. Çıkan sonucu Yusuf (Donanım) ve Sude (Veritabanı) için temiz bir pakete koyuyoruz
            var finalCommand = new FinalCommandDto
            {
                SulamaSuresi      = fuzzyDecision.Kararlar.Sulama.Sure,
                SulamaKararMetni  = fuzzyDecision.Kararlar.Sulama.Karar,

                FanSeviyesi       = fuzzyDecision.Kararlar.Havalandirma.Seviye,
                FanKararMetni     = fuzzyDecision.Kararlar.Havalandirma.Karar,

                IsitmaSeviyesi    = fuzzyDecision.Kararlar.Isitma.Seviye,
                IsitmaKararMetni  = fuzzyDecision.Kararlar.Isitma.Karar
            };

            // Hazırlanan paketi dışarı (bu metodu kim çağırırsa ona) veriyoruz
            return finalCommand;
        }
    }
}