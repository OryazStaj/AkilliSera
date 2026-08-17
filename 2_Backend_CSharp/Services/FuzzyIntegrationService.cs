using System.Net.Http.Json;
using AkilliSera_API.Models;

namespace AkilliSera_API.Models
{
    public class FuzzyIntegrationService
    {
        private readonly HttpClient _httpClient;

        public FuzzyIntegrationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Hastalık bilgisini sildik, sadece sensör verisi (sensorData) alıyor
        public async Task<FinalCommandDto> ProcessAndRouteAsync(object sensorData)
        {
            // 1. Python Bulanık Mantık servisine (kapısına) sensör verisini yolluyoruz
            var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:5000/api/fuzzy/calculate", sensorData);

            // Eğer Python cevap vermezse sistemi çökertmemek için boş dönüyoruz
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // 2. Python'dan gelen JSON kararını bizim yazdığımız DTO sınıfına çeviriyoruz
            var fuzzyDecision = await response.Content.ReadFromJsonAsync<FuzzyDecisionDto>();

            // 3. Çıkan sonucu Yusuf (Donanım) ve Sude (Veritabanı) için temiz bir pakete koyuyoruz
            var finalCommand = new FinalCommandDto
            {
                SulamaSuresi = fuzzyDecision.Kararlar.Sulama.Sure,
                SulamaKararMetni = fuzzyDecision.Kararlar.Sulama.Karar,

                FanSeviyesi = fuzzyDecision.Kararlar.Havalandirma.Seviye,
                FanKararMetni = fuzzyDecision.Kararlar.Havalandirma.Karar,

                IsitmaSeviyesi = fuzzyDecision.Kararlar.Isitma.Seviye,
                IsitmaKararMetni = fuzzyDecision.Kararlar.Isitma.Karar
            };

            // Hazırlanan paketi dışarı (bu metodu kim çağırırsa ona) veriyoruz
            return finalCommand;
        }
    }
}