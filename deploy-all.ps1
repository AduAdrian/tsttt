# Deployment Complet pentru cPanel
# Acest script rulează întregul proces de build și pregătire pentru cPanel

Write-Host "=== DEPLOYMENT COMPLET PENTRU CPANEL ===" -ForegroundColor Green
Write-Host "Rulează întregul proces de build și deployment..." -ForegroundColor Yellow
Write-Host ""

# Rulează build-ul
Write-Host "PASUL 1: Build aplicația..." -ForegroundColor Cyan
& ".\build.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Eroare la build! Opresc procesul." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "PASUL 2: Pregătesc fișierele pentru cPanel..." -ForegroundColor Cyan

# Rulează deployment-ul
& ".\deploy-cpanel.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Eroare la deployment! Opresc procesul." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== PROCESUL COMPLET FINALIZAT ===" -ForegroundColor Green
Write-Host ""
Write-Host "✅ Build completat" -ForegroundColor Green
Write-Host "✅ Fișiere pregătite pentru cPanel" -ForegroundColor Green  
Write-Host "✅ Instrucțiuni de upload create" -ForegroundColor Green
Write-Host ""
Write-Host "ACUM POȚI:" -ForegroundColor Yellow
Write-Host "1. Compress 'cpanel-deploy' folder to ZIP" -ForegroundColor White
Write-Host "2. Upload ZIP to cPanel File Manager" -ForegroundColor White
Write-Host "3. Follow instructions in UPLOAD_INSTRUCTIONS.md" -ForegroundColor White
Write-Host ""