# Akilli Sera - Ekip Kurulum Gereksinimleri

Bu dokuman, tum ekiplerin kendi ortamini ayni standartta hazirlamasi icin olusturuldu.

## 1) Ortak Gereksinimler

- Git
- .NET SDK 9.x (frontend icin)
- .NET SDK 8.x (backend icin)
- Python 3.11
- SQL Server (Express olur) + SSMS veya Azure Data Studio

## 2) AI Ekibi (1_AI_Python)

Kurulum:

```powershell
cd 1_AI_Python
pip install -r requirements.txt
```

Kullanilan paketler [1_AI_Python/requirements.txt](1_AI_Python/requirements.txt) icinde yonetilir.

## 3) Backend Ekibi (2_Backend_CSharp)

Kurulum:

```powershell
cd 2_Backend_CSharp
dotnet restore
dotnet build
```

Not:
- NuGet paketleri csproj dosyasinda tanimli oldugu icin ayri bir paket listesi gerekmez.
- Gelistirme baglanti metnini appsettings.Development.json veya user-secrets ile verin.

## 4) Frontend Ekibi (4_Frontend_Web)

Kurulum:

```powershell
cd 4_Frontend_Web
dotnet restore
dotnet build
```

## 5) ESP32 Ekibi (3_ESP32_Embedded)

Kullanilan kutuphaneler [3_ESP32_Embedded/libraries.txt](3_ESP32_Embedded/libraries.txt) dosyasina yazilmistir.

Gerekli olanlar:
- ESP32 Board Package
- ArduinoJson
- DHT sensor kutuphaneleri

## 6) Veritabani Ekibi (5_Veritabanı_SQL)

Kurulum sirasiyla SQL script calistirin:
1. 01_Sabit_Referans_Tablolari.sql
2. 02_Dinamik_Log_Tablolari.sql
3. 05_Dis_Ortam.sql
4. 06_Kullanici_ve_Bildirim_Tablolari.sql
5. 07_Ilaclama_Takip.sql
6. 03_Baslangic_Verileri.sql
7. 04_Stored_Prosedur_Tablolari.sql

## 7) Tek Komutla Hazirlik (Opsiyonel)

Kok dizinde asagidaki script ile AI + backend + frontend kurulumunu hizlica baslatabilirsiniz:

```powershell
./setup-all.ps1
```

Script dosyasi: [setup-all.ps1](setup-all.ps1)
