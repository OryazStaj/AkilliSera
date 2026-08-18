# 🧪 Akıllı Sera - Genel Sistem Test Kılavuzu (Uçtan Uca)

Bu kılavuz; kodlarda **hiçbir değişiklik yapmadan**, sadece terminal ve tarayıcı kullanarak tüm sistemi (Yapay Zeka, Backend, Veritabanı, Frontend ve ESP32 Simülasyonu) adım adım test etmeniz için hazırlanmıştır.

---

## 🏗️ Sistem Mimarisi ve Portlar

| Bileşen | Görev | Port / Adres | Test Şekli |
| :--- | :--- | :--- | :--- |
| **1. AI Python (Flask)** | Bulanık Mantık Karar Motoru | `http://localhost:5000` | Terminal (POST İsteği) |
| **2. Backend API (.NET)** | Veri İşleme, DB Kaydı, SignalR | `https://localhost:7266` (veya `http://localhost:5108`) | Swagger / Terminal |
| **3. Web Frontend (.NET)** | Canlı Gösterge Paneli | `https://localhost:7214` (veya `http://localhost:5149`) | Tarayıcı (Canlı Takip) |
| **4. MS SQL Server** | Kalıcı Veri Depolama | `localhost` (`AkilliSeraDB`) | SSMS veya PowerShell |

---

## 🚀 1. Hazırlık: Tüm Servisleri Terminalden Başlatma

Projeyi baştan sona test etmek için **3 ayrı terminal** penceresi açın:

### 🔹 Terminal 1: AI (Bulanık Mantık) Servisini Başlatın
```powershell
cd 1_AI_Python
python Bulanik_Mantik/fuzzy_sistem.py
```
> Beklenen Çıktı: `Running on http://0.0.0.0:5000`

---

### 🔹 Terminal 2: Backend API Servisini Başlatın
```powershell
cd 2_Backend_CSharp
dotnet run --launch-profile https
```
> Beklenen Çıktı: `Now listening on: https://localhost:7266` ve `Swagger` hazır.

---

### 🔹 Terminal 3: Frontend Web Arayüzünü Başlatın
```powershell
cd 4_Frontend_Web
dotnet run --launch-profile https
```
> Beklenen Çıktı: `Now listening on: https://localhost:7214`
> Tarayıcınızda `https://localhost:7214` adresini açın (Dashboard / Kontrol sayfası).

---

## 🧪 2. Modül Bazlı Hızlı Testler

Yeni bir **4. Terminal** açarak aşağıdaki test komutlarını sırayla çalıştırabilirsiniz:

---

### A) 🧠 Yapay Zeka (Fuzzy Logic) Tek Başına Test

Backend'e gerek kalmadan sadece Python API'sinin hesaplamasını test etmek için:

```powershell
$fuzzyPayload = @{
    bitki = @{
        bitkiAdi = "Domates"; evreAdi = "Fide"
        minToprakNemi = 60; maxToprakNemi = 75
        minOrtamNemi = 65;  maxOrtamNemi = 75
        gunduzMinSicaklik = 21; gunduzMaxSicaklik = 26
        geceMinSicaklik = 16;   geceMaxSicaklik = 19
    }
    anlikVeriler = @{
        toprakNemi = 35  # Kuru
        ortamNemi = 45   # Düşük
        sicaklik = 32    # Sıcak
    }
    zaman = @{ saat = 14 }
} | ConvertTo-Json -Depth 5

Invoke-RestMethod -Uri "http://localhost:5000/api/fuzzy/calculate" -Method POST -ContentType "application/json" -Body $fuzzyPayload
```

✅ **Beklenen Çıktı:** Sulama süresi, fan seviyesi ve ısıtma kararını içeren JSON cevabı.

---

### B) 🌐 Backend API Testi (Swagger Üzerinden)

