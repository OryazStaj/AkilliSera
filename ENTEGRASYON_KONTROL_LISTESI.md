# 🌿 Akıllı Sera – Entegrasyon Kontrol Listesi

Bu belge; Frontend, Backend, ESP32, Yapay Zeka ve Veritabanı ekiplerinin ortak veri sözleşmesini ve entegrasyon durumunu takip eder.

> **Son Güncelleme:** 2026-08-17 (Tüm modüller incelendi, eksikler belgelendi)

---

## 📊 1. Modül Durum Özeti

| Modül | Durum | Güncel Not |
|---|:---:|---|
| **Veritabanı (SQL)** | 🟢 Hazır | `setup-all.ps1` ile boş DB oluşturma ve 7 SQL scriptinin yüklenmesi otomatikleştirildi. |
| **Backend (.NET 8)** | 🟢 Hazır | `0 Hata` ile derleniyor. Swagger, EF Core, Telemetry ve SignalR (`/serahub`) devrede. |
| **Frontend (.NET 9)** | 🟡 Kısmi | Razor Pages çalışıyor. Hastalık sayfası `ReceivePlantAnalysis` sinyalini dinlemiyor. Bildirim sayfası eksik. |
| **AI (Python)** | 🟡 Kısmi | Kamera servisi ve YOLO tespiti çalışıyor. `fuzzy_sistem.py` HTTP API olarak sunulmadı; Backend'e bağlanmıyor. |
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

### C. Yapay Zeka & Görüntü İşleme
- [x] `kamera_servisi.py` → IP kamera veya simülasyon modunda görüntü alıyor, `kamera_anlik.jpg` üzerine yazıyor.
- [x] YOLO modelleri (`model.onnx`, `model_domates.onnx`) görüntüyü analiz edip hastalık ve domates tespiti yapıyor.
- [x] Analiz sonucu `POST /api/Analiz/goruntu-sonucu` ile Backend'e gönderiliyor (uçtan uca test edildi).

### D. Frontend
- [x] Merkezi `api.js` üzerinden HTTP hata yönetimi bağlandı.
- [x] `sensorler.cshtml` → `ReceiveTelemetry` SignalR dinleyicisi aktif; kayıtlı veri `Offline` rozetiyle gösteriliyor.
- [x] `Index.cshtml` → ESP32 canlılık durumu (`/api/Telemetry/health`) gösteriliyor.
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

### 🤖 Yapay Zeka Ekibi — [`1_AI_Python/Bulanik_Mantik/fuzzy_sistem.py`](1_AI_Python/Bulanik_Mantik/fuzzy_sistem.py)

- [ ] **`fuzzy_sistem.py` HTTP API olarak sarmalanmalı (Flask/FastAPI):**
  - Şu an script düz `print()` ile çalışıyor; Backend'den HTTP isteği alamıyor.
  - `POST http://127.0.0.1:5000/api/fuzzy/calculate` endpoint'i oluşturulmalı.
  - Backend `FuzzyIntegrationService.cs` bu endpoint'e bağlanmaya hazır; Python tarafı eksik.

- [ ] **`requirements.txt` güncellenmeli:**
  - `flask` veya `fastapi` + `uvicorn` bağımlılıkları eklenmeli.

---

### 🌐 Frontend Ekibi — [`4_Frontend_Web/Pages/`](4_Frontend_Web/Pages/)

- [ ] **`Hastalik.cshtml` sayfasına `ReceivePlantAnalysis` SignalR dinleyicisi eklenmeli:**
  - Kamera analizi geldiğinde sayfa yenilenmeden yeni hastalık satırları tabloya eklenmiyor.

- [ ] **Bildirim sayfası eksik:**
  - Backend `GET /api/Bildirim/liste` çalışıyor; Frontend'de bildirimleri gösteren sayfa yok.

- [ ] **`kontrol.cshtml` sayfasında aktüatör (vana/fan/ısıtıcı) durumu dinamik gösterilmeli:**
  - Şu an sayfa statik; ESP32'den gelen yanıttaki `normalValve`, `fan`, `heater` değerleri gösterilmiyor.

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
```json
{
  "kararlar": {
    "sulama": { "sure": 12.5, "karar": "Uzun Sulama" },
    "havalandirma": { "seviye": 75.0, "karar": "Yüksek" },
    "isitma": { "seviye": 22.0, "karar": "Ilık" }
  }
}
```

---

## 🧪 5. Uçtan Uca Doğrulama Testi

1. [x] `.\setup-all.ps1` sıfır ortamda çalıştırıldığında tüm modüllerin yeşil tamamlanması.
2. [x] Backend ayağa kalktığında Swagger üzerinden `POST /api/Telemetry` ile veri basılabilmesi.
3. [x] Veritabanına kaydın yazıldığının `GET /api/Sensors/history` ile doğrulanması.
4. [x] `kamera_servisi.py` çalıştırıldığında analiz sonucunun `POST /api/Analiz/goruntu-sonucu` ile veritabanına kaydedilmesi.
5. [ ] ESP32 donanımının düzeltilmiş endpoint ile yerel ağ üzerinden Backend'e veri iletmesi.
6. [ ] `fuzzy_sistem.py` Flask/FastAPI ile sarmalandıktan sonra Backend'in bulanık mantık kararlarını alması.
7. [ ] `Hastalik.cshtml` sayfasının yeni kamera analizi geldiğinde SignalR ile anlık güncellenmesi.

