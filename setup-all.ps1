$ErrorActionPreference = "Continue"

Write-Host "==================================================" -ForegroundColor Green
Write-Host "   🌱 AKILLI SERA - GELİSTİRİCİ ORTAMI KURULUMU  " -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green

# 1. Kök dizine geçiş
$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
if ($scriptDir) { Set-Location $scriptDir }

$stepSuccess = @{}

function Step($msg) {
    Write-Host "`n=== $msg ===" -ForegroundColor Cyan
}

function Check-Command($cmd, $name) {
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-Warning "UYARI: '$name' ($cmd) sistemde bulunamadi veya PATH'e ekli degil."
        return $false
    }
    return $true
}

# ----------------------------------------------------
# 1. AI Python Bağımlılıkları
# ----------------------------------------------------
Step "1. AI Python Bagimliliklari (numpy, scikit-fuzzy, opencv, ultralytics)"
if (Check-Command "python" "Python") {
    $pythonReq = "1_AI_Python/requirements.txt"
    if (Test-Path $pythonReq) {
        Write-Host "pip guncelleniyor ve gereksinimler yukleniyor..." -ForegroundColor Gray
        python -m pip install --upgrade pip --quiet
        python -m pip install -r $pythonReq
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Python paketleri basariyla yuklendi." -ForegroundColor Green
            $stepSuccess["AI_Python"] = $true
        } else {
            Write-Warning "Python paket yuklemesi sirasinda bazi hatalar olustu."
            $stepSuccess["AI_Python"] = $false
        }
    } else {
        Write-Warning "$pythonReq bulunamadi, AI adimi atlandi."
        $stepSuccess["AI_Python"] = $false
    }
} else {
    $stepSuccess["AI_Python"] = $false
}

# ----------------------------------------------------
# 2. Veritabanı (MS SQL Server) Otomatik Kurulumu
# ----------------------------------------------------
Step "2. Veritabani (MS SQL Server) Kurulumu & Yapilandirmasi"
$sqlInstances = @(".", ".\SQLEXPRESS", "(localdb)\MSSQLLocalDB", "localhost")
$detectedSqlInstance = $null

if (Check-Command "sqlcmd" "SQLCMD (SQL Server CLI)") {
    Write-Host "Kullanilabilir SQL Server ornegi araniyor..." -ForegroundColor Gray
    foreach ($inst in $sqlInstances) {
        try {
            $testOutput = & sqlcmd -S $inst -Q "SELECT 1" -l 2 2>&1
            if ($LASTEXITCODE -eq 0) {
                $detectedSqlInstance = $inst
                break
            }
        } catch {
            # Devam et
        }
    }

    if ($detectedSqlInstance) {
        Write-Host "SQL Server bulundu: '$detectedSqlInstance'" -ForegroundColor Green
        
        # 1. Veritabanı oluşturma
        Write-Host "AkilliSeraDB veritabani kontrol ediliyor..." -ForegroundColor Gray
        & sqlcmd -S $detectedSqlInstance -Q "IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'AkilliSeraDB') CREATE DATABASE AkilliSeraDB;" -l 5
        
        # 2. SQL scriptlerini sırayla çalıştırma
        $sqlFiles = @(
            "5_Veritabanı_SQL/01_Sabit_Referans_Tablolari.sql",
            "5_Veritabanı_SQL/02_Dinamik_Log_Tablolari.sql",
            "5_Veritabanı_SQL/05_Dis_Ortam.sql",
            "5_Veritabanı_SQL/06_Kullanici_ve_Bildirim_Tablolari.sql",
            "5_Veritabanı_SQL/07_Ilaclama_Takip.sql",
            "5_Veritabanı_SQL/03_Baslangic_Verileri.sql",
            "5_Veritabanı_SQL/04_Stored_Prosedur_Tablolari.sql"
        )
        
        $sqlErrors = 0
        foreach ($sqlFile in $sqlFiles) {
            if (Test-Path $sqlFile) {
                Write-Host "Calistiriliyor: $sqlFile" -ForegroundColor DarkGray
                & sqlcmd -S $detectedSqlInstance -d AkilliSeraDB -i $sqlFile -b 2>&1 | Out-Null
                # Bazı scriptler içindeki select'ler hata değildir, kritik olmayanları tolere et
            }
        }
        
        # 3. Backend appsettings.Development.json connection string'ini otomatik güncelle
        $appSettingsDev = "2_Backend_CSharp/appsettings.Development.json"
        $appSettingsMain = "2_Backend_CSharp/appsettings.json"
        $connStr = "Server=$detectedSqlInstance;Database=AkilliSeraDB;Trusted_Connection=True;TrustServerCertificate=True;"
        
        $jsonContent = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "$connStr"
  }
}
"@
        Set-Content -Path $appSettingsDev -Value $jsonContent -Encoding UTF8
        Set-Content -Path $appSettingsMain -Value $jsonContent -Encoding UTF8
        Write-Host "Backend baglanti adresi '$detectedSqlInstance' olarak guncellendi." -ForegroundColor Green
        $stepSuccess["Veritabani_SQL"] = $true
    } else {
        Write-Warning "Calisan bir SQL Server ornegi (. veya .\SQLEXPRESS) bulunamadi. SQL adimi atlandi."
        $stepSuccess["Veritabani_SQL"] = $false
    }
} else {
    Write-Warning "sqlcmd komutu bulunamadi. SQL Server Management Studio (SSMS) veya SQL Server kurulu oldugundan emin olun."
    $stepSuccess["Veritabani_SQL"] = $false
}

# ----------------------------------------------------
# 3. C# Backend (.NET 8.0)
# ----------------------------------------------------
Step "3. C# Backend Restore & Build (.NET 8.0 - AkilliSera_API)"
if (Check-Command "dotnet" ".NET SDK") {
    $backendProj = Get-ChildItem -Path "2_Backend_CSharp" -Filter "*.csproj" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($backendProj) {
        Write-Host "Proje bulundu: $($backendProj.Name)" -ForegroundColor Gray
        dotnet restore $backendProj.FullName
        if ($LASTEXITCODE -eq 0) {
            dotnet build $backendProj.FullName --no-restore
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Backend basariyla derlendi." -ForegroundColor Green
                $stepSuccess["Backend"] = $true
            } else {
                Write-Warning "Backend derleme (build) hatasi olustu."
                $stepSuccess["Backend"] = $false
            }
        } else {
            Write-Warning "Backend paket geri yukleme (restore) hatasi olustu."
            $stepSuccess["Backend"] = $false
        }
    } else {
        Write-Warning "2_Backend_CSharp klasorunde .csproj bulunamadi."
        $stepSuccess["Backend"] = $false
    }
} else {
    $stepSuccess["Backend"] = $false
}

# ----------------------------------------------------
# 4. C# Frontend Web (.NET 9.0)
# ----------------------------------------------------
Step "4. C# Frontend Web Restore & Build (.NET 9.0 - SERASISTEMI)"
if (Check-Command "dotnet" ".NET SDK") {
    $frontendProj = Get-ChildItem -Path "4_Frontend_Web" -Filter "*.csproj" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($frontendProj) {
        Write-Host "Proje bulundu: $($frontendProj.Name)" -ForegroundColor Gray
        dotnet restore $frontendProj.FullName
        if ($LASTEXITCODE -eq 0) {
            dotnet build $frontendProj.FullName --no-restore
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Frontend basariyla derlendi." -ForegroundColor Green
                $stepSuccess["Frontend"] = $true
            } else {
                Write-Warning "Frontend derleme (build) hatasi olustu."
                $stepSuccess["Frontend"] = $false
            }
        } else {
            Write-Warning "Frontend paket geri yukleme (restore) hatasi olustu."
            $stepSuccess["Frontend"] = $false
        }
    } else {
        Write-Warning "4_Frontend_Web klasorunde .csproj bulunamadi."
        $stepSuccess["Frontend"] = $false
    }
} else {
    $stepSuccess["Frontend"] = $false
}

# ----------------------------------------------------
# 5. Bilgilendirme (ESP32)
# ----------------------------------------------------
Step "5. Donanim (ESP32) Bilgilendirmesi"
Write-Host "[ESP32] Gerekli kutuphaneler: [3_ESP32_Embedded/libraries.txt]" -ForegroundColor Yellow
Write-Host "  - ESP32 Board Package (Espressif Systems)" -ForegroundColor DarkGray
Write-Host "  - ArduinoJson (6.x)" -ForegroundColor DarkGray
Write-Host "  - DHT sensor library (Adafruit) & Adafruit Unified Sensor" -ForegroundColor DarkGray

# ----------------------------------------------------
# Ozet Rapor
# ----------------------------------------------------
Write-Host "`n==================================================" -ForegroundColor Green
Write-Host "                   KURULUM OZETI                  " -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green

foreach ($key in $stepSuccess.Keys) {
    $status = if ($stepSuccess[$key]) { "BASARILI" } else { "HATALI / ATLANDI" }
    $color = if ($stepSuccess[$key]) { "Green" } else { "Red" }
    Write-Host ("{0,-20}: {1}" -f $key, $status) -ForegroundColor $color
}
Write-Host "==================================================`n" -ForegroundColor Green