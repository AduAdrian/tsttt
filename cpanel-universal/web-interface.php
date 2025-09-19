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
