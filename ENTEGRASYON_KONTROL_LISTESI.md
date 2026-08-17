# Akıllı Sera – Entegrasyon Kontrol Listesi

Bu belge; frontend, backend, ESP32, yapay zeka ve veritabanı ekiplerinin aynı veri sözleşmesiyle çalışması için minimum gereksinimleri tanımlar.

Guncelleme tarihi: 2026-08-17 (kod tabani uzerinden dogrulandi)

## 1. Mevcut durum ozeti

| Alan | Durum | Gerekli ilk is |
|---|---|---|
| Veritabani | SQL semasi ve dosyalar mevcut | Bos DB'de kurulum sirasi ve FK bagimliliklarini ekipce yeniden dogrulamak |
| Backend | Telemetry + SignalR kodu aktif, `dotnet build` basarili | Swagger + veritabani baglantisi ile calisma testlerini tamamlamak |
| Frontend | Razor Pages, API istemcisi ve SignalR baglantilari mevcut; `dotnet build` basarili | Backend ayaga kalkinca canli API + hub testlerini tekrar gecmek |
| ESP32 | Cihaz kodu var ve Wi-Fi/JSON POST uygulaniyor | Hedef endpoint ve JSON alanlarini backend ile ayni sozlesmeye getirmek |
| Yapay zeka | Goruntu analizi JSON cikti sozlesmesi hazir | Backend'de goruntu analizi kabul endpoint/DTO'su yayinlanmali |

## 2. Projeyi ayaga kaldirmak icin minimum sira

1. SQL Server'da `AkilliSeraDB` veritabanini olusturun.
2. SQL dosyalarini bagimlilik sirasiyla calistirin:
   1. `01_Sabit_Referans_Tablolari.sql`
   2. `02_Dinamik_Log_Tablolari.sql`
   3. `05_Dis_Ortam.sql`
   4. `06_Kullanici_ve_Bildirim_Tablolari.sql`
   5. `07_Ilaclama_Takip.sql`
   6. `03_Baslangic_Verileri.sql`
   7. `04_Stored_Prosedur_Tablolari.sql`
3. Backend baglanti metnini `ConnectionStrings:DefaultConnection` uzerinden ayarlayin.
4. `2_Backend_CSharp` altinda `dotnet build` calistirin ve once derleme hatalarini temizleyin.
5. `dotnet run` ile Swagger'in acildigini ve DB erisiminin calistigini dogrulayin.
6. Frontend ve ESP32 sadece bu dokumandaki endpoint/JSON alan adlariyla baglansin.

## 3. Ortak kurallar

- Gelistirmede backend adresleri: `http://localhost:5108` ve `https://localhost:7266`.
- Frontend gelistirme adresi: `https://localhost:7214`.
- JSON alan adlarinda `camelCase` kullanin.
- Tarih/saat formati ISO-8601 olmalidir.
- Hata durumunda istemciler HTTP kodunu ve hata govdesini gostermeli/loglamalidir.
- Yeni endpoint veya alan eklenirse bu dosya ayni gunde guncellenmelidir.

## 4. Veritabani ekibi kontrol listesi

- [ ] SQL dosyalarinin yukaridaki sirada, bos bir veritabaninda hatasiz calistigini ekip ici tekrar dogrulayin.
- [ ] `03_Baslangic_Verileri.sql` dosyasinin bagimli tablolardan sonra calistigini onaylayin.
- [ ] `04_Stored_Prosedur_Tablolari.sql` dosyasini tum tablolar olustuktan sonra calistirin.
- [ ] En az bir `Sera_Durum` ve bir `Bitki_Evreleri` kaydi oldugunu dogrulayin.
- [ ] `Sensor_Loglari`, `Bitki_Hastalik`, `Ilaclama_Takip`, `Bildirim`, `Dis_Ortam_Loglari` kolonlarini EF Core modelleriyle karsilastirin.
- [ ] Sema degisikligi oldugunda backend ekibine tablo/kolon tipi, null durumu ve FK bilgisini yazili bildirin.

