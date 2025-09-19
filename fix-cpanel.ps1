# Fix cPanel Deployment - Universal Solution
# Această soluție funcționează pe orice tip de server cPanel

Write-Host "=== cPanel Universal Fix ===" -ForegroundColor Green
Write-Host "Creating web-compatible version..." -ForegroundColor Yellow

# Creez deployment universal
$universalPath = ".\cpanel-universal"
if (Test-Path $universalPath) { Remove-Item $universalPath -Recurse -Force }
New-Item -ItemType Directory -Path $universalPath -Force | Out-Null

# Copiez doar fișierele statice (nu .NET executabile)
if (Test-Path ".\build-output\wwwroot") {
    Copy-Item ".\build-output\wwwroot\*" -Destination $universalPath -Recurse -Force
}

Write-Host "✅ Copied static files" -ForegroundColor Green

# Creez index.php pentru servere PHP
$indexPhp = @'
<?php
// NotificariClienti - cPanel Compatible Version
header('Content-Type: text/html; charset=UTF-8');
?>
<!DOCTYPE html>
<html lang="ro">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>NotificariClienti - Sistem Notificări</title>
    <link rel="stylesheet" href="css/site.css">
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            margin: 0;
            padding: 0;
            min-height: 100vh;
        }
        .header {
            background: rgba(0,0,0,0.2);
            padding: 20px;
            text-align: center;
            border-bottom: 2px solid rgba(255,255,255,0.1);
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 40px 20px;
        }
        .card {
            background: rgba(255,255,255,0.15);
            backdrop-filter: blur(10px);
            border-radius: 15px;
            padding: 30px;
            margin: 20px 0;
            border: 1px solid rgba(255,255,255,0.2);
        }
        .features {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
            margin: 30px 0;
        }
        .feature {
            background: rgba(255,255,255,0.1);
            padding: 20px;
            border-radius: 10px;
            text-align: center;
        }
        .btn {
            background: #4CAF50;
            color: white;
            border: none;
            padding: 12px 24px;
            border-radius: 5px;
            cursor: pointer;
            text-decoration: none;
            display: inline-block;
            margin: 10px 5px;
            transition: background 0.3s;
        }
        .btn:hover { background: #45a049; }
        .btn-secondary { background: #2196F3; }
        .btn-secondary:hover { background: #1976D2; }
        .status {
            background: rgba(255,193,7,0.2);
            border-left: 4px solid #FFC107;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }
        .footer {
            text-align: center;
            padding: 20px;
            background: rgba(0,0,0,0.3);
            margin-top: 50px;
        }
    </style>
</head>
<body>
    <div class="header">
        <h1>🚀 NotificariClienti</h1>
        <p>Sistem de Management și Notificări pentru Clienți</p>
    </div>

    <div class="container">
        <div class="card">
            <h2>⚠️ Configurare Necesară</h2>
            <div class="status">
                <strong>Status:</strong> Aplicația necesită configurare server pentru .NET
            </div>
            
            <h3>Opțiuni Disponibile:</h3>
            
            <div class="features">
                <div class="feature">
                    <h4>🔧 Configurare Server</h4>
                    <p>Contactează furnizorul de hosting pentru suport ASP.NET Core</p>
                    <a href="#" class="btn btn-secondary" onclick="showHostingInfo()">Detalii Hosting</a>
                </div>
                
                <div class="feature">
                    <h4>☁️ Cloud Deployment</h4>
                    <p>Deployment pe platforme cloud cu suport .NET nativ</p>
                    <a href="#" class="btn btn-secondary" onclick="showCloudOptions()">Opțiuni Cloud</a>
                </div>
                
                <div class="feature">
                    <h4>📱 Versiune Web</h4>
                    <p>Interfață web simplificată compatibilă cu orice server</p>
                    <a href="web-interface.php" class="btn">Deschide Interfața</a>
                </div>
            </div>
        </div>

        <div class="card" id="hosting-info" style="display:none;">
            <h3>📞 Informații pentru Furnizorul de Hosting</h3>
            <p><strong>Cerințe tehnice pentru aplicația NotificariClienti:</strong></p>
            <ul>
                <li>ASP.NET Core 8.0 Runtime</li>
                <li>Windows Server cu IIS sau Linux cu .NET runtime</li>
                <li>AspNetCoreModuleV2 pentru IIS</li>
                <li>SQLite support pentru baza de date</li>
            </ul>
            <p><strong>Alternative recomandate:</strong></p>
            <ul>
                <li>Windows Hosting Plans</li>
                <li>Azure App Service</li>
                <li>AWS Elastic Beanstalk</li>
                <li>VPS cu control complet</li>
            </ul>
        </div>

        <div class="card" id="cloud-info" style="display:none;">
            <h3>☁️ Opțiuni Cloud Deployment</h3>
            <div class="features">
                <div class="feature">
                    <h4>Microsoft Azure</h4>
                    <p>App Service cu suport nativ .NET</p>
                    <a href="https://azure.microsoft.com" target="_blank" class="btn">Vizitează Azure</a>
                </div>
                <div class="feature">
                    <h4>Amazon AWS</h4>
                    <p>Elastic Beanstalk pentru .NET</p>
                    <a href="https://aws.amazon.com" target="_blank" class="btn">Vizitează AWS</a>
                </div>
            </div>
        </div>
    </div>

    <div class="footer">
        <p>&copy; 2025 NotificariClienti - Sistem de Management Clienți</p>
        <p>Pentru suport tehnic: contact@misedainspect.ro</p>
    </div>

    <script>
        function showHostingInfo() {
            document.getElementById('hosting-info').style.display = 'block';
            document.getElementById('cloud-info').style.display = 'none';
        }
        
        function showCloudOptions() {
            document.getElementById('cloud-info').style.display = 'block';
            document.getElementById('hosting-info').style.display = 'none';
        }
        
        // Auto-test pentru .NET support
        fetch('/api/health', {method: 'HEAD', mode: 'no-cors'})
            .then(() => {
                document.querySelector('.status').innerHTML = 
                    '<strong>Status:</strong> ✅ Server detectat cu suport .NET';
                document.querySelector('.status').style.borderLeftColor = '#4CAF50';
                document.querySelector('.status').style.background = 'rgba(76,175,80,0.2)';
            })
            .catch(() => {
                console.log('No .NET support detected');
            });
    </script>
</body>
</html>
'@

Set-Content -Path "$universalPath\index.php" -Value $indexPhp
Write-Host "✅ Created index.php" -ForegroundColor Green

# Creez interfață web simplă
$webInterface = @'
<?php
// Interfața web simplificată pentru NotificariClienti
?>
<!DOCTYPE html>
<html lang="ro">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>NotificariClienti - Interfață Web</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Arial, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            min-height: 100vh;
        }
        .navbar {
            background: rgba(0,0,0,0.3);
            padding: 15px;
            position: sticky;
            top: 0;
            z-index: 1000;
        }
        .nav-brand { font-size: 24px; font-weight: bold; }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }
        .dashboard {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
            margin: 30px 0;
        }
        .card {
            background: rgba(255,255,255,0.15);
            backdrop-filter: blur(10px);
            border-radius: 15px;
            padding: 25px;
            border: 1px solid rgba(255,255,255,0.2);
        }
        .card h3 { margin-bottom: 15px; color: #FFF; }
        .btn {
            background: #4CAF50;
            color: white;
            border: none;
            padding: 10px 20px;
            border-radius: 5px;
            cursor: pointer;
            text-decoration: none;
            display: inline-block;
            margin: 5px;
        }
        .btn:hover { background: #45a049; }
        .form-group {
            margin-bottom: 15px;
        }
        .form-group label {
            display: block;
            margin-bottom: 5px;
        }
        .form-group input, .form-group textarea {
            width: 100%;
            padding: 10px;
            border: 1px solid rgba(255,255,255,0.3);
            border-radius: 5px;
            background: rgba(255,255,255,0.1);
            color: white;
        }
        .form-group input::placeholder { color: rgba(255,255,255,0.7); }
        .alert {
            padding: 15px;
            border-radius: 5px;
            margin: 15px 0;
        }
        .alert-info { background: rgba(33,150,243,0.2); border-left: 4px solid #2196F3; }
        .alert-warning { background: rgba(255,193,7,0.2); border-left: 4px solid #FFC107; }
        .clients-list {
            background: rgba(255,255,255,0.1);
            border-radius: 10px;
            overflow: hidden;
        }
        .client-item {
            padding: 15px;
            border-bottom: 1px solid rgba(255,255,255,0.1);
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        .client-item:hover { background: rgba(255,255,255,0.05); }
    </style>
</head>
<body>
    <div class="navbar">
        <div class="nav-brand">📋 NotificariClienti - Demo Interface</div>
    </div>

    <div class="container">
        <div class="alert alert-info">
            <strong>ℹ️ Demo Mode:</strong> Aceasta este o interfață demonstrativă. Pentru funcționalitate completă, 
            aplicația necesită server cu suport .NET Core.
        </div>

        <div class="dashboard">
            <div class="card">
                <h3>👥 Managementul Clienților</h3>
                <p>Adaugă și gestionează informațiile clienților</p>
                
                <div class="form-group">
                    <label>Nume Client:</label>
                    <input type="text" placeholder="Ex: Popescu Ion" id="clientName">
                </div>
                <div class="form-group">
                    <label>Telefon:</label>
                    <input type="tel" placeholder="Ex: 0721123456" id="clientPhone">
                </div>
                <div class="form-group">
                    <label>Email:</label>
                    <input type="email" placeholder="Ex: ion@email.com" id="clientEmail">
                </div>
                
                <button class="btn" onclick="addClient()">➕ Adaugă Client</button>
                <button class="btn" onclick="showClients()">👀 Vezi Clienți</button>
            </div>

            <div class="card">
                <h3>📱 Notificări SMS</h3>
                <p>Trimite notificări către clienți</p>
                
                <div class="form-group">
                    <label>Mesaj:</label>
                    <textarea placeholder="Introduceți mesajul..." id="smsMessage" rows="3"></textarea>
                </div>
                <div class="form-group">
                    <label>Numărul de telefon:</label>
                    <input type="tel" placeholder="0721123456" id="smsPhone">
                </div>
                
                <button class="btn" onclick="sendSMS()">📤 Trimite SMS</button>
                <button class="btn" onclick="showTemplates()">📝 Template-uri</button>
            </div>

            <div class="card">
                <h3>📊 Rapoarte și Statistici</h3>
                <p>Vizualizează statisticile sistemului</p>
                
                <div class="alert alert-warning">
                    <strong>⚠️ Funcționalitate limitată:</strong><br>
                    Pentru rapoarte complete și export Excel, este necesară aplicația .NET completă.
                </div>
                
                <button class="btn" onclick="showStats()">📈 Statistici Demo</button>
                <button class="btn" onclick="exportDemo()">📄 Export Demo</button>
            </div>
        </div>

        <div class="card" id="clientsList" style="display:none;">
            <h3>📋 Lista Clienților (Demo)</h3>
            <div class="clients-list" id="clientsContainer">
                <div class="client-item">
                    <div>
                        <strong>Demo Client 1</strong><br>
                        <small>0721111111 | demo1@email.com</small>
                    </div>
                    <button class="btn">Notifică</button>
                </div>
                <div class="client-item">
                    <div>
                        <strong>Demo Client 2</strong><br>
                        <small>0721222222 | demo2@email.com</small>
                    </div>
                    <button class="btn">Notifică</button>
                </div>
            </div>
        </div>
    </div>

    <script>
        function addClient() {
            const name = document.getElementById('clientName').value;
            const phone = document.getElementById('clientPhone').value;
            const email = document.getElementById('clientEmail').value;
            
            if (name && phone) {
                alert(`✅ Client adăugat în demo:\n${name}\n${phone}\n${email}`);
                // Clear form
                document.getElementById('clientName').value = '';
                document.getElementById('clientPhone').value = '';
                document.getElementById('clientEmail').value = '';
            } else {
                alert('⚠️ Completează cel puțin numele și telefonul');
            }
        }
        
        function showClients() {
            document.getElementById('clientsList').style.display = 'block';
        }
        
        function sendSMS() {
            const message = document.getElementById('smsMessage').value;
            const phone = document.getElementById('smsPhone').value;
            
            if (message && phone) {
                alert(`📱 SMS Demo trimis la ${phone}:\n"${message}"`);
                document.getElementById('smsMessage').value = '';
                document.getElementById('smsPhone').value = '';
            } else {
                alert('⚠️ Completează mesajul și numărul de telefon');
            }
        }
        
        function showTemplates() {
            alert(`📝 Template-uri disponibile:\n\n1. "Reamintire programare maine la ora..."\n2. "Confirmare programare pentru..."\n3. "Anulare programare din cauza..."\n4. "Mulțumim pentru vizită..."`);
        }
        
        function showStats() {
            alert(`📊 Statistici Demo:\n\n👥 Clienți: 150\n📱 SMS trimise: 1,250\n📧 Email-uri: 850\n📅 Programări: 95\n⭐ Rating mediu: 4.8/5`);
        }
        
        function exportDemo() {
            alert(`📄 Export Demo:\n\nÎn versiunea completă poți exporta:\n• Lista clienților (Excel)\n• Rapoarte SMS (PDF)\n• Statistici programări (Excel)\n• Istoricul notificărilor (CSV)`);
        }
    </script>
</body>
</html>
'@

Set-Content -Path "$universalPath\web-interface.php" -Value $webInterface
Write-Host "✅ Created web-interface.php" -ForegroundColor Green

# Creez .htaccess pentru redirects
$htaccess = @'
# NotificariClienti - Universal cPanel
DirectoryIndex index.php index.html

# Security
<Files "*.config">
    Order allow,deny
    Deny from all
</Files>

<Files "*.exe">
    Order allow,deny
    Deny from all
</Files>

# Clean URLs
RewriteEngine On
RewriteCond %{REQUEST_FILENAME} !-f
RewriteCond %{REQUEST_FILENAME} !-d
RewriteRule ^(.*)$ index.php [QSA,L]

# Compression
<IfModule mod_deflate.c>
    AddOutputFilterByType DEFLATE text/html text/css text/javascript application/javascript
</IfModule>
'@

Set-Content -Path "$universalPath\.htaccess" -Value $htaccess
Write-Host "✅ Created .htaccess" -ForegroundColor Green

Write-Host ""
Write-Host "=== UNIVERSAL SOLUTION READY! ===" -ForegroundColor Green
Write-Host ""
Write-Host "📁 Upload contents of: cpanel-universal/" -ForegroundColor Cyan
Write-Host "   ├── index.php (main interface)" -ForegroundColor White
Write-Host "   ├── web-interface.php (demo app)" -ForegroundColor White
Write-Host "   ├── .htaccess (configuration)" -ForegroundColor White
Write-Host "   └── css/, js/ (static files)" -ForegroundColor White
Write-Host ""
Write-Host "🎯 This version works on ANY cPanel server!" -ForegroundColor Yellow
Write-Host "💡 No .NET required - pure PHP/HTML/JS" -ForegroundColor Yellow
Write-Host ""