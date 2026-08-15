# Akıllı Sera – Entegrasyon Kontrol Listesi

Bu belge; frontend, backend, ESP32, yapay zekâ ve veritabanı ekiplerinin aynı veri sözleşmesiyle çalışması için minimum gereksinimleri tanımlar. Yeni özellik eklemeden önce ilgili bölümdeki maddeler tamamlanmalıdır.

## 1. Mevcut durum özeti

| Alan | Durum | Gerekli ilk iş |
|---|---|---|
| Veritabanı | SQL şeması mevcut | Dosyaları doğru bağımlılık sırasıyla çalıştırmak ve bağlantıyı doğrulamak |
| Backend | Telemetri/SignalR dosyaları mevcut; `dotnet build` namespace uyuşmazlığı nedeniyle başarısız | `AkıllıSera.API.*` namespace'lerini `AkilliSera_API.*` ile eşitlemek, sonra yapılandırmayı tamamlamak |
| Frontend | Razor Pages uygulaması ve API/SignalR istemcisi mevcut; `dotnet build` başarılı | Backend ayağa kalkınca canlı API, SignalR ve hata senaryolarını uçtan uca doğrulamak |
| ESP32 | Uygulama kodu henüz yok | Wi-Fi, API çağrısı ve sensör JSON sözleşmesini uygulamak |
| Yapay zekâ | Görüntü analizi JSON sözleşmesi tamamlandı ve örnek model testi geçti | Backend'in görüntü analizi endpoint/DTO'sunu yayınlamasını beklemek |

## 2. Projeyi ayağa kaldırmak için minimum sıra

1. SQL Server'da `AkilliSeraDB` veritabanını oluşturun.
2. SQL dosyalarını aşağıdaki bağımlılık sırasıyla çalıştırın:
   1. `01_Sabit_Referans_Tablolari.sql`
   2. `02_Dinamik_Log_Tablolari.sql`
   3. `05_Dis_Ortam.sql`
   4. `06_Kullanici_ve_Bildirim_Tablolari.sql`
   5. `07_Ilaclama_Takip.sql`
   6. `03_Baslangic_Verileri.sql`
   7. `04_Stored_Prosedur_Tablolari.sql`
3. Backend yapılandırmasına bağlantı metnini ekleyin; gerçek parola/bağlantı bilgilerini Git'e göndermeyin.
4. `2_Backend_CSharp` altında `dotnet build` ve ardından `dotnet run` çalıştırın.
5. Swagger üzerinden API'nin açıldığını ve veritabanına erişebildiğini kontrol edin.
6. Frontend ve ESP32 yalnızca aşağıda tanımlanan endpoint ve JSON alan adlarını kullanarak bağlanmalıdır. Frontend şu an `https://localhost:7266` adresini kullanacak şekilde yapılandırılmıştır.
7. Görüntü işleme çıktısını örnek bir görselle üretin; backend görüntü analizi endpoint'i hazır olduğunda aynı JSON'u uçtan uca gönderin.

## 3. Ortak kurallar

- API temel adresi geliştirmede `http://localhost:5108`'dir. HTTPS kullanılacaksa `https://localhost:7266` kullanılmalıdır.
- Route'lar büyük/küçük harfe duyarsız olsa da dokümanda yazıldığı biçimde kullanılmalıdır.
- ASP.NET Core JSON çıktısı varsayılan olarak `camelCase` döner. İstek gövdelerinde de `camelCase` kullanın.
- Tarih/saat değerleri ISO-8601 formatında gönderilmelidir: `2026-08-15T14:30:00Z`.
- Ondalık değerlerde JSON standardı gereği nokta kullanılmalıdır: `23.50`.
- Her kayıtta geçerli bir `seraId` gönderilmelidir. İlişkili sera kaydı veritabanında yoksa kayıt yapılmamalıdır.
- İstemciler hata durumlarında HTTP durum kodunu ve hata gövdesini göstermeli/kaydetmelidir; yalnızca başarılı yanıtı varsaymamalıdır.
- Yeni alan, tablo veya endpoint eklenmeden önce ilgili ekipler bu dosyadaki sözleşmeyi güncellemelidir.

## 4. Veritabanı ekibi kontrol listesi

