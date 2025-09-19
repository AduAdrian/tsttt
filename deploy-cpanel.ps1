# Deploy Script pentru cPanel
# Acest script pregătește fișierele pentru upload în File Manager cPanel

# Parametrii de configurare
$BuildOutputPath = ".\build-output"
$DeploymentPath = ".\cpanel-deploy"
$WebConfigPath = ".\WebApplication1\web.config"
$DatabasePath = ".\WebApplication1\app.db"

Write-Host "=== Deploy Script pentru cPanel ===" -ForegroundColor Green
Write-Host "Pregătește fișierele pentru cPanel..." -ForegroundColor Yellow

# Verifică dacă build-ul există
if (-not (Test-Path $BuildOutputPath)) {
    Write-Host "Build-ul nu există! Rulează mai întâi build.ps1" -ForegroundColor Red
    exit 1
}

# Creează directorul de deployment dacă nu există
if (-not (Test-Path $DeploymentPath)) {
    New-Item -ItemType Directory -Path $DeploymentPath -Force | Out-Null
}

# Creează structura de directoare pentru cPanel
$PublicHtmlPath = Join-Path $DeploymentPath "public_html"
$AppDataPath = Join-Path $DeploymentPath "app_data"
$LogsPath = Join-Path $DeploymentPath "logs"

New-Item -ItemType Directory -Path $PublicHtmlPath -Force | Out-Null
New-Item -ItemType Directory -Path $AppDataPath -Force | Out-Null
New-Item -ItemType Directory -Path $LogsPath -Force | Out-Null

Write-Host "Structura de directoare creată" -ForegroundColor Green

# Copiază toate fișierele din build în public_html
Write-Host "Copiez fișierele aplicației..." -ForegroundColor Yellow
Copy-Item -Path "$BuildOutputPath\*" -Destination $PublicHtmlPath -Recurse -Force

# Copiază web.config dacă există
if (Test-Path $WebConfigPath) {
    Copy-Item -Path $WebConfigPath -Destination $PublicHtmlPath -Force
    Write-Host "web.config copiat" -ForegroundColor Green
}

# Copiază baza de date în app_data
if (Test-Path $DatabasePath) {
    Copy-Item -Path $DatabasePath -Destination $AppDataPath -Force
    Write-Host "Baza de date copiată în app_data" -ForegroundColor Green
}

# Creează fișier .htaccess pentru IIS/Apache compatibility
$HtaccessContent = @"
# NotificariClienti .htaccess
RewriteEngine On

# Redirect all requests to the ASP.NET Core app
RewriteCond %{REQUEST_FILENAME} !-f
RewriteCond %{REQUEST_FILENAME} !-d
RewriteRule ^(.*)$ index.html [QSA,L]

# Security headers
Header always set X-Content-Type-Options nosniff
Header always set X-Frame-Options DENY
Header always set X-XSS-Protection "1; mode=block"

# Cache static files
<FilesMatch "\.(css|js|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$">
    ExpiresActive On
    ExpiresDefault "access plus 30 days"
</FilesMatch>
"@

$HtaccessPath = Join-Path $PublicHtmlPath ".htaccess"
Set-Content -Path $HtaccessPath -Value $HtaccessContent -Encoding UTF8
Write-Host ".htaccess creat" -ForegroundColor Green

# Creează fișier de configurare pentru cPanel
$CpanelConfigContent = @"
# Configurare cPanel pentru NotificariClienti
# Aceste instrucțiuni trebuie urmate în cPanel

1. Upload fișierele din cpanel-deploy/public_html în directorul public_html al domeniului
2. Upload fișierele din cpanel-deploy/app_data în afara directorului public_html
3. Configurează variabilele de mediu în cPanel:
   - ASPNETCORE_ENVIRONMENT = Production
   - ConnectionStrings__DefaultConnection = Data Source=/path/to/app.db

4. Asigură-te că .NET 8.0 runtime este instalat pe server
5. Configurează aplicația să ruleze ca ASP.NET Core app în cPanel

Paths importante:
- public_html/: toate fișierele aplicației web
- app_data/: baza de date SQLite
- logs/: pentru log-urile aplicației (creează manual în cPanel)
"@

$ConfigPath = Join-Path $DeploymentPath "CPANEL_SETUP_INSTRUCTIONS.txt"
Set-Content -Path $ConfigPath -Value $CpanelConfigContent -Encoding UTF8

# Creează script de upload pentru File Manager
$UploadInstructions = @"
# Instrucțiuni pentru upload în cPanel File Manager

## Pasul 1: Pregătește fișierele
1. Compresează directorul 'cpanel-deploy' într-un arhivă ZIP
2. Logează-te în cPanel
3. Deschide File Manager

## Pasul 2: Upload fișierele
1. Navighează la directorul root al domeniului tău
2. Upload arhiva ZIP
3. Extrage arhiva

## Pasul 3: Mutarea fișierelor
1. Mută tot conținutul din 'cpanel-deploy/public_html' în directorul 'public_html' al domeniului
2. Mută directorul 'app_data' în afara directorului public_html
3. Creează directorul 'logs' pentru aplicație

## Pasul 4: Configurare cPanel
1. În cPanel, accesează 'Softaculous' sau 'App Installer'
2. Caută opțiunea pentru ASP.NET Core applications
3. Configurează aplicația să pointeze la fișierul principal

## Pasul 5: Testare
1. Accesează domeniul tău
2. Verifică dacă aplicația se încarcă corect
3. Testează funcționalitățile principale

## Note importante:
- Asigură-te că serverul suportă .NET 8.0
- Verifică permisiunile fișierelor (755 pentru directoare, 644 pentru fișiere)
- Configurează variabilele de mediu în cPanel
"@

$UploadInstructionsPath = Join-Path $DeploymentPath "UPLOAD_INSTRUCTIONS.md"
Set-Content -Path $UploadInstructionsPath -Value $UploadInstructions -Encoding UTF8

Write-Host "" -ForegroundColor White
Write-Host "=== DEPLOYMENT PREGĂTIT CU SUCCES! ===" -ForegroundColor Green
Write-Host "" -ForegroundColor White
Write-Host "Fișierele pentru cPanel sunt în: $DeploymentPath" -ForegroundColor Cyan
Write-Host "" -ForegroundColor White
Write-Host "STRUCTURA CREATĂ:" -ForegroundColor Yellow
Write-Host "📁 cpanel-deploy/" -ForegroundColor White
Write-Host "  📁 public_html/          # Upload în public_html cPanel" -ForegroundColor Cyan
Write-Host "  📁 app_data/             # Upload în afara public_html" -ForegroundColor Cyan
Write-Host "  📁 logs/                 # Pentru log-uri (creează manual)" -ForegroundColor Cyan
Write-Host "  📄 CPANEL_SETUP_INSTRUCTIONS.txt" -ForegroundColor White
Write-Host "  📄 UPLOAD_INSTRUCTIONS.md" -ForegroundColor White
Write-Host "" -ForegroundColor White
Write-Host "URMĂTORII PAȘI:" -ForegroundColor Yellow
Write-Host "1. Compresează directorul 'cpanel-deploy' într-un ZIP" -ForegroundColor White
Write-Host "2. Upload ZIP-ul în cPanel File Manager" -ForegroundColor White  
Write-Host "3. Extrage și urmează instrucțiunile din fișierele create" -ForegroundColor White
Write-Host "" -ForegroundColor White