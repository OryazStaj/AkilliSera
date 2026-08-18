# 🌿 Akıllı Sera – Entegrasyon Kontrol Listesi

Bu belge; Frontend, Backend, ESP32, Yapay Zeka ve Veritabanı ekiplerinin ortak veri sözleşmesini ve entegrasyon durumunu takip eder.

> **Son Güncelleme:** 2026-08-18 (Fuzzy Logic + Backend entegrasyonu tamamlandı)

---

## 📊 1. Modül Durum Özeti

| Modül | Durum | Güncel Not |
|---|:---:|---|
| **Veritabanı (SQL)** | 🟢 Hazır | `setup-all.ps1` ile boş DB oluşturma ve 7 SQL scriptinin yüklenmesi otomatikleştirildi. |
| **Backend (.NET 8)** | 🟢 Hazır | `0 Hata` ile derleniyor. Fuzzy servisi entegre edildi; aktif bitki evresine göre karar üretiyor. |
| **Frontend (.NET 9)** | 🟡 Kısmi | Razor Pages çalışıyor. `Hastalik.cshtml` SignalR dinleyicisi eksik. Bildirim sayfası yok. `kontrol.cshtml` statik. |
| **AI (Python)** | 🟢 Hazır | `fuzzy_sistem.py` Flask API olarak çalışıyor. `POST /api/fuzzy/calculate` endpoint aktif. |
| **ESP32 (C++)** | 🔴 Bağlı Değil | Yanlış endpoint (`/api/sera`), yanlış JSON alanları (`sicaklik`, `nem`), yanıt parse'ı yanlış. |

---

## 🛠️ 2. Tamamlanan Entegrasyon Maddeleri

### A. Veritabanı ve Konfigürasyon
- [x] `AkilliSeraDB` veritabanı kurulumu ve 7 SQL dosyasının bağımlılık sırası doğrulandı.
- [x] `appsettings.json` ve `appsettings.Development.json` bağlantı dizeleri dinamik yapılandırıldı.
- [x] Başlangıç verileri (`Bitki_Evreleri`, `Sera_Durum`) varsayılan olarak yükleniyor.

### B. Backend ve SignalR
- [x] `IlaclamaController` ve `KullaniciController` derleme hataları giderildi.
- [x] SignalR Hub (`/serahub`) ve `HealthCheckService` servisleri `Program.cs` içine kaydedildi.
- [x] `POST /api/Telemetry` → Veritabanına kaydedip `ReceiveTelemetry` ile SignalR'a yayınlıyor.
- [x] `POST /api/Analiz/goruntu-sonucu` → `Bitki_Hastalik`, `Bildirim` ve `Kamera_Loglari` kayıtlarını oluşturup `ReceivePlantAnalysis` ile yayınlıyor.
- [x] `GET /api/Kullanici/liste` şifre bilgisini gizleyerek güvenli döndürüyor.
- [x] `GET /api/Sensors/history` en yeni kayıtlar üstte olacak şekilde çalışıyor.
- [x] `GET /api/Bildirim/liste` bildirimleri döndürüyor.
- [x] `FuzzyIntegrationService` → `TelemetryController`'a inject edildi; aktif bitki evresi DB'den çekilerek Fuzzy API'ye doğru formatta (`bitki + anlikVeriler + zaman`) gönderiliyor.
- [x] Fuzzy API çevrimdışıysa veya aktif evre tanımlı değilse sabit eşik fallback devreye giriyor.

### C. Yapay Zeka & Görüntü İşleme
- [x] `kamera_servisi.py` → IP kamera veya simülasyon modunda görüntü alıyor, `kamera_anlik.jpg` üzerine yazıyor.
- [x] YOLO modelleri (`model.onnx`, `model_domates.onnx`) görüntüyü analiz edip hastalık ve domates tespiti yapıyor.
- [x] Analiz sonucu `POST /api/Analiz/goruntu-sonucu` ile Backend'e gönderiliyor (uçtan uca test edildi).
- [x] `fuzzy_sistem.py` sözdizimi hatası (satır 789 `IndentationError`) ve çift `toprak` Antecedent tanımı giderildi.
- [x] `fuzzy_sistem.py` Flask HTTP API olarak sarmalandı → `POST http://127.0.0.1:5000/api/fuzzy/calculate` çalışıyor.
- [x] `requirements.txt`'e `flask` bağımlılığı eklendi.

### D. Frontend
- [x] Merkezi `api.js` üzerinden HTTP hata yönetimi bağlandı.
- [x] `sensorler.cshtml` → `ReceiveTelemetry` SignalR dinleyicisi aktif; kayıtlı veri `Offline` rozetiyle gösteriliyor.
- [x] `Index.cshtml` → ESP32 canlılık durumu (`/api/Telemetry/health`) gösteriliyor; `GET /api/Bildirim/liste` ile bildirim listesi yükleniyor.
- [x] `grafikler.cshtml` → Geçmiş sensör verileri grafik olarak çiziliyor.

---

## 📋 3. Yapılacaklar Listesi (Kalan Eksikler)

### 🔌 ESP32 Ekibi — [`3_ESP32_Embedded/main.cpp`](3_ESP32_Embedded/main.cpp)

- [ ] **Hedef Endpoint düzeltilmeli:**
  - ❌ Şu an: `http://192.168.1.X:5000/api/sera`
  - ✅ Olması gereken: `http://<BACKEND_IP>:5108/api/Telemetry`

- [ ] **JSON alan adları düzeltilmeli:**
  - ❌ Şu an gönderilen: `sicaklik`, `nem`, `toprak_nemi`, `isik`
  - ✅ Backend'in beklediği: `seraId`, `ortamSicakligi`, `ortamNemi`, `toprakNemi`

- [ ] **Yanıt (response) parse'ı düzeltilmeli:**
  - ❌ Şu an okunan: `su_pompasi`, `fan`, `aydinlatma`
  - ✅ Backend'in döndürdüğü: `normalValve`, `treatmentValve`, `fan`, `heater`

- [ ] **WiFi bilgileri (`WIFI_SSID` / `WIFI_PASSWORD`) seranın gerçek ağ bilgileriyle doldurulmalı.**

---

### 🌐 Frontend Ekibi — [`4_Frontend_Web/Pages/`](4_Frontend_Web/Pages/)

- [ ] **`Hastalik.cshtml` sayfasına `ReceivePlantAnalysis` SignalR dinleyicisi eklenmeli:**
  - Kamera analizi geldiğinde sayfa yenilenmeden yeni hastalık satırları tabloya eklenmiyor.

- [ ] **Bildirim sayfası eksik:**
  - Backend `GET /api/Bildirim/liste` çalışıyor; Frontend'de bildirimleri gösteren ayrı bir sayfa (`Bildirim.cshtml`) yok.
  - Not: `Index.cshtml` ana sayfada bildirim listesi kısmen gösteriliyor; bu tam sayfa değil.

- [ ] **`kontrol.cshtml` sayfasında aktüatör (vana/fan/ısıtıcı) durumu dinamik gösterilmeli:**
  - Şu an sayfa statik; `ReceiveTelemetry` SignalR olayıyla gelen `normalValve`, `fan`, `heater` değerleri gösterilmiyor.

---

## 🌐 4. Standart API Sözleşmesi

* **Backend Portları:** `http://localhost:5108` & `https://localhost:7266`
* **Swagger Dokümantasyonu:** `https://localhost:7266/swagger`
* **SignalR Hub Adresi:** `https://localhost:7266/serahub`

### Telemetry Gönderim Sözleşmesi (`POST /api/Telemetry`):
```json
{
  "seraId": 1,
  "ortamSicakligi": 26.5,
  "ortamNemi": 60.0,
  "toprakNemi": 35.0
}
```

### Backend Yanıtı — Aktüatör Emirleri:
```json
{
  "message": "Telemetri başarıyla kaydedildi ve yayınlandı.",
  "normalValve": false,
  "treatmentValve": false,
  "fan": false,
  "heater": false
}
```

### Görüntü Analiz Sözleşmesi (`POST /api/Analiz/goruntu-sonucu`):
```json
{
  "seraId": 1,
  "bitkiEvresi": "Filiz",
  "fotografYolu": "kamera_anlik.jpg",
  "hastalikDetaylari": [{ "hastalikAdi": "Late Blight", "guvenSkoru": 87.3 }],
  "aciklama": "15 hastalık tespiti yapıldı."
}
```

### Bulanık Mantık Sözleşmesi (`POST http://127.0.0.1:5000/api/fuzzy/calculate`):

**İstek (Backend → Python):**
```json
{
  "bitki": {
    "bitkiAdi": "Domates",
    "evreAdi": "Vejetatif",
    "minToprakNemi": 45, "maxToprakNemi": 70,
    "minOrtamNemi": 60,  "maxOrtamNemi": 80,
    "gunduzMinSicaklik": 20, "gunduzMaxSicaklik": 28,
    "geceMinSicaklik": 16,   "geceMaxSicaklik": 20
  },
  "anlikVeriler": { "toprakNemi": 38, "ortamNemi": 45, "sicaklik": 31 },
  "zaman": { "saat": 14 }
}
```

**Yanıt (Python → Backend):**
```json
{
  "bitki": "Domates",
  "evre": "Vejetatif",
  "kararlar": {
    "sulama":      { "sure": 22.78, "karar": "fazla sulama" },
    "havalandirma":{ "seviye": 50.0, "karar": "orta fan" },
    "isitma":      { "seviye": 20.0, "karar": "isi_dusur" }
  }
}
```

---

## 🧪 5. Uçtan Uca Doğrulama Testi

1. [x] `.\setup-all.ps1` sıfır ortamda çalıştırıldığında tüm modüllerin yeşil tamamlanması.
2. [x] Backend ayağa kalktığında Swagger üzerinden `POST /api/Telemetry` ile veri basılabilmesi.
3. [x] Veritabanına kaydın yazıldığının `GET /api/Sensors/history` ile doğrulanması.
4. [x] `kamera_servisi.py` çalıştırıldığında analiz sonucunun `POST /api/Analiz/goruntu-sonucu` ile veritabanına kaydedilmesi.
5. [x] `fuzzy_sistem.py` Flask API olarak ayağa kaldırıldığında `POST /api/fuzzy/calculate` endpoint'inin doğru JSON döndürmesi. *(Yerel test edildi: sulama=22.78sn, fan=%50, ısıtma=isi_dusur)*
6. [x] Backend'in Fuzzy API'yi çağırıp `normalValve/fan/heater` kararlarını Fuzzy çıktısına göre üretmesi. *(Derleme doğrulandı: 0 Hata)*
7. [ ] ESP32 donanımının düzeltilmiş endpoint ile yerel ağ üzerinden Backend'e veri iletmesi.
8. [ ] `Hastalik.cshtml` sayfasının yeni kamera analizi geldiğinde SignalR ile anlık güncellenmesi.
9. [ ] `kontrol.cshtml` sayfasının aktüatör durumunu `ReceiveTelemetry` ile dinamik göstermesi.