## 5. Backend ekibi kontrol listesi

### A. Kritik derleme durumu

#### 1. Guncel build durumu (tamamlandi)

`dotnet build` backend tarafinda basarili.

Tamamlananlar:

- [x] `FinalCommandDto` ve `FuzzyDecisionDto` tipleri eklendi.
- [x] `FuzzyIntegrationService` namespace'i `Program.cs` kaydiyla (`AkilliSera_API.Services`) uyumlu hale getirildi.
- [x] Build tekrar calistirildi ve basarili sonuc alindi.

Not: Onceki namespace uyusmazligi (`TelemetryController`/`SeraHub`/`HealthCheckService`) cozulmus durumda.

#### 2. Baglanti metni ve konfigurasyon (kritik)

- [ ] `appsettings.Development.json` veya user-secrets icinde `ConnectionStrings:DefaultConnection` tanimlayin.
- [ ] `AkilliSeraDbContext.OnConfiguring` icindeki sabit connection string'i kaldirin.
- [ ] Build warning (`#warning` baglanti metni) temizlenmeli.

### B. Mevcut API sozlesmesi (dogrulandi)

- [x] `GET /api/Sensors/history?limit=...`
- [x] `POST /api/Sensors/save`
- [x] `GET /api/Kullanici/liste`
- [x] `POST /api/Kullanici/kayit-ol`
- [x] `POST /api/Kullanici/giris-yap`
- [x] `GET /api/BitkiEvre/liste`
- [x] `GET /api/Hastalik/liste`
- [x] `GET /api/Ilaclama/gecmis`
- [x] `POST /api/Ilaclama/ekle`
- [x] `GET /api/Bildirim/liste`
- [x] `GET /api/Telemetry/health`
- [x] SignalR hub `/serahub` ve `ReceiveTelemetry`

### C. Tamamlanmasi gereken backend isleri

- [ ] Entity'leri dogrudan donmek yerine DTO katmanina gecin.
- [ ] `Sensors/history` sonucunu `KayitZamani DESC` siralayin, `limit` icin ust sinir koyun.
- [ ] `DataBaseService` icindeki `try/catch + Console.WriteLine` yaklasimi yerine hatayi ust kata tasiyin; basarisiz kayitta sahte basari donmeyin.
- [ ] Kullanici sifresini hashleyin; `GET /api/Kullanici/liste` yanitindan sifreyi tamamen cikarin.
- [ ] CORS'u uretimde sadece izinli frontend origin'lerine sinirlayin.
- [ ] Swagger'a ornek istek/yanit ve hata senaryolari ekleyin.

### D. Telemetry ve SignalR uyumu

#### 3. Cift sensor sozlesmesi (yuksek)

Durum:

- [x] `POST /api/Telemetry` su an hem (`seraId`, `ortamSicakligi`, `ortamNemi`, `toprakNemi`) hem de (`sectionId`, `temperature`, `humidity`, `soilMoisture`) alanlarini kabul edecek sekilde esleme yapiyor.
- [ ] Proje genelinde tek kanonik sozlesme yazili olarak secilmeli.
- [ ] `POST /api/Sensors/save`, frontend SignalR verisi, Swagger ve ESP32 payload'i ayni sozlesmede birlestirilmeli.

#### 4. Telemetry kalici kayit (yuksek)

Durum:

- [x] `POST /api/Telemetry` veriyi `Sensor_Loglari` tablosuna kaydediyor.
- [x] DB kaydi hata verirse `500` donuyor.
- [x] SignalR yayini DB kaydindan sonra yapiliyor.

#### 5. Goruntu analizi endpoint'i (yuksek)

- [ ] `POST /api/Analiz/goruntu-sonucu` (veya esdeger) endpoint + DTO henuz yok.
- [ ] AI cikti alanlari (`seraId`, `bitkiEvresi`, `hastalikDetaylari`, `domatesDetaylari`, `fotografYolu`, `analizZamani`, `guvenEsigi`) backend tarafinda kabul edilmeli.

## 6. ESP32 ekibi kontrol listesi

Guncel kod durumu: `main.cpp` mevcut ve calisiyor, ancak su an C# API yerine Python endpoint'ine (`http://192.168.1.X:5000/api/sera`) veri gonderiyor.

- [x] Wi-Fi baglantisi kurulmadan veri gonderilmiyor.
- [x] `Content-Type: application/json` basligi gonderiliyor.
- [ ] Backend ile ortak endpoint'e gecis yapilmali (`/api/Sensors/save` veya ekipce secilen tek endpoint).
- [ ] JSON alan adlari backend sozlesmesiyle birebir uyumlu hale getirilmeli.
- [ ] Ag kopmalarinda kuyruklama/yeniden gonderim stratejisi eklenmeli.
- [ ] Sunucu adresi derleme sabiti yerine ortam/yapi konfigurasyonundan yonetilmeli.

## 7. Frontend ekibi kontrol listesi

- [x] Ortak `wwwroot/js/api.js` ile GET/POST hata kontrolu var.
- [x] Ana sayfa, sensorler, grafikler, bitki evreleri, hastaliklar, ilaclama, giris/kayit ekranlari API'ye bagli.
- [x] Ana sayfa/sensorler/grafikler `ReceiveTelemetry` dinliyor.
- [x] Yukleniyor, bos veri ve hata durumlari birden fazla ekranda gosteriliyor.
- [x] `4_Frontend_Web` icin `dotnet build` basarili.
- [ ] `API_BASE_URL` su an sabit (`https://localhost:7266`); ortam bazli konfigurasyona alinmali.
- [ ] Sensor gecmisi siralama varsayimi backend'de garanti altina alinmali (`DESC`) ve istemci tarafinda da dogrulanmali.

## 8. Yapay zeka ekibi kontrol listesi

- [x] Goruntu analizi JSON cikti sozlesmesi belgelenmis durumda.
- [ ] Backend goruntu analizi endpoint'i yayinlanmadan HTTP gonderimi eklenmemeli.
- [ ] `Bitki_Hastalik.hastalikOrani` icin yuzde (`0-100`) anlami backend + DB tarafinda resmi olarak kilitlenmeli.
- [ ] Bulanik mantik karar ciktilarinin backend ve ESP32 tarafinda hangi endpoint/formatla tasinacagi netlestirilmeli.

## 9. Uctan uca kabul testi

Bu senaryo gecmeden proje "entegre" kabul edilmemelidir:

1. Veritabani kurulum adimlari bos DB'de sorunsuz tamamlanir.
2. Backend `dotnet build` ve `dotnet run` hatasiz calisir.
3. Test istemcisi/ESP32 sensor verisini secilen kanonik endpoint'e yollar.
4. Kayit veritabanina yazilir ve `GET /api/Sensors/history?limit=1` ile geri okunur.
5. Frontend `https://localhost:7214` uzerinden acik kalirken SignalR `ReceiveTelemetry` olayi gorulur.
6. `GET /api/Telemetry/health` cihazi cevrimici olarak raporlar.
7. Giris/kayit ve ilaclama ekranlari hem basarili hem hatali yanitlari dogru gosterir.
8. AI sonucu backend goruntu endpoint'ine gonderilir ve ilgili hastalik/bildirim akisi tetiklenir.

## 10. Teslimden once ortak kontrol

- [ ] Bos veritabaninda kurulum adimlari baska ekip uyesi tarafindan tekrarlandi.
- [ ] Backend Swagger acildi ve kritik endpoint'ler test edildi.
- [ ] Frontend gercek API ile canli veri gosterebiliyor.
- [ ] ESP32 secilen backend endpoint'ine gercek ag uzerinden veri gonderiyor.
- [ ] AI cikti alanlari backend DTO'su ile birebir uyumlu.
- [ ] Gizli bilgiler (sifreler, baglanti metinleri, anahtarlar) repoya eklenmedi.
