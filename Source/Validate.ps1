$ErrorActionPreference = 'Stop'
$modRoot = Split-Path $PSScriptRoot -Parent
$xmlFiles = Get-ChildItem $modRoot -Recurse -Filter '*.xml'
foreach ($file in $xmlFiles) { [xml]$parsed = Get-Content -LiteralPath $file.FullName -Raw }
[xml]$definitions = Get-Content "$modRoot/Defs/Campfire.xml" -Raw
$baseline = @{}
$englishRoot = "$modRoot/Languages/English"
foreach($file in Get-ChildItem $englishRoot -Recurse -Filter '*.xml') {
    [xml]$document = Get-Content $file.FullName -Raw
    $relative = $file.FullName.Substring($englishRoot.Length + 1)
    $baseline[$relative] = @{}
    foreach($entry in $document.LanguageData.ChildNodes) { if($entry.NodeType -eq 'Element') { $baseline[$relative][$entry.Name]=$entry.InnerText } }
}
foreach($language in @('English','ChineseTraditional','ChineseSimplified','Japanese','Korean')) {
    $count = 0
    foreach($relative in $baseline.Keys) {
        [xml]$document = Get-Content "$modRoot/Languages/$language/$relative" -Raw
        $entries = @($document.LanguageData.ChildNodes | Where-Object NodeType -eq Element)
        if($entries.Count -ne $baseline[$relative].Count) { throw "Mismatched key count: $language/$relative" }
        foreach($entry in $entries) {
            if(!$baseline[$relative].ContainsKey($entry.Name) -or [string]::IsNullOrWhiteSpace($entry.InnerText)) { throw "Invalid entry: $language/$($entry.Name)" }
            $expected = @([regex]::Matches($baseline[$relative][$entry.Name], '\{[^}]+\}') | ForEach-Object Value | Sort-Object)
            $actual = @([regex]::Matches($entry.InnerText, '\{[^}]+\}') | ForEach-Object Value | Sort-Object)
            if(($expected -join '|') -ne ($actual -join '|')) { throw "Placeholder mismatch: $language/$($entry.Name)" }
            if($relative.StartsWith('DefInjected')) {
                $parts=$entry.Name.Split('.')
                $defType=Split-Path (Split-Path $relative -Parent) -Leaf
                $node=$definitions.Defs.ChildNodes | Where-Object { $_.Name -eq $defType -and $_.defName -eq $parts[0] }
                foreach($part in $parts[1..($parts.Length-1)]) {
                    if($part -match '^\d+$') { $node=@($node.SelectNodes('li'))[[int]$part] }
                    else { $node=$node.SelectSingleNode($part) }
                    if($null -eq $node) { throw "Unresolved translated field: $($entry.Name)" }
                }
            }
            $count++
        }
    }
    Write-Output "$language : $count translated fields verified"
}
if(!(Test-Path "$modRoot/1.6/Assemblies/CampfireStories.dll")) { throw 'Runtime assembly missing.' }

Write-Output "$($xmlFiles.Count) XML files parsed; translated paths and placeholders verified."
