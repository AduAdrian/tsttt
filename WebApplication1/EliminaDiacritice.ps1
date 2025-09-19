# Script PowerShell pentru ELIMINAREA diacriticelor din aplica?ia Miseda Inspect SRL
# Înlocuie?te toate diacriticele cu echivalentele f?r? diacritice pentru a evita problemele de encoding

param(
    [switch]$WhatIf,    # Doar afi?eaz? ce ar schimba, f?r? a face modific?ri
    [switch]$Verbose    # Afi?eaz? informa?ii detaliate
)

Write-Host "=== ELIMINAREA DIACRITICELOR - MISEDA INSPECT SRL ===" -ForegroundColor Magenta
Write-Host "Script pentru înlocuirea diacriticelor cu caractere simple`n" -ForegroundColor White

if ($WhatIf) {
    Write-Host "??  Mod simulare activ - nu se vor face modific?ri reale`n" -ForegroundColor Yellow
}

# Mapare pentru eliminarea diacriticelor
$eliminaDiacritice = @{
    # ? ? a
    '?' = 'a'
    '?' = 'A'
    
    # â ? a
    'â' = 'a'
    'Â' = 'A'
    
    # î ? i
    'î' = 'i'
    'Î' = 'I'
    
    # ? ? s
    '?' = 's'
    '?' = 'S'
    
    # ? ? t
    '?' = 't'
    '?' = 'T'
    
    # Cazuri problematice de encoding
    '?' = 'a'
    '?' = 'A'
    'â' = 'a'
    'Â' = 'A'
    'î' = 'i'
    'Î' = 'I'
    '?' = 's'
    '?' = 'S'
    '?' = 't'
    '?' = 'T'
    
    # Caractere problematice cu ?
    '?' = 'a'  # În cazul în care ? reprezint? ?
    '?' = 'i'  # În cazul în care ? reprezint? î
    '?' = 's'  # În cazul în care ? reprezint? ?
    '?' = 't'  # În cazul în care ? reprezint? ?
}

# Mapare specific? pentru cuvintele din aplica?ie
$cuvinteFaraDiacritice = @{
    # Cuvinte cu ? ? a
    'clien?i' = 'clienti'
    'Clien?i' = 'Clienti'
    'înmatriculare' = 'inmatriculare'
    'Înmatriculare' = 'Inmatriculare'
    'num?r' = 'numar'
    'Num?r' = 'Numar'
    'c?tre' = 'catre'
    'C?tre' = 'Catre'
    'f?r?' = 'fara'
    'F?r?' = 'Fara'
    'dep??i' = 'depaseste'
    'dep??e?te' = 'depaseste'
    'r?mase' = 'ramase'
    'R?mase' = 'Ramase'
    'program?ri' = 'programari'
    'Program?ri' = 'Programari'
    'v?' = 'va'
    'V?' = 'Va'
    'bun?' = 'buna'
    'Bun?' = 'Buna'
    
    # Cuvinte cu â ? a
    'întârziere' = 'intarziere'
    'Întârziere' = 'Intarziere'
    'mâine' = 'maine'
    'Mâine' = 'Maine'
    'pân?' = 'pana'
    'Pân?' = 'Pana'
    'român?' = 'romana'
    'Român?' = 'Romana'
    'România' = 'Romania'
    
    # Cuvinte cu î ? i
    'înapoi' = 'inapoi'
    'Înapoi' = 'Inapoi'
    'în' = 'in'
    'În' = 'In'
    'între' = 'intre'
    'Între' = 'Intre'
    'încerci' = 'incerci'
    'Încerci' = 'Incerci'
    'încearc?' = 'incearca'
    'Încearc?' = 'Incearca'
    
    # Cuvinte cu ? ? s
    'a?teapt?' = 'asteapta'
    'A?teapt?' = 'Asteapta'
    'a?teptare' = 'asteptare'
    'A?teptare' = 'Asteptare'
    '?i' = 'si'
    '?i' = 'Si'
    '?ters' = 'sters'
    '?ters' = 'Sters'
    'gre?it' = 'gresit'
    'Gre?it' = 'Gresit'
    
    # Cuvinte cu ? ? t
    'aten?ie' = 'atentie'
    'Aten?ie' = 'Atentie'
    'func?ie' = 'functie'
    'Func?ie' = 'Functie'
    'sec?iune' = 'sectiune'
    'Sec?iune' = 'Sectiune'
    'loca?ie' = 'locatie'
    'Loca?ie' = 'Locatie'
    'notific?ri' = 'notificari'
    'Notific?ri' = 'Notificari'
    
    # Verbe specifice
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
    
    # Expresii frecvente
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
    'ast?zi' = 'astazi'
    'Ast?zi' = 'Astazi'
    'curând' = 'curand'
    'Curând' = 'Curand'
    
    # Loca?ii
    'R?d?u?i' = 'Radauti'
    'r?d?u?i' = 'radauti'
}

# G?se?te toate fi?ierele relevante
$searchPatterns = @("*.cs", "*.cshtml", "*.json", "*.md")
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
        
        # Primul pas: înlocuie?te cuvintele complete
        foreach ($entry in $cuvinteFaraDiacritice.GetEnumerator()) {
            $oldValue = $entry.Key
            $newValue = $entry.Value
            
            # Folose?te \b pentru word boundaries pentru a evita înlocuirile par?iale
            $pattern = '\b' + [regex]::Escape($oldValue) + '\b'
            if ($content -match $pattern) {
                $replacementCount = ([regex]::Matches($content, $pattern)).Count
                $content = $content -replace $pattern, $newValue
                $fileReplacements += $replacementCount
                $totalReplacements += $replacementCount
                
                if ($Verbose) {
                    Write-Host "    ? '$oldValue' ? '$newValue' ($replacementCount înlocuiri)" -ForegroundColor Gray
                }
            }
        }
        
        # Al doilea pas: înlocuie?te caracterele individuale r?mase
        foreach ($entry in $eliminaDiacritice.GetEnumerator()) {
            $oldChar = $entry.Key
            $newChar = $entry.Value
            
            if ($content.Contains($oldChar)) {
                $replacementCount = ($content.ToCharArray() | Where-Object { $_ -eq $oldChar }).Count
                $content = $content.Replace($oldChar, $newChar)
                $fileReplacements += $replacementCount
                $totalReplacements += $replacementCount
                
                if ($Verbose) {
                    Write-Host "    ? '$oldChar' ? '$newChar' ($replacementCount înlocuiri)" -ForegroundColor DarkGray
                }
            }
        }
        
        # Verific? dac? con?inutul s-a modificat
        if ($content -ne $originalContent) {
            if (-not $WhatIf) {
                Set-Content $file.FullName -Value $content -Encoding UTF8 -NoNewline
            }
            Write-Host "? $(if($WhatIf){'[SIMULARE] '}else{''})$($file.Name) - $fileReplacements înlocuiri" -ForegroundColor Green
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
Write-Host "?? FINALIZAT! Eliminarea diacriticelor completat?." -ForegroundColor Green

if ($WhatIf) {
    Write-Host "??  SIMULARE - Nu s-au f?cut modific?ri reale!" -ForegroundColor Yellow
}

Write-Host "`n?? STATISTICI:" -ForegroundColor Cyan
Write-Host "   • Fi?iere procesate: $totalFiles" -ForegroundColor White
Write-Host "   • Fi?iere modificate: $modifiedFiles" -ForegroundColor White
Write-Host "   • Total înlocuiri: $totalReplacements" -ForegroundColor White

if ($modifiedFiles -gt 0) {
    Write-Host "`n?? TIPURI DE ELIMIN?RI APLICATE:" -ForegroundColor Yellow
    Write-Host "   • ? ? a (clien?i ? clienti)" -ForegroundColor White
    Write-Host "   • â ? a (mâine ? maine)" -ForegroundColor White  
    Write-Host "   • î ? i (înapoi ? inapoi)" -ForegroundColor White
    Write-Host "   • ? ? s (a?teapt? ? asteapta)" -ForegroundColor White
    Write-Host "   • ? ? t (aten?ie ? atentie)" -ForegroundColor White
    
    Write-Host "`n?? URM?TORII PA?I:" -ForegroundColor Magenta
    Write-Host "   1. Ruleaz?: dotnet build" -ForegroundColor White
    Write-Host "   2. Testeaz? aplica?ia" -ForegroundColor White
    Write-Host "   3. Commit modific?rile în Git" -ForegroundColor White
    Write-Host "`n   Comand? rapid?: dotnet build && echo 'Build reu?it!'" -ForegroundColor Gray
    
    Write-Host "`n??  NOT? IMPORTANT?:" -ForegroundColor Yellow
    Write-Host "   Aplica?ia va avea textele în român? dar f?r? diacritice." -ForegroundColor White
    Write-Host "   Aceasta evit? problemele de encoding ?i este mai compatibil?." -ForegroundColor White
} else {
    Write-Host "`n? Nu au fost g?site diacritice de eliminat. Aplica?ia este deja simplificat?!" -ForegroundColor Green
}

Write-Host "`n?? Pentru simulare: .\EliminaDiacritice.ps1 -WhatIf" -ForegroundColor Cyan
Write-Host "?? Pentru detalii: .\EliminaDiacritice.ps1 -Verbose`n" -ForegroundColor Cyan