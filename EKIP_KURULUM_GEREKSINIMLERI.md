# 🌱 Akıllı Sera - Hızlı Kurulum Kılavuzu

Bu rehber, projenin tüm modüllerini (AI, SQL Veritabanı, Backend ve Frontend) tek komutla kendi bilgisayarınızda hazır hale getirmeniz için hazırlanmıştır.

---

## 🛠️ 1. Ön Gereksinimler (Sadece 1 Kez Kurulacaklar)

Projeyi çalıştırmadan önce bilgisayarınızda şu temel programların kurulu olması gerekir.

> 💡 **Zaten kurulu olabilir!** Aşağıdaki kontrolleri önce yapın, yoksa kurulum adımlarına geçin.

### ✅ Hızlı Kontrol — Neye İhtiyacın Var?

| Program | Zaten Varsa... | Kontrol Komutu |
|---------|---------------|----------------|
| **Git** | GitHub Desktop kurulduysa Git de gelir | `git --version` |
| **.NET 8 & 9** | Visual Studio 2022 kurulduysa büyük ihtimalle vardır | `dotnet --list-sdks` |
| **Python 3.11+** | Daha önce Python kurduysanız sürümü kontrol edin | `python --version` |
| **SQL Server** | Visual Studio ile "Data Storage" workload kurduysanız LocalDB gelir | SSMS'i açıp bağlanmayı deneyin |

**PowerShell'i açıp bu komutları çalıştırın. Sonuç çıkıyorsa o program zaten kurulu, o adımı atlayabilirsiniz.**

---

### 1️⃣ Git

📥 **İndir:** https://git-scm.com/download/win

- Sayfaya girince indirme otomatik başlar
- Kurulum sihirbazında tüm seçenekleri varsayılan bırakın, sadece **Next > Next > Install** tıklayın
- Kurulumu doğrulamak için PowerShell'de:
  ```powershell
  git --version
  ```

---

### 2️⃣ .NET SDK (8.0 ve 9.0)

📥 **.NET 8.0 İndir:** https://dotnet.microsoft.com/download/dotnet/8.0
📥 **.NET 9.0 İndir:** https://dotnet.microsoft.com/download/dotnet/9.0

- Her iki sayfada da **"SDK x64"** yazan Windows yükleyicisini indirin
- İndirilen `.exe` dosyasını çalıştırın, kurulum otomatik tamamlanır
- Kurulumu doğrulamak için PowerShell'de:
  ```powershell
  dotnet --list-sdks
  ```

---

### 3️⃣ Python 3.11+

📥 **İndir:** https://www.python.org/downloads/

- Sayfadaki sarı **"Download Python 3.x.x"** butonuna tıklayın
- İndirilen `.exe` dosyasını çalıştırın
- ⚠️ **ÖNEMLİ:** Kurulum ekranının en altındaki **"Add Python to PATH"** kutucuğunu mutlaka işaretleyin!
- Ardından **"Install Now"** tıklayın
- Kurulumu doğrulamak için PowerShell'de:
  ```powershell
  python --version
  ```

---

### 4️⃣ MS SQL Server Express + SSMS

**Adım 1 — SQL Server Express'i İndirin:**
📥 **İndir:** https://www.microsoft.com/tr-tr/sql-server/sql-server-downloads

- Sayfayı aşağı kaydırın, **"Express"** kutusunun altındaki **"Şimdi indir"** butonuna tıklayın
- İndirilen `.exe` dosyasını çalıştırın
- Kurulum türü olarak **"Temel (Basic)"** seçin → **"Kabul Et"** → **"Yükle"**

**Adım 2 — SSMS'i İndirin** *(Veritabanını görsel yönetmek için):*
📥 **İndir:** https://aka.ms/ssmsfullsetup

- İndirilen `.exe` dosyasını çalıştırın, **"Yükle"** tıklayın

---



## 🚀 2. Tek Komutla Otomatik Kurulum (Önerilen)

Projeyi indirdikten sonra kök dizinde **PowerShell** açın ve tek komut çalıştırın:

```powershell
.\setup-all.ps1
```

### 🤖 Bu Script Sizin İçin Ne Yapar?
* ✅ **AI (Python):** `requirements.txt` içindeki yapay zeka paketlerini yükler.
* ✅ **SQL Veritabanı:** Bilgisayarınızdaki SQL Server'ı (`.` veya `.\SQLEXPRESS`) otomatik bulur, `AkilliSeraDB` veritabanını ve tüm tabloları oluşturur.
* ✅ **Bağlantı Ayarı:** Backend `appsettings.json` dosyasındaki veritabanı adresini sizin bilgisayarınıza göre otomatik ayarlar.
* ✅ **Backend & Frontend:** .NET projelerinin paketlerini geri yükler ve hatasız derler.

---

## 🧪 3. Kurulum Sonrası Sistemi Test Etme (Adım Adım)

Kurulum tamamlandıktan sonra donanım (ESP32) bağlı olmasa bile sistemi uçtan uca test edebilirsiniz:

### 1️⃣ Adım: Servisleri Başlatın (3 Ayrı Terminal)

* **Terminal 1 (AI - Bulanık Mantık):**
  ```powershell
  cd 1_AI_Python
  python Bulanik_Mantik/fuzzy_sistem.py
  ```
  *(Çıktı: `Running on http://0.0.0.0:5000`)*

* **Terminal 2 (Backend API):**
  ```powershell
  cd 2_Backend_CSharp
  dotnet run --launch-profile https
  ```
  *(Çıktı: `Now listening on: https://localhost:7266` — Tarayıcıda Swagger açılır)*

* **Terminal 3 (Web Arayüzü):**
  ```powershell
  cd 4_Frontend_Web
  dotnet run --launch-profile https
  ```
  *(Çıktı: `Now listening on: https://localhost:7214` — Tarayıcıda paneli açın)*

---

### 2️⃣ Adım: Canlı Test Verisi Gönderin (4. Terminal)

Yeni bir PowerShell terminali açın ve sisteme sanal bir sensör gibi veri paketi yollayın:

```powershell
$testVerisi = @{
    SeraId = 1
    OrtamSicakligi = 34.0
    OrtamNemi = 40.0
    ToprakNemi = 25.0
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7266/api/telemetry" -Method POST -ContentType "application/json" -Body $testVerisi -SkipCertificateCheck
```

### ✅ Neleri Gözlemlemelisiniz?
1. **Terminalde:** `actuators` başlığı altında sulama/vana ve fanın açıldığına dair JSON yanıtı döner.
2. **Backend Terminalinde:** Fuzzy API'den karar alındığına dair loglar düşer.
3. **Web Tarayıcısında (`https://localhost:7214`):** Sayfa yenilemeye gerek kalmadan sensör değerleri ve aktüatör durumları SignalR ile canlı güncellenir!

> 📖 **Daha fazla senaryo ve detaylı testler için:** [GENEL_SISTEM_TEST_KILAVUZU.md](GENEL_SISTEM_TEST_KILAVUZU.md) belgesini inceleyebilirsiniz.

---

## 📌 4. Ekip Bazlı Manuel Komutlar (İsteğe Bağlı)

* **AI Ekibi:** `cd 1_AI_Python` ➔ `pip install -r requirements.txt`
* **Backend Ekibi:** `cd 2_Backend_CSharp` ➔ `dotnet build`
* **Frontend Ekibi:** `cd 4_Frontend_Web` ➔ `dotnet build`
* **ESP32 Ekibi:** Gerekli kütüphaneler için [3_ESP32_Embedded/libraries.txt](3_ESP32_Embedded/libraries.txt) dosyasını inceleyin.