- [ ] SQL dosyalarının yukarıdaki sırayla, boş bir veritabanında hatasız çalıştığını doğrulayın.
- [ ] `03_Baslangic_Verileri.sql` dosyasının bağımlı olduğu tablolardan sonra çalıştığını doğrulayın.
- [ ] `04_Stored_Prosedur_Tablolari.sql` dosyasını tüm tablolar oluştuktan sonra çalıştırın. Özellikle kullanıcı, hastalık, bildirim ve ilaçlama tabloları olmadan çalışmaz.
- [ ] En az bir `Sera_Durum` ve bir `Bitki_Evreleri` örnek kaydı ekleyin; sensör verisi gönderimi için FK ilişkisi gerekir.
- [ ] `Sensor_Loglari`, `Bitki_Hastalik`, `Ilaclama_Takip`, `Bildirim` ve `Dis_Ortam_Loglari` tablolarının kolonlarını EF Core modelleriyle karşılaştırın.
- [ ] Şema değişikliğinde tablo/kolon adını, veri tipini, null durumunu ve FK ilişkisini backend ekibine yazılı olarak bildirin.
- [ ] Uygulama hesabına yalnızca gereken yetkileri verin; geliştirme dışında Windows/yerel yönetici hesabını kullanmayın.

## 5. Backend ekibi kontrol listesi

### Zorunlu yapılandırma

#### 1. Derlemeyi engelleyen namespace uyuşmazlığı — Öncelik: kritik

Yeni `TelemetryController.cs`, `SeraHub.cs` ve `HealthCheckService.cs` dosyalarında namespace `AkıllıSera.API.*`; mevcut projenin diğer tüm dosyalarında ise `AkilliSera_API.*` kullanılıyor. Yeni dosyaların namespace ve `using` ifadeleri mevcut proje standardı olan `AkilliSera_API.*` ile eşitlenmelidir.

**Çözülmezse:** `Program.cs`, `SeraHub` ve `HealthCheckService` türlerini bulamaz; `dotnet build` başarısız olur ve API hiç çalışmaz.

#### 2. Veritabanı bağlantı metni — Öncelik: kritik

- [ ] `appsettings.Development.json` veya kullanıcı gizli ayarlarına aşağıdaki anahtarı ekleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=AkilliSeraDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

- [ ] Bağlantı metnini `AkilliSeraDbContext.OnConfiguring` içinde sabit tutmayın; tek kaynak yapılandırma olmalıdır.
- [ ] Başlangıçta veritabanı erişimini doğrulayan kısa bir health check veya kontrollü hata çıktısı ekleyin.
- [ ] `dotnet build` uyarılarını temizleyin; nullable giriş modeli ve null dönüş değerlerini açıkça tanımlayın.
- [ ] Hataları `Console.WriteLine` ile yutup başarılı yanıt dönmeyin. Kayıt başarısızsa uygun `4xx/5xx` yanıtı verin ve loglayın.

**Çözülmezse:** API açılmış görünse bile veritabanı kullanan endpoint'ler ilk istekte hata verebilir; sensör ve kullanıcı verileri kaydedilemez.

### Mevcut API sözleşmesi

| Amaç | Metot ve endpoint | İstek gövdesi / not | Frontend kullanımı |
|---|---|---|---|
| Sensör geçmişi | `GET /api/Sensors/history?limit=50` | En yeni kayıtlar önce dönecek şekilde sıralanmalı | Ana sayfa, sensörler ve grafikler |
| Sensör kaydı | `POST /api/Sensors/save` | Aşağıdaki sensör JSON'u | ESP32/test istemcisi için; frontend çağırmıyor |
| Kullanıcı listesi | `GET /api/Kullanici/liste` | Şifre alanı asla dönmemeli | Ekranda çağrı tespit edilmedi |
| Kayıt ol | `POST /api/Kullanici/kayit-ol` | Kullanıcı JSON'u | Giriş/kayıt ekranı |
| Giriş | `POST /api/Kullanici/giris-yap` | `{ "eposta": "...", "sifre": "..." }` | Giriş ekranı |
| Bitki evreleri | `GET /api/BitkiEvre/liste` | | Bitki evreleri ekranı |
| Hastalıklar | `GET /api/Hastalik/liste` | | Ana sayfa ve hastalıklar ekranı |
| İlaçlama geçmişi | `GET /api/Ilaclama/gecmis` | | Ana sayfa, sensörler ve ilaçlama ekranı |
| İlaçlama kaydı | `POST /api/Ilaclama/ekle` | İlaçlama JSON'u | İlaçlama ekranı |
| Bildirimler | `GET /api/Bildirim/liste` | | Ana sayfa |
| Cihaz canlılığı | `GET /api/Telemetry/health` | `isOnline`, `lastSeen` döner | Ana sayfa, sensörler ve kontrol ekranı |
| Canlı telemetri | SignalR `/serahub`, `ReceiveTelemetry` | `sectionId`, `temperature`, `humidity`, `soilMoisture`, `recordedAt` | Ana sayfa, sensörler ve grafikler |

Sensör kaydı örneği:

```json
{
  "seraId": 1,
  "ortamSicakligi": 24.6,
  "ortamNemi": 61.2,
  "toprakNemi": 48.7
}
```

İlaçlama kaydı örneği:

```json
{
  "hastalikId": 1,
  "ilacAdi": "Örnek ilaç"
}
```

Kullanıcı kaydı örneği:

```json
{
  "isim": "Ada",
  "soyisim": "Yılmaz",
  "eposta": "ada@example.com",
  "sifre": "guclu-bir-sifre"
}
```

### Tamamlanması gereken backend işleri

- [ ] Entity'leri doğrudan istemciye vermek yerine DTO kullanın.
- [ ] Girdi doğrulaması ekleyin: `seraId > 0`, nem `0–100`, anlamlı sıcaklık aralığı, zorunlu alanlar ve metin uzunlukları.
- [ ] Sensör geçmişini `KayitZamani DESC` ile sıralayın; `limit` için güvenli üst sınır belirleyin.
- [ ] Kullanıcı şifresini hashleyin; `GET /liste` yanıtından şifreyi tamamen çıkarın.
- [ ] Üretimde CORS'u `AllowAnyOrigin` yerine sadece frontend alan adına sınırlayın.
- [ ] Aşağıdaki eksik işlevler için endpoint/DTO tasarlayın: dış ortam kaydı, kamera analizi kaydı, aksiyon kaydı, aktif evre güncelleme, tek seranın anlık durumu ve ideal değerleri.
- [ ] Swagger'a örnek istekler, başarı/hata yanıtları ekleyin.

### Yeni telemetri ve SignalR için zorunlu uyum işleri

#### 3. Çift sensör sözleşmesini birleştirin — Öncelik: yüksek

Mevcut `POST /api/Sensors/save` şu alanları kullanır: `seraId`, `ortamSicakligi`, `ortamNemi`, `toprakNemi`. Yeni `POST /api/Telemetry` ise `sectionId`, `temperature`, `humidity`, `soilMoisture` bekler.

- [ ] ESP32 için tek bir kanonik JSON sözleşmesi seçin; önerilen alanlar `seraId`, `ortamSicakligi`, `ortamNemi`, `toprakNemi`dir.
- [ ] `TelemetryDto` seçilen sözleşmeye göre güncellensin veya açık bir dönüştürme katmanı eklensin.
- [ ] Swagger, ESP32 örnek kodu ve frontend alanları aynı sözleşmeye göre güncellensin.

**Çözülmezse:** Bir endpoint'e gönderilen veri diğerinde görünmez; alanlar varsayılan `0` değerine düşebilir ve yanlış sulama/fan/ısıtma kararları oluşabilir.

#### 4. Telemetriyi kalıcı olarak kaydedin — Öncelik: yüksek

Yeni `POST /api/Telemetry` veriyi SignalR ile yayınlıyor, ancak `Sensor_Loglari` tablosuna yazmıyor.

- [ ] Telemetri doğrulandıktan sonra `Sensor_Loglari` tablosuna kayıt ekleyin.
- [ ] Kayıt başarısız olursa başarı yanıtı dönmeyin; hata logu ve uygun HTTP yanıtı üretin.
- [ ] SignalR yayınının, yalnızca kabul edilen/kaydedilen veri için yapılacağını belirleyin.

**Çözülmezse:** Kullanıcı canlı değeri görür fakat sayfa yenilenince veri kaybolur; geçmiş grafikler, raporlar ve AI'nin kullanacağı geçmiş veri eksik kalır.

#### 5. Görüntü analizi kabul endpoint'i oluşturun — Öncelik: yüksek

AI görüntü işleme çıktısı hazırdır; ancak backend bu sonucu alan bir endpoint/DTO sunmuyor. `POST /api/Telemetry` bu amaçla kullanılmamalıdır; yalnızca sensör telemetrisi içindir.

