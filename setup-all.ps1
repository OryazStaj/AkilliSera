$ErrorActionPreference = "Stop" # Herhangi bir adımda hata çıkarsa işlemi durdurur

Write-Host "Akilli Sera setup basliyor..." -ForegroundColor Green

# Çalıştırılan dosyanın bulunduğu kök dizine geçiş yap
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

function Step($msg) {
    Write-Host "`n=== $msg ===" -ForegroundColor Cyan
}

# 1. AI Python Bağımlılıkları
Step "AI dependencies"
$pythonReq = "1_AI_Python/requirements.txt"
if (Test-Path $pythonReq) {
    python -m pip install --upgrade pip
    python -m pip install -r $pythonReq
} else {
    Write-Warning "$pythonReq bulunamadi, AI adimi atlandi."
}

# 2. C# Backend
Step "Backend restore/build"
$backendProj = "2_Backend_CSharp/AkilliSera_API.csproj"
if (Test-Path $backendProj) {
    dotnet restore $backendProj
    dotnet build $backendProj
} else {
    Write-Warning "$backendProj bulunamadi, backend adimi atlandi."
}

# 3. C# Frontend Web
Step "Frontend restore/build"
# Türkçe 'İ' harfi yerine 'I' kullanarak kontrol edin:
$frontendProj = "4_Frontend_Web/SERASISTEMI.csproj" 

if (Test-Path $frontendProj) {
    dotnet restore $frontendProj
    dotnet build $frontendProj
} else {
    # Alternatif olarak klasördeki .csproj dosyasını dinamik aratabilirsiniz:
    $autoProj = Get-ChildItem -Path "4_Frontend_Web" -Filter "*.csproj" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($autoProj) {
        dotnet restore $autoProj.FullName
        dotnet build $autoProj.FullName
    } else {
        Write-Warning "4_Frontend_Web klasorunde .csproj bulunamadi, frontend adimi atlandi."
    }
}

Write-Host "`nKurulum adimlari basariyla tamamlandi." -ForegroundColor Green