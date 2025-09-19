# Build Script pentru NotificariClienti
# Acest script construiește aplicația pentru deployment pe cPanel

# Parametrii de configurare
$ProjectPath = ".\WebApplication1"
$BuildOutputPath = ".\build-output"
$DeploymentPath = ".\cpanel-deploy"

Write-Host "=== Build Script pentru NotificariClienti ===" -ForegroundColor Green
Write-Host "Începe procesul de build..." -ForegroundColor Yellow

# Curăță directoarele existente
if (Test-Path $BuildOutputPath) {
    Remove-Item $BuildOutputPath -Recurse -Force
    Write-Host "Directorul build-output curățat" -ForegroundColor Yellow
}

if (Test-Path $DeploymentPath) {
    Remove-Item $DeploymentPath -Recurse -Force
    Write-Host "Directorul cpanel-deploy curățat" -ForegroundColor Yellow
}

# Creează directoarele
New-Item -ItemType Directory -Path $BuildOutputPath -Force | Out-Null
New-Item -ItemType Directory -Path $DeploymentPath -Force | Out-Null

Write-Host "Directoare create cu succes" -ForegroundColor Green

# Restore packages
Write-Host "Restaurez pachetele NuGet..." -ForegroundColor Yellow
Set-Location $ProjectPath
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Eroare la restaurarea pachetelor!" -ForegroundColor Red
    exit 1
}

# Build aplicația pentru production
Write-Host "Build aplicația pentru production..." -ForegroundColor Yellow
dotnet publish -c Release -o "..\$BuildOutputPath" --self-contained false --runtime win-x64
if ($LASTEXITCODE -ne 0) {
    Write-Host "Eroare la build!" -ForegroundColor Red
    exit 1
}

Set-Location ..

Write-Host "Build completat cu succes!" -ForegroundColor Green
Write-Host "Fișierele de build sunt în: $BuildOutputPath" -ForegroundColor Cyan