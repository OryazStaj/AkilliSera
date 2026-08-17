Write-Host "Akilli Sera setup basliyor..."

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

function Step($msg) {
    Write-Host "`n=== $msg ==="
}

Step "AI dependencies"
if (Test-Path "1_AI_Python/requirements.txt") {
    python -m pip install -r "1_AI_Python/requirements.txt"
} else {
    Write-Warning "1_AI_Python/requirements.txt bulunamadi, AI adimi atlandi."
}

Step "Backend restore/build"
if (Test-Path "2_Backend_CSharp/AkilliSera_API.csproj") {
    dotnet restore "2_Backend_CSharp/AkilliSera_API.csproj"
    dotnet build "2_Backend_CSharp/AkilliSera_API.csproj"
} else {
    Write-Warning "Backend csproj bulunamadi, backend adimi atlandi."
}

Step "Frontend restore/build"
if (Test-Path "4_Frontend_Web/SERASİSTEMİ.csproj") {
    dotnet restore "4_Frontend_Web/SERASİSTEMİ.csproj"
    dotnet build "4_Frontend_Web/SERASİSTEMİ.csproj"
} else {
    Write-Warning "Frontend csproj bulunamadi, frontend adimi atlandi."
}

Write-Host "`nKurulum adimlari tamamlandi."
