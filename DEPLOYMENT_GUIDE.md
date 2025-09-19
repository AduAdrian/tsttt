# 🚀 Ghid Deployment cPanel pentru NotificariClienti

Acest ghid te ajută să faci deployment pentru aplicația NotificariClienti pe un server cPanel.

## 📋 Ce conțin scripturile

### 1. `build.ps1`
- Construiește aplicația .NET pentru production
- Creează build-ul optimizat în folderul `build-output`

### 2. `deploy-cpanel.ps1` 
- Pregătește fișierele pentru structura cPanel
- Creează directoarele necesare: `public_html`, `app_data`, `logs`
- Generează `.htaccess` și fișiere de configurare

### 3. `deploy-all.ps1`
- Rulează întreg procesul automat (build + deploy)
- Script complet pentru deployment

## 🔧 Cum să folosești scripturile

### Opțiunea 1: Deployment complet (recomandat)
```powershell
.\deploy-all.ps1
```

### Opțiunea 2: Pas cu pas
```powershell
# 1. Build aplicația
.\build.ps1

# 2. Pregătește pentru cPanel
.\deploy-cpanel.ps1
```

## 📁 Structura generată

După rularea scripturilor, vei avea:

```
cpanel-deploy/
├── public_html/          # → Upload în public_html din cPanel
│   ├── WebApplication1.exe
│   ├── wwwroot/
│   ├── Views/
│   ├── web.config
│   ├── .htaccess
│   └── toate fișierele aplicației
├── app_data/             # → Upload în afara public_html
│   └── app.db
├── logs/                 # → Creează manual în cPanel
├── CPANEL_SETUP_INSTRUCTIONS.txt
└── UPLOAD_INSTRUCTIONS.md
```

## 🌐 Pași pentru upload în cPanel

### 1. Pregătire locală
1. Rulează `.\deploy-all.ps1`
2. Compresează folderul `cpanel-deploy` într-un fișier ZIP

### 2. Upload în cPanel
1. Logează-te în cPanel
2. Deschide **File Manager**
3. Navighează la directorul root al domeniului
4. Upload fișierul ZIP
5. Extrage ZIP-ul

### 3. Organizare fișiere
1. Mută conținutul din `cpanel-deploy/public_html/` în `public_html/` al domeniului
2. Mută folderul `app_data/` în afara directorului `public_html`
3. Creează folderul `logs/` pentru aplicație

### 4. Configurare cPanel
1. În cPanel, caută **App Installer** sau **Softaculous**
2. Configurează aplicația ASP.NET Core
3. Setează variabilele de mediu:
   - `ASPNETCORE_ENVIRONMENT = Production`
   - `ConnectionStrings__DefaultConnection = Data Source=/path/to/app.db`

## ⚙️ Configurări importante

### Variabile de mediu necesare în cPanel:
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://*:80
ConnectionStrings__DefaultConnection=Data Source=/home/username/app_data/app.db
```

### Permisiuni fișiere:
- Directoare: `755`
- Fișiere: `644`
- Executabile: `755`

## 🐛 Depanare

### Probleme comune:

1. **Aplicația nu se încarcă**
   - Verifică dacă .NET 8.0 runtime este instalat
   - Controlează variabilele de mediu
   - Verifică log-urile în folderul `logs/`

2. **Erori de baza de date**
   - Asigură-te că `app.db` este în locația corectă
   - Verifică connection string-ul
   - Controlează permisiunile pentru folderul `app_data/`

3. **Probleme cu fișierele statice**
   - Verifică dacă `.htaccess` este în `public_html/`
   - Controlează permisiunile pentru `wwwroot/`

## 📞 Suport

Pentru probleme specifice cPanel:
1. Consultă documentația furnizorului de hosting
2. Contactează suportul tehnic pentru configurarea ASP.NET Core
3. Verifică că serverul suportă .NET 8.0

## 📝 Note importante

- **Backup**: Fă backup la baza de date înainte de deployment
- **Testing**: Testează aplicația pe un subdomeniu înainte de production
- **SSL**: Configurează certificatul SSL în cPanel după deployment
- **Security**: Asigură-te că fișierele sensibile sunt în afara `public_html/`

---

*Acest ghid este creat pentru aplicația NotificariClienti - un sistem de notificări pentru clienți dezvoltat în ASP.NET Core 8.0*