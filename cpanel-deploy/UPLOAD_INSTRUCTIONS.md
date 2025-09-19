# InstrucÈ›iuni pentru upload Ã®n cPanel File Manager

## Pasul 1: PregÄƒteÈ™te fiÈ™ierele
1. CompreseazÄƒ directorul 'cpanel-deploy' Ã®ntr-un arhivÄƒ ZIP
2. LogeazÄƒ-te Ã®n cPanel
3. Deschide File Manager

## Pasul 2: Upload fiÈ™ierele
1. NavigheazÄƒ la directorul root al domeniului tÄƒu
2. Upload arhiva ZIP
3. Extrage arhiva

## Pasul 3: Mutarea fiÈ™ierelor
1. MutÄƒ tot conÈ›inutul din 'cpanel-deploy/public_html' Ã®n directorul 'public_html' al domeniului
2. MutÄƒ directorul 'app_data' Ã®n afara directorului public_html
3. CreeazÄƒ directorul 'logs' pentru aplicaÈ›ie

## Pasul 4: Configurare cPanel
1. ÃŽn cPanel, acceseazÄƒ 'Softaculous' sau 'App Installer'
2. CautÄƒ opÈ›iunea pentru ASP.NET Core applications
3. ConfigureazÄƒ aplicaÈ›ia sÄƒ pointeze la fiÈ™ierul principal

## Pasul 5: Testare
1. AcceseazÄƒ domeniul tÄƒu
2. VerificÄƒ dacÄƒ aplicaÈ›ia se Ã®ncarcÄƒ corect
3. TesteazÄƒ funcÈ›ionalitÄƒÈ›ile principale

## Note importante:
- AsigurÄƒ-te cÄƒ serverul suportÄƒ .NET 8.0
- VerificÄƒ permisiunile fiÈ™ierelor (755 pentru directoare, 644 pentru fiÈ™iere)
- ConfigureazÄƒ variabilele de mediu Ã®n cPanel
