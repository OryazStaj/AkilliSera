# 🌱 Akıllı Sera - Hızlı Kurulum Kılavuzu

Bu rehber, projenin tüm modüllerini (AI, SQL Veritabanı, Backend ve Frontend) tek komutla kendi bilgisayarınızda hazır hale getirmeniz için hazırlanmıştır.

---

## 🛠️ 1. Ön Gereksinimler (Sadece 1 Kez Kurulacaklar)

Projeyi çalıştırmadan önce bilgisayarınızda şu temel programların kurulu olması gerekir:

1. **Git**
2. **.NET SDK** (.NET 8.0 ve .NET 9.0)
3. **Python 3.11+** *(Kurulum sırasında "Add Python to PATH" kutucuğunu işaretleyin)*
4. **MS SQL Server** *(SQL Express veya Standart) + **SSMS***

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

## 🧪 3. Sistemi Test Etme (Hızlı Başlangıç)

Donanım veya ESP32 olmadan sistemi test etmek için:

1. **Backend'i Başlatın:**
   ```powershell
   cd 2_Backend_CSharp
   dotnet run --launch-profile https
   ```
   *Tarayıcıda API test panelini açın:* `https://localhost:7266/swagger`

2. **Frontend'i (Web Arayüzü) Başlatın:**
   ```powershell
   cd 4_Frontend_Web
   dotnet run
   ```
   *Terminalde çıkan adresi tarayıcınızda açın.*

---

## 📌 4. Ekip Bazlı Manuel Komutlar (İsteğe Bağlı)

* **AI Ekibi:** `cd 1_AI_Python` ➔ `pip install -r requirements.txt`
* **Backend Ekibi:** `cd 2_Backend_CSharp` ➔ `dotnet build`
* **Frontend Ekibi:** `cd 4_Frontend_Web` ➔ `dotnet build`
* **ESP32 Ekibi:** Gerekli kütüphaneler için [3_ESP32_Embedded/libraries.txt](3_ESP32_Embedded/libraries.txt) dosyasını inceleyin.
