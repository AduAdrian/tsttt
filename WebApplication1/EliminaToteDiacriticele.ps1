# Script FINAL pentru eliminarea diacriticelor din aplica?ia Miseda Inspect
# Versiune optimizat? ?i complet?

Write-Host "?? ELIMINAREA DIACRITICELOR - Miseda Inspect SRL" -ForegroundColor Green
Write-Host "Procesare automat? pentru compatibilitate maxim?...`n" -ForegroundColor White

# Mapare complet?: diacritice ? caractere simple
$replacements = @{
    # Caractere individuale
    '?' = 'a'; '?' = 'A'
    'â' = 'a'; 'Â' = 'A' 
    'î' = 'i'; 'Î' = 'I'
    '?' = 's'; '?' = 'S'
    '?' = 't'; '?' = 'T'
    '?' = 't'; '?' = 'T'
    
    # Caractere problematice din encoding
    'Clien?i' = 'Clienti'
    'clien?i' = 'clienti'
    'num?r' = 'numar'
    'Num?r' = 'Numar'
    'num?rul' = 'numarul'
    'Num?rul' = 'Numarul'
    'înm?triculare' = 'inmatriculare'
    'Înm?triculare' = 'Inmatriculare'
    'notific?ri' = 'notificari'
    'Notific?ri' = 'Notificari'
    'program?ri' = 'programari'
    'Program?ri' = 'Programari'
    'aten?ie' = 'atentie'
    'Aten?ie' = 'Atentie'
    'a?teapt?' = 'asteapta'
    'A?teapt?' = 'Asteapta'
    'a?teptare' = 'asteptare'
    'A?teptare' = 'Asteptare'
    'f?r?' = 'fara'
    'F?r?' = 'Fara'
    'dep??i' = 'depaseste'
    'Dep??i' = 'Depaseste'
    'înv???m?nt' = 'invatamant'
    'Înv???m?nt' = 'Invatamant'
    
    # Cuvinte frecvente
    'c?tre' = 'catre'
    'C?tre' = 'Catre'
    '?i' = 'si'
    '?i' = 'Si'
    'v?' = 'va'
    'V?' = 'Va'
    'bun?' = 'buna'
    'Bun?' = 'Buna'
    'în' = 'in'
    'În' = 'In'
    'înapoi' = 'inapoi'
    'Înapoi' = 'Inapoi'
    'între' = 'intre'
    'Între' = 'Intre'
    'încerci' = 'incerci'
    'Încerci' = 'Incerci'
    'încearc?' = 'incearca'
    'Încearc?' = 'Incearca'
    'ast?zi' = 'astazi'
    'Ast?zi' = 'Astazi'
    'mâine' = 'maine'
    'Mâine' = 'Maine'
    'curând' = 'curand'
    'Curând' = 'Curand'
    'pân?' = 'pana'
    'Pân?' = 'Pana'
    'român?' = 'romana'
    'Român?' = 'Romana'
    'România' = 'Romania'
    'r?mase' = 'ramase'
    'R?mase' = 'Ramase'
    'r?d?u?i' = 'radauti'
    'R?d?u?i' = 'Radauti'
    
    # Verbe
    'expir?' = 'expira'
    'Expir?' = 'Expira'
    'adaug?' = 'adauga'
    'Adaug?' = 'Adauga'
    'editeaz?' = 'editeaza'
    'Editeaz?' = 'Editeaza'
    'salveaz?' = 'salveaza'
    'Salveaz?' = 'Salveaza'
    'actualizeaz?' = 'actualizeaza'
    'Actualizeaz?' = 'Actualizeaza'
    'dezactiveaz?' = 'dezactiveaza'
    'Dezactiveaz?' = 'Dezactiveaza'
    'activeaz?' = 'activeaza'
    'Activeaz?' = 'Activeaza'
    'anuleaz?' = 'anuleaza'
    'Anuleaz?' = 'Anuleaza'
    'continu?' = 'continua'
    'Continu?' = 'Continua'
    'seteaz?' = 'seteaza'
    'Seteaz?' = 'Seteaza'
    'calculeaz?' = 'calculeaza'
    'Calculeaz?' = 'Calculeaza'
    'normalizeaz?' = 'normalizeaza'
    'Normalizeaz?' = 'Normalizeaza'
    'reactiveaz?' = 'reactiveaza'
    'Reactiveaz?' = 'Reactiveaza'
    
    # Expresii
    'v? rug?m' = 'va rugam'
    'V? rug?m' = 'Va rugam'
    'v? a?tept?m' = 'va asteptam'
    'V? a?tept?m' = 'Va asteptam'
    'nou? inspec?ie' = 'noua inspectie'
    'Nou? inspec?ie' = 'Noua inspectie'
    'inspec?ie tehnic?' = 'inspectie tehnica'
    'Inspec?ie tehnic?' = 'Inspectie tehnica'
    'sta?ia ITP' = 'statia ITP'
    'Sta?ia ITP' = 'Statia ITP'
    'zi bun?' = 'zi buna'
    'Zi bun?' = 'Zi buna'
    'bun? ziua' = 'buna ziua'
    'Bun? ziua' = 'Buna ziua'
    
    # Mesaje specifice
    'exist?' = 'exista'
    'Exist?' = 'Exista'
    'întârziere' = 'intarziere'
    'Întârziere' = 'Intarziere'
    'a?teapt?' = 'asteapta'
    'A?teapt?' = 'Asteapta'
    'gre?it' = 'gresit'
    'Gre?it' = 'Gresit'
    '?ters' = 'sters'
    '?ters' = 'Sters'
    'început' = 'inceput'
    'Început' = 'Inceput'
    'înregistra?i' = 'inregistrati'
    'Înregistra?i' = 'Inregistrati'
    'înregistrare' = 'inregistrare'
    'Înregistrare' = 'Inregistrare'
    
    # Mesaje UI
    'Editeaz?' = 'Editeaza'
    'Salveaz? Client' = 'Salveaza Client'
    'Salveaz? Modific?rile' = 'Salveaza Modificarile'
    'Înapoi la List?' = 'Inapoi la Lista'
    'Adaug? Client Nou' = 'Adauga Client Nou'
    'Zile R?mase' = 'Zile Ramase'
    'Ac?iuni' = 'Actiuni'
    'Confirmare Dezactivare' = 'Confirmare Dezactivare'
    'E?ti sigur c? vrei s? dezactivezi' = 'Esti sigur ca vrei sa dezactivezi'
    '?ters permanent' = 'sters permanent'
    'Anuleaz?' = 'Anuleaza'
    'Dezactiveaz?' = 'Dezactiveaza'
}

# G?se?te fi?iere relevante
$files = Get-ChildItem -Path . -Recurse -Include "*.cs","*.cshtml","*.json" | 
    Where-Object { $_.FullName -notmatch "\\obj\\|\\bin\\|\\node_modules\\|\\.git\\|\\wwwroot\\lib\\|\\Migrations\\" }

$totalFiles = $files.Count
$modifiedFiles = 0
$totalChanges = 0

Write-Host "?? G?site $totalFiles fi?iere pentru procesare..." -ForegroundColor Cyan

foreach ($file in $files) {
    try {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8 -ErrorAction Stop
        $originalContent = $content
        $fileChanges = 0
        
        foreach ($pair in $replacements.GetEnumerator()) {
            if ($content.Contains($pair.Key)) {
                $oldCount = ($content -split [regex]::Escape($pair.Key)).Count - 1
                $content = $content.Replace($pair.Key, $pair.Value)
                $fileChanges += $oldCount
                $totalChanges += $oldCount
            }
        }
        
        if ($content -ne $originalContent) {
            Set-Content $file.FullName -Value $content -Encoding UTF8 -NoNewline
            Write-Host "? $($file.Name) - $fileChanges înlocuiri" -ForegroundColor Green
            $modifiedFiles++
        } else {
            Write-Host "? $($file.Name)" -ForegroundColor DarkGray
        }
    }
    catch {
        Write-Host "? Eroare: $($file.Name) - $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n" + ("=" * 60) -ForegroundColor Magenta
Write-Host "?? FINALIZAT!" -ForegroundColor Green
Write-Host "?? Statistici:" -ForegroundColor Cyan
Write-Host "   • Fi?iere procesate: $totalFiles" -ForegroundColor White
Write-Host "   • Fi?iere modificate: $modifiedFiles" -ForegroundColor White  
Write-Host "   • Total înlocuiri: $totalChanges" -ForegroundColor White

if ($modifiedFiles -gt 0) {
    Write-Host "`n?? URM?TORII PA?I:" -ForegroundColor Yellow
    Write-Host "   1. dotnet build" -ForegroundColor White
    Write-Host "   2. Testeaz? aplica?ia" -ForegroundColor White
    Write-Host "   3. git add . && git commit -m 'Elimina diacriticele pentru compatibilitate'" -ForegroundColor White
    
    Write-Host "`n?? Toate textele sunt acum f?r? diacritice pentru compatibilitate maxim?!" -ForegroundColor Cyan
} else {
    Write-Host "`n? Nu au fost necesare modific?ri!" -ForegroundColor Green
}

Write-Host "`nRuleaz? build pentru verificare..." -ForegroundColor Yellow