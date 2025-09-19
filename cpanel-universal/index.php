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