1. Tarayıcınızda açın: **`https://localhost:7266/swagger`**
2. `GET /api/BitkiEvre/liste` endpoint'ine tıklayın ➔ **Try it out** ➔ **Execute**
3. Veritabanındaki bitki evrelerinin (`Tohum`, `Fide`, `Olgunlasma`) listelendiğini doğrulayın.

---

### C) 📡 ESP32 Donanımını Simüle Etme (Uçtan Uca Entegrasyon Testi)

ESP32 kartınız bağlı olmadan, sanal bir sensör gibi Backend'e veri paketi gönderin. Bu komut çalıştığında:
1. Veri SQL veritabanına (`Sensor_Loglari`) kaydedilir.
2. Backend otomatik olarak Python Fuzzy API'yi çağırır.
3. Alınan aktüatör kararları yanıt olarak döner.
4. SignalR ile açık olan Web arayüzü anında güncellenir!

```powershell
$telemetryData = @{
    SeraId = 1
    OrtamSicakligi = 33.5
    OrtamNemi = 42.0
    ToprakNemi = 28.0
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7266/api/telemetry" -Method POST -ContentType "application/json" -Body $telemetryData -SkipCertificateCheck
```

✅ **Beklenen Çıktı (Terminal):**
```json
{
  "status": "success",
  "message": "Telemetri işlendi.",
  "actuators": {
    "normalValve": true,
    "fan": true,
    "heater": false
  }
}
```

---

### D) 🗄️ Veritabanı Doğrulama Testi

Gönderilen verinin SQL Server'a başarıyla yazıldığını kontrol etmek için PowerShell'de:

```powershell
sqlcmd -S . -d AkilliSeraDB -Q "SELECT TOP 5 LogID, SeraID, OrtamSicakligi, OrtamNemi, ToprakNemi, KayitZamani FROM Sensor_Loglari ORDER BY LogID DESC;"
```
*(Eğer SQLEXPRESS kullanıyorsanız `-S .\SQLEXPRESS` yazın)*

✅ **Beklenen Çıktı:** Az önce gönderdiğiniz `33.5` sıcaklık ve `28.0` toprak nemi değerlerinin tabloda en üstte listelenmesi.

---

## 📊 3. Canlı UI (Frontend) Test Senaryosu

1. Tarayıcıda **`https://localhost:7214`** adresini açın.
2. Terminalden farklı uç durumlar için telemetri gönderip ekrandaki değişimi izleyin:

| Test Senaryosu | Gönderilecek Değerler | Beklenen Aktüatör Durumu |
| :--- | :--- | :--- |
| **🚨 Aşırı Kurak ve Sıcak** | Toprak: `15`, Sıcaklık: `36`, Nem: `30` | Vana: **AÇIK**, Fan: **AÇIK (%100)**, Isıtıcı: **KAPALI** |
| **❄️ Soğuk ve Islak** | Toprak: `80`, Sıcaklık: `12`, Nem: `85` | Vana: **KAPALI**, Fan: **KAPALI**, Isıtıcı: **AÇIK** |
| **🌿 İdeal Durum** | Toprak: `68`, Sıcaklık: `24`, Nem: `70` | Vana: **KAPALI**, Fan: **KAPALI/DÜŞÜK**, Isıtıcı: **KAPALI** |

---

## 🔍 Olası Hata Çözümleri

| Hata | Olası Neden | Çözüm |
| :--- | :--- | :--- |
| `Fuzzy API yanıt vermedi, sabit eşik kullanılıyor` | Python Flask servisi çalışmıyor | Terminal 1'de `python Bulanik_Mantik/fuzzy_sistem.py` çalıştırın. |
| `Veritabanı bağlantı hatası (SqlException)` | SQL Server servisi kapalı veya `appsettings.json` sunucu adı farklı | `setup-all.ps1` scriptini tekrar çalıştırın veya SQL Server servisinin çalıştığından emin olun. |
| `SSL / Certificate Warning` | Geliştirme SSL sertifikası uyarısı | PowerShell komutlarına `-SkipCertificateCheck` ekleyin veya `dotnet dev-certs https --trust` çalıştırın. |
