# Notificari-Progamari

## Descriere
Sistem avansat de notific?ri ?i program?ri pentru Miseda Inspect SRL - extensie a sistemului principal de notific?ri pentru clien?ii ITP.

## Caracteristici Principale

### ?? Sistem de Notific?ri Avansat
- **Notific?ri Multi-Canal**: SMS, Email, Push notifications
- **Programare Automat?**: Notific?ri la intervale personalizate
- **Template-uri Personalizabile**: Mesaje adaptabile pentru diferite tipuri de notific?ri
- **Istoric Complet**: Tracking pentru toate notific?rile trimise

### ?? Sistem de Program?ri
- **Calendar Interactiv**: Vizualizare ?i gestionare program?ri
- **Rezerv?ri Online**: Clien?ii pot programa singuri vizitele ITP
- **Confirm?ri Automate**: SMS/Email de confirmare pentru program?ri
- **Reminder-uri**: Notific?ri de memento înainte de program?ri

### ?? Gestionare Clien?i ITP
- **Baza de Date Centralizat?**: Informa?ii complete despre clien?i ?i vehicule
- **Istoric Inspec?ii**: Tracking complet pentru toate inspec?iile ITP
- **Prognoze Expir?ri**: Predic?ii pentru inspec?iile care urmeaz? s? expire
- **Raportare Avansat?**: Statistici detaliate ?i rapoarte personalizabile

### ?? Integrat pentru Miseda Inspect SRL
- **Branding Personalizat**: Interfa?? adaptat? pentru Miseda Inspect SRL
- **Contact Direct**: Telefon 0756596565, Email notificari-sms@misedainspectsrl.ro
- **Loca?ia R?d?u?i, Suceava**: Optimizat pentru regiunea local?

## Tehnologii
- **.NET 8**: Framework principal
- **ASP.NET Core MVC**: Arhitectur? web
- **Entity Framework Core**: ORM pentru baza de date
- **SQL Server/SQLite**: Baza de date
- **Bootstrap 5**: Framework CSS
- **JavaScript ES6+**: Interactivitate client-side
- **SignalR**: Real-time notifications
- **Quartz.NET**: Job scheduling

## Structura Proiectului
```
/
??? Controllers/           # Controllere MVC
??? Models/               # Modele de date ?i ViewModels
??? Views/                # Vizualiz?ri Razor
??? Services/             # Servicii business logic
??? BackgroundServices/   # Servicii de fundal pentru job-uri
??? Data/                 # Context baza de date ?i migra?ii
??? wwwroot/             # Resurse statice (CSS, JS, imagini)
??? Tests/               # Teste unitare ?i de integrare
??? Documentation/       # Documenta?ie tehnic?
```

## Instalare ?i Configurare

### Cerin?e
- .NET 8 SDK
- SQL Server sau SQLite
- Server SMTP pentru email (mail.misedainspectsrl.ro)
- API SMS pentru notific?ri texto

### Configurare
1. **Cloneaz? repository-ul**
   ```bash
   git clone [URL_REPOSITORY]
   cd notificari-progamari
   ```

2. **Configureaz? appsettings.json**
   ```json
   {
     "EmailSettings": {
       "SmtpServer": "mail.misedainspectsrl.ro",
       "FromEmail": "notificari-sms@misedainspectsrl.ro"
     },
     "SmsSettings": {
       "ApiUrl": "https://www.smsadvert.ro/api/sms/"
     }
   }
   ```

3. **Ruleaz? migra?iile**
   ```bash
   dotnet ef database update
   ```

4. **Porne?te aplica?ia**
   ```bash
   dotnet run
   ```

## Utilizare

### Pentru Administratori
- **Dashboard Central**: Vizualizare complet? a tuturor notific?rilor ?i program?rilor
- **Gestionare Clien?i**: Ad?ugare, editare ?i ?tergere clien?i
- **Configurare Notific?ri**: Setare template-uri ?i intervale
- **Rapoarte**: Generare rapoarte detaliate

### Pentru Clien?i
- **Portal Self-Service**: Programare online pentru inspec?ii ITP
- **Istoric Personal**: Vizualizare istoric inspec?ii ?i notific?ri
- **Notific?ri Personalizate**: Primire alerte prin canalul preferat
- **Confirm?ri Rapide**: Confirmare/reprogramare prin SMS sau email

## API ?i Integr?ri

### Endpoints Principale
- `/api/notifications` - Gestionare notific?ri
- `/api/appointments` - Program?ri
- `/api/clients` - Clien?i
- `/api/reports` - Rapoarte

### Integr?ri Externe
- **SMS Provider**: smsadvert.ro
- **Email Provider**: Server SMTP personalizat
- **Calendar APIs**: Google Calendar, Outlook integration
- **Payment Gateways**: Pentru taxe program?ri (op?ional)

## Securitate ?i Conformitate
- **Autentificare Multi-Factor**: SMS + Email verification
- **Criptare Date**: Toate datele sensibile sunt criptate
- **Audit Trail**: Logging complet pentru toate ac?iunile
- **GDPR Compliance**: Respectarea regulamentelor de protec?ie a datelor

## Suport ?i Contact

### Miseda Inspect SRL
- **Telefon**: 0756596565
- **Email**: notificari-sms@misedainspectsrl.ro
- **Adresa**: R?d?u?i, Suceava
- **Website**: www.misedainspectsrl.ro

### Dezvoltare ?i Mentenan??
- **Repository**: GitHub
- **Issues**: Pentru bug reports ?i feature requests
- **Wiki**: Documenta?ie tehnic? detaliat?

## Licen??
© 2024 Miseda Inspect SRL. Toate drepturile rezervate.

## Versiuni ?i Release Notes

### v1.0.0 (Current)
- ? Sistem de baz? pentru notific?ri ?i program?ri
- ? Integrare cu sistemul existent de clien?i ITP
- ? Dashboard administrativ
- ? Portal clien?i

### Versiuni Planificate
- **v1.1.0**: Integrare pl??i online
- **v1.2.0**: API mobile pentru aplica?ie Android/iOS
- **v1.3.0**: AI-powered predictions pentru program?ri
- **v2.0.0**: Multi-tenant support pentru alte sta?ii ITP

---
**Dezvoltat cu ?? pentru Miseda Inspect SRL**