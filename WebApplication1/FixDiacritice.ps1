# Script RAPID pentru eliminarea diacriticelor din aplica?ie
# �nlocuie?te �n memorie toate diacriticele cu caractere simple

Write-Host "?? Script RAPID - Eliminare Diacritice" -ForegroundColor Green

# Simple mappings - doar caracterele de baz?
$chars = @{
    '?' = 'a'; '?' = 'A'
    '�' = 'a'; '�' = 'A' 
    '�' = 'i'; '�' = 'I'
    '?' = 's'; '?' = 'S'
    '?' = 't'; '?' = 'T'
    '?' = 't'; '?' = 'T'
    # Problematic characters
    'Clien?i' = 'Clienti'
    'clien?i' = 'clienti'
    'num?r' = 'numar'
    'Num?r' = 'Numar'
    '�nm?triculare' = 'inmatriculare'
    '�nm?triculare' = 'Inmatriculare'
    'notific?ri' = 'notificari'
    'Notific?ri' = 'Notificari'
    'program?ri' = 'programari'
    'Program?ri' = 'Programari'
    'aten?ie' = 'atentie'
    'Aten?ie' = 'Atentie'
    'a?teapt?' = 'asteapta'
    'A?teapt?' = 'Asteapta'
    'f?r?' = 'fara'
    'F?r?' = 'Fara'
}

# Quick words replacement
$words = @{
    'expir?' = 'expira'
    'adaug?' = 'adauga' 
    'editeaz?' = 'editeaza'
    'salveaz?' = 'salveaza'
    '�napoi' = 'inapoi'
    '�napoi' = 'Inapoi'
    '�n' = 'in'
    '�n' = 'In'
    'c?tre' = 'catre'
    'C?tre' = 'Catre'
    '?i' = 'si'
    '?i' = 'Si'
    'v?' = 'va'
    'V?' = 'Va'
    'bun?' = 'buna'
    'Bun?' = 'Buna'
    'ast?zi' = 'astazi'
    'm�ine' = 'maine'
    'cur�nd' = 'curand'
    'a?teapt?' = 'asteapta'
    'r?mase' = 'ramase'
    'R?mase' = 'Ramase'
}

# Combine all replacements
$allReplacements = @{}
$allReplacements += $chars
$allReplacements += $words

# Get files
$files = Get-ChildItem -Path . -Recurse -Include "*.cs","*.cshtml" | 
    Where-Object { $_.FullName -notmatch "\\obj\\|\\bin\\|\\node_modules\\|\\.git\\" }

$count = 0
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content
    
    foreach ($pair in $allReplacements.GetEnumerator()) {
        $content = $content.Replace($pair.Key, $pair.Value)
    }
    
    if ($content -ne $original) {
        Set-Content $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "? $($file.Name)" -ForegroundColor Green
        $count++
    }
}

Write-Host "`n?? Finalizat! $count fisiere modificate." -ForegroundColor Cyan
Write-Host "Ruleaza: dotnet build" -ForegroundColor Yellow