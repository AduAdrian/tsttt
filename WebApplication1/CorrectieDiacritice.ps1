# Script PowerShell pentru corectarea COMPLET? a diacriticelor �n aplica?ia Miseda Inspect SRL
# Versiunea �mbun?t??it? - Ruleaz? din directorul r?d?cin? al proiectului

param(
    [switch]$WhatIf,    # Doar afi?eaz? ce ar schimba, f?r? a face modific?ri
    [switch]$Verbose    # Afi?eaz? informa?ii detaliate
)

Write-Host "=== CORECTAREA DIACRITICELOR - MISEDA INSPECT SRL ===" -ForegroundColor Magenta
Write-Host "Script �mbun?t??it pentru corectarea automat? a diacriticelor rom�ne?ti`n" -ForegroundColor White

if ($WhatIf) {
    Write-Host "??  Mod simulare activ - nu se vor face modific?ri reale`n" -ForegroundColor Yellow
}

# Mapare COMPLET? pentru diacriticele rom�ne?ti
$diacriticeMap = @{
    # Caractere cu probleme de encoding
    'Clien?i' = 'Clien?i'
    'clien?i' = 'clien?i'
    'notific?ri' = 'notific?ri'
    'Notific?ri' = 'Notific?ri'
    'program?ri' = 'program?ri'
    'Program?ri' = 'Program?ri'
    '�nm?triculare' = '�nmatriculare'
    '�nm?triculare' = '�nmatriculare'
    'Num?r' = 'Num?r'
    'num?r' = 'num?r'
    'Num?rul' = 'Num?rul'
    'num?rul' = 'num?rul'
    'Telefon' = 'Telefon'
    'telefon' = 'telefon'
    'aten?ie' = 'aten?ie'
    'Aten?ie' = 'Aten?ie'
    'a?teapt?' = 'a?teapt?'
    'A?teapt?' = 'A?teapt?'
    'a?teptare' = 'a?teptare'
    'A?teptare' = 'A?teptare'
    '�nv???m?nt' = '�nv???m�nt'
    '�nv???m?nt' = '�nv???m�nt'
    'dep??i' = 'dep??i'
    'Dep??i' = 'Dep??i'
    'dep??e?te' = 'dep??e?te'
    'Dep??e?te' = 'Dep??e?te'
    'cre?rii' = 'cre?rii'
    'Cre?rii' = 'Cre?rii'
    'f?r?' = 'f?r?'
    'F?r?' = 'F?r?'
    
    # ? -> ?
    'clien?i' = 'clien?i'
    'Clien?i' = 'Clien?i'
    '�nm?triculare' = '�nmatriculare'
    '�nm?triculare' = '�nmatriculare'
    'num?r' = 'num?r'
    'Num?r' = 'Num?r'
    'dep??i' = 'dep??i'
    'f?r?' = 'f?r?'
    'F?r?' = 'F?r?'
    
    # ? -> �
    '�nt?mpl?' = '�nt�mpl?'
    '�nt?mpl?' = '�nt�mpl?'
    '�t?rnare' = 'at�rnare'
    '�nt�rzia' = '�nt�rzia'
    '�nt�rziere' = '�nt�rziere'
    'p�n?' = 'p�n?'
    'P�n?' = 'P�n?'
    'Rom�nia' = 'Rom�nia'
    'rom�n?' = 'rom�n?'
    'Rom�n?' = 'Rom�n?'
    'c�t' = 'c�t'
    'C�t' = 'C�t'
    
    # ? -> �  
    '�napoi' = '�napoi'
    '�napoi' = '�napoi'
    '�ncerci' = '�ncerci'
    '�ncerci' = '�ncerci'
    '�ncerc?' = '�ncearc?'
    '�ncerc?' = '�ncearc?'
    '�ntre' = '�ntre'
    '�ntre' = '�ntre'
    '�n' = '�n'
    '�n' = '�n'
    '�nv???m�nt' = '�nv???m�nt'
    '�nv???m�nt' = '�nv???m�nt'
    
    # ? -> ?
    'a?teapt?' = 'a?teapt?'
    'A?teapt?' = 'A?teapt?'
    'a?teptare' = 'a?teptare'
    'A?teptare' = 'A?teptare'
    '�nce?' = '�ncerci'
    '�nce?' = '�ncerci'
    'gre?it' = 'gre?it'
    'Gre?it' = 'Gre?it'
    '?ters' = '?ters'
    '?ters?' = '?tears?'
    '?i' = '?i'
    '?i' = '?i'
    
    # ? -> ?
    'aten?ie' = 'aten?ie'
    'Aten?ie' = 'Aten?ie'
    'func?ie' = 'func?ie'
    'Func?ie' = 'Func?ie'
    'sec?iune' = 'sec?iune'
    'Sec?iune' = 'Sec?iune'
    'loca?ie' = 'loca?ie'
    'Loca?ie' = 'Loca?ie'
    
    # Corectarea unor cuvinte specifice aplica?iei
    'expira' = 'expir?'
    'Expira' = 'Expir?'
    'cur?nd' = 'cur�nd'
    'Cur?nd' = 'Cur�nd'
    'maine' = 'm�ine'
    'Maine' = 'M�ine'
    'astazi' = 'ast?zi'
    'Astazi' = 'Ast?zi'
    'buna' = 'bun?'
    'Buna' = 'Bun?'
    'inspectie' = 'inspec?ie'
    'Inspectie' = 'Inspec?ie'
    'tehnica' = 'tehnic?'
    'Tehnica' = 'Tehnic?'
    'noua' = 'nou?'
    'Noua' = 'Nou?'
    'v? rugam' = 'v? rug?m'
    'V? rugam' = 'V? rug?m'
    'va rugam' = 'v? rug?m'
    'Va rugam' = 'V? rug?m'
    'v? a?teptam' = 'v? a?tept?m'
    'V? a?teptam' = 'V? a?tept?m'
    'va asteptam' = 'v? a?tept?m'
    'Va asteptam' = 'V? a?tept?m'
    'c?tre' = 'c?tre'
    'C?tre' = 'C?tre'
    'Radauti' = 'R?d?u?i'
    'radauti' = 'r?d?u?i'
    'statia' = 'sta?ia'
    'Statia' = 'Sta?ia'
    
    # Mesaje specifice SMS/Email
    'Buna ziua, expira ITP' = 'Bun? ziua, expir? ITP'
    'pentru o noua inspectie tehnica' = 'pentru o nou? inspec?ie tehnic?'
    'Pentru o noua inspectie tehnologica' = 'Pentru o nou? inspec?ie tehnologic?'
    'va rugam apelati' = 'v? rug?m apela?i'
    'Va rugam apelati' = 'V? rug?m apela?i'
    'sau va asteptam' = 'sau v? a?tept?m'
    'Sau va asteptam' = 'Sau v? a?tept?m'
    'zi buna va dorim' = 'zi bun? v? dorim'
    'Zi buna va dorim' = 'Zi bun? v? dorim'
    
    # Mesaje de eroare ?i validare
    'este obligatoriu' = 'este obligatoriu'
    'Este obligatoriu' = 'Este obligatoriu'
    'nu poate depasi' = 'nu poate dep??i'
    'Nu poate depasi' = 'Nu poate dep??i'
    'caracterele' = 'caracterele'
    'Caracterele' = 'Caracterele'
    'formatul' = 'formatul'
    'Formatul' = 'Formatul'
    'adresa de email' = 'adresa de email'
    'Adresa de email' = 'Adresa de email'
    'parola' = 'parola'
    'Parola' = 'Parola'
    'parolele nu coincid' = 'parolele nu coincid'
    'Parolele nu coincid' = 'Parolele nu coincid'
    'utilizatorul' = 'utilizatorul'
    'Utilizatorul' = 'Utilizatorul'
    'conectare' = 'conectare'
    'Conectare' = 'Conectare'
    'deconectare' = 'deconectare'
    'Deconectare' = 'Deconectare'
    'inregistrare' = '�nregistrare'
    'Inregistrare' = '�nregistrare'
    'confirmare' = 'confirmare'
    'Confirmare' = 'Confirmare'
    
    # UI specific
    'Dashboard' = 'Dashboard'
    'Control Panel' = 'Control Panel'
    'Acasa' = 'Acas?'
    'acasa' = 'acas?'
    'Inapoi' = '�napoi'
    'inapoi' = '�napoi'
    'Inapoi la' = '�napoi la'
    'inapoi la' = '�napoi la'
    'Salveaza' = 'Salveaz?'
    'salveaza' = 'salveaz?'
    'Editeaza' = 'Editeaz?'
    'editeaza' = 'editeaz?'
    'Adauga' = 'Adaug?'
    'adauga' = 'adaug?'
    'Dezactiveaza' = 'Dezactiveaz?'
    'dezactiveaza' = 'dezactiveaz?'
    'Activeaza' = 'Activeaz?'
    'activeaza' = 'activeaz?'
    'Actualizeaza' = 'Actualizeaz?'
    'actualizeaza' = 'actualizeaz?'
    'Anuleaza' = 'Anuleaz?'
    'anuleaza' = 'anuleaz?'
    'Continua' = 'Continu?'
    'continua' = 'continu?'
}

# G?se?te toate fi?ierele relevante
$searchPatterns = @("*.cs", "*.cshtml", "*.json", "*.md", "*.txt")
$excludePaths = @("\\obj\\", "\\bin\\", "\\node_modules\\", "\\.git\\", "\\wwwroot\\lib\\", "\\Migrations\\", "\\packages\\")

$allFiles = @()
foreach ($pattern in $searchPatterns) {
    $files = Get-ChildItem -Path . -Recurse -Include $pattern
    foreach ($file in $files) {
        $shouldExclude = $false
        foreach ($excludePath in $excludePaths) {
            if ($file.FullName -match $excludePath) {
                $shouldExclude = $true
                break
            }
        }
        if (-not $shouldExclude) {
            $allFiles += $file
        }
    }
}

$totalFiles = $allFiles.Count
$modifiedFiles = 0
$totalReplacements = 0

Write-Host "?? G?site $totalFiles fi?iere pentru procesare..." -ForegroundColor Cyan

foreach ($file in $allFiles) {
    try {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8 -ErrorAction Stop
        $originalContent = $content
        $fileReplacements = 0
        
        # Aplic? toate corecturile de diacritice
        foreach ($entry in $diacriticeMap.GetEnumerator()) {
            $oldValue = $entry.Key
            $newValue = $entry.Value
            
            if ($content -match [regex]::Escape($oldValue)) {
                $replacementCount = ([regex]::Matches($content, [regex]::Escape($oldValue))).Count
                $content = $content -replace [regex]::Escape($oldValue), $newValue
                $fileReplacements += $replacementCount
                $totalReplacements += $replacementCount
                
                if ($Verbose) {
                    Write-Host "    ? '$oldValue' ? '$newValue' ($replacementCount �nlocuiri)" -ForegroundColor Gray
                }
            }
        }
        
        # Verific? dac? con?inutul s-a modificat
        if ($content -ne $originalContent) {
            if (-not $WhatIf) {
                Set-Content $file.FullName -Value $content -Encoding UTF8 -NoNewline
            }
            Write-Host "? $(if($WhatIf){'[SIMULARE] '}else{''})$($file.Name) - $fileReplacements �nlocuiri" -ForegroundColor Green
            $modifiedFiles++
        } elseif ($Verbose) {
            Write-Host "? $($file.Name) - f?r? modific?ri" -ForegroundColor Gray
        }
    }
    catch {
        Write-Host "? Eroare la procesarea: $($file.Name) - $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Rezultate finale
Write-Host "`n" + ("=" * 50) -ForegroundColor Magenta
Write-Host "?? FINALIZAT! Corectarea diacriticelor completat?." -ForegroundColor Green

if ($WhatIf) {
    Write-Host "??  SIMULARE - Nu s-au f?cut modific?ri reale!" -ForegroundColor Yellow
}

Write-Host "`n?? STATISTICI:" -ForegroundColor Cyan
Write-Host "   � Fi?iere procesate: $totalFiles" -ForegroundColor White
Write-Host "   � Fi?iere modificate: $modifiedFiles" -ForegroundColor White
Write-Host "   � Total �nlocuiri: $totalReplacements" -ForegroundColor White

if ($modifiedFiles -gt 0) {
    Write-Host "`n?? TIPURI DE CORECT?RI APLICATE:" -ForegroundColor Yellow
    Write-Host "   � ? ? ? (clien?i ? clien?i)" -ForegroundColor White
    Write-Host "   � ? ? � (�nt�mpl? ? �nt�mpl?)" -ForegroundColor White  
    Write-Host "   � ? ? � (�nv???m?nt ? �nv???m�nt)" -ForegroundColor White
    Write-Host "   � ? ? ? (a?teapt? ? a?teapt?)" -ForegroundColor White
    Write-Host "   � ? ? ? (aten?ie ? aten?ie)" -ForegroundColor White
    Write-Host "   � Cuvinte specifice: expira ? expir?, buna ? bun?, etc." -ForegroundColor White
    
    Write-Host "`n?? URM?TORII PA?I:" -ForegroundColor Magenta
    Write-Host "   1. Ruleaz?: dotnet build" -ForegroundColor White
    Write-Host "   2. Testeaz? aplica?ia" -ForegroundColor White
    Write-Host "   3. Commit modific?rile �n Git" -ForegroundColor White
    Write-Host "`n   Comand? rapid?: dotnet build && echo 'Build reu?it!'" -ForegroundColor Gray
} else {
    Write-Host "`n? Nu au fost g?site diacritice de corectat. Aplica?ia este deja corect?!" -ForegroundColor Green
}

Write-Host "`n?? Pentru simulare (f?r? modific?ri reale): .\CorrectieDiacritice.ps1 -WhatIf" -ForegroundColor Cyan
Write-Host "?? Pentru detalii complete: .\CorrectieDiacritice.ps1 -Verbose`n" -ForegroundColor Cyan