- [ ] Örneğin `POST /api/Analiz/goruntu-sonucu` endpoint'ini ve DTO'sunu oluşturun.
- [ ] DTO şu alanları kabul etmelidir: `seraId`, `bitkiEvresi`, `hastalikDetaylari[].hastalikAdi`, `hastalikDetaylari[].guvenSkoru`, `domatesDetaylari`, `fotografYolu`, `analizZamani`, `guvenEsigi`.
- [ ] `guvenSkoru` değerinin yüzde (`0–100`) olduğu doğrulansın ve `Bitki_Hastalik.hastalikOrani` alanına aynı anlamla kaydedilsin.
- [ ] `seraId` FK'si, zorunlu alanlar, güven skoru ve fotoğraf yolu doğrulansın; hastalık/bildirim kayıtlarının ne zaman oluşturulacağı belirlensin.

**Çözülmezse:** AI sonucu yalnızca yerelde JSON dosyası olarak kalır; hastalık geçmişi, bildirimler ve ilaçlama iş akışı otomatik başlayamaz.

#### 6. SignalR ve CORS güvenliğini tamamlayın — Öncelik: orta

- [ ] Geliştirme dışındaki ortamlarda `SetIsOriginAllowed(_ => true)` yerine izinli frontend adreslerini açıkça tanımlayın.
- [ ] `AllowCredentials()` yalnızca gerekli ve belirli origin'lerle kullanılmalı; kimlik doğrulama/izin gereksinimleri netleştirilmelidir.
- [ ] Frontend'e hub URL'si (`/serahub`) ve `ReceiveTelemetry` olay şeması belgelenmelidir.

**Çözülmezse:** Her web sitesi API/SignalR bağlantısı açmayı deneyebilir; canlı veri yetkisiz kaynaklara sızabilir veya tarayıcı tarafında CORS bağlantı hataları yaşanır.

## 6. ESP32 ekibi kontrol listesi

- [ ] Wi-Fi bağlantısı tamamlanmadan ölçüm göndermeyin; bağlantı kopunca yeniden bağlanma stratejisi kullanın.
- [ ] API adresini yapılandırılabilir tutun. Fiziksel cihazdan `localhost` kullanılamaz; bilgisayarın yerel ağ IP'si kullanılmalıdır.
- [ ] `POST /api/Sensors/save` çağrısında `Content-Type: application/json` başlığını gönderin.
- [ ] Her ölçümde `seraId`, `ortamSicakligi`, `ortamNemi`, `toprakNemi` alanlarının tamamını gönderin.
- [ ] HTTP başarı kodu dışındaki yanıtları ve ağ hatalarını seri porta loglayın.
- [ ] Ağ kesintilerinde ölçümleri sınırlı bir kuyrukta saklayıp tekrar göndermeyi planlayın; sonsuz tekrar döngüsünden kaçının.
- [ ] Sensör değerlerini göndermeden önce birimlerini doğrulayın: sıcaklık °C, nem yüzde `0–100`.
- [ ] Gerçek cihazla testten önce Postman/curl ile aynı JSON'un API tarafından kaydedildiğini doğrulayın.

## 7. Frontend ekibi kontrol listesi

- [x] Ortak `wwwroot/js/api.js` istemcisi, GET/POST hata kontrolüyle birlikte kullanılıyor.
- [x] Aşağıdaki ekranlar mevcut endpointlere bağlandı: ana sayfa (`Sensors/history`, `Telemetry/health`, ilaçlama, hastalık, bildirim), sensörler, grafikler, bitki evreleri, hastalıklar, ilaçlama ve giriş/kayıt.
- [x] Ana sayfa, sensörler ve grafikler `https://localhost:7266/serahub` üzerinden `ReceiveTelemetry` olayını dinliyor; otomatik yeniden bağlanma etkin.
- [ ] `API_BASE_URL` şu an `api.js` içinde sabit `https://localhost:7266` değeridir. Geliştirme/üretim için yapılandırılabilir hâle getirin; bağlantı adresi değiştiğinde bu dosyayı güncelleyin.
- [ ] SignalR canlı iletisi `sectionId`, `temperature`, `humidity`, `soilMoisture`, `recordedAt` alanlarını kullanıyor. Kalıcı sensör geçmişi ise `seraId`, `ortamSicakligi`, `ortamNemi`, `toprakNemi` alanlarını kullanıyor. Backend tek sözleşmeyi seçene kadar bu dönüşümün bilinçli ve test edilmiş olduğunu doğrulayın.
- [ ] Frontend `dotnet build` başarılıdır; backend derlenmeden canlı endpoint/SignalR testi yapılamaz.
- [ ] Yükleniyor, boş veri ve hata durumlarını her liste/grafik ekranında gösterin.
- [ ] Sensör geçmişini zaman sırasına göre grafikte gösterin; backend sıralamasını istemci tarafında körü körüne varsaymayın.
- [ ] Başarılı kayıt sonrası API yanıtını ve hata mesajlarını kullanıcıya uygun biçimde gösterin.
- [ ] Kullanıcı şifresini tarayıcıda düz metin olarak saklamayın; backend kimlik doğrulaması tamamlanana kadar oturum yönetimini geçici olarak açıkça işaretleyin.
- [ ] CORS hatası veya sertifika hatası görünürse API URL'si, backend CORS politikası ve HTTPS sertifikasını birlikte kontrol edin.
- [ ] API için TypeScript kullanılıyorsa ortak türleri/DTO'ları backend'in yayınladığı sözleşmeyle eşleştirin.

## 8. Yapay zekâ ekibi kontrol listesi

- [ ] Görüntü analizi sonucunu backend'e doğrudan göndermek için önce backend ekibiyle endpoint ve DTO üzerinde anlaşın.
- [ ] Backend/veritabanı ekibi `Bitki_Hastalik.hastalikOrani` alanının yüzde (`0–100`) formatını onaylasın.
- [ ] `fuzzy_sistem.py` içindeki örnek sabit JSON'u API'den alınan canlı verilerle değiştirmeden önce backend'in ideal değerler ve güncel sensör verisi endpoint'lerini tamamlamasını bekleyin.
- [ ] Karar çıktılarını (sulama süresi, fan seviyesi, ısıtma kararı) backend'e hangi endpoint üzerinden ileteceğinizi ve ESP32'nin bunları nasıl okuyacağını yazılı olarak kararlaştırın.

AI'nin mevcut görüntü işleme çıktısı `1_AI_Python/Goruntu_Isleme/README.md` içinde belgelenmiştir. Backend `POST /api/Analiz/goruntu-sonucu` veya eşdeğer endpoint'i yayınlamadan AI koduna HTTP isteği eklenmemelidir.

## 9. Uçtan uca kabul testi

Bu senaryo geçmeden proje "entegre" kabul edilmemelidir:

1. Veritabanında `seraId: 1` için geçerli sera ve aktif bitki evresi bulunur.
2. ESP32 veya test istemcisi bir sensör ölçümünü `POST /api/Sensors/save` ile gönderir.
3. Backend kaydı veritabanına yazar ve hata durumunda başarısız yanıt döner.
4. `GET /api/Sensors/history?limit=1` son kaydı döndürür.
5. Frontend `https://localhost:7214` adresinde açılır; `https://localhost:7266` API'sine bağlanarak son kaydı ana sayfa, sensörler ve grafik ekranında gösterir.
6. `POST /api/Telemetry` sonrası frontend açıkken `ReceiveTelemetry` olayı gelir; kartlar/grafik yenilenmeden güncellenir ve `GET /api/Telemetry/health` cihazı çevrimiçi gösterir.
7. Giriş/kayıt ve ilaçlama ekranları sırasıyla ilgili POST endpointlerinden başarılı veya anlaşılır hata yanıtı alır; eklenen ilaçlama kaydı geçmişte görünür.
8. Yapay zekâ analizi, `seraId` ile geçerli JSON üretir; backend endpoint'i hazır olduğunda aynı sonuç üzerinde anlaşılmış endpoint'e gönderilir ve hastalık/bildirim kaydı oluşur.

## 10. Teslimden önce ortak kontrol

- [ ] Boş veritabanında kurulum adımları başka bir ekip üyesi tarafından tekrarlanabildi.
- [ ] Backend Swagger'da çalışıyor ve örnek istekler başarılı.
- [ ] Frontend gerçek API'ye bağlanıyor; mock veri ile sınırlı değil.
- [ ] ESP32 gerçek ağ adresiyle ölçüm gönderiyor.
- [ ] Yapay zekâ çıktısının alanları, yayınlanmış backend DTO'su ile karşılaştırılıp uyumlu bulundu.
- [ ] Şifreler, bağlantı metinleri ve gizli anahtarlar Git'e eklenmedi.
- [ ] Her ekip yaptığı yeni sözleşme değişikliğini bu belgeye ve ilgili kişilere bildirdi.
