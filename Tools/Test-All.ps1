# Checks run by Deploy-Mod before deploying: translation keys match the source, no loops over shared PawnsFinder lists, no tiny fonts, both game versions, preview size.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$failures = New-Object System.Collections.Generic.List[string]

$sourceFiles = Get-ChildItem (Join-Path $root 'Source') -Recurse -Filter *.cs
$sourceText = ($sourceFiles | ForEach-Object { [IO.File]::ReadAllText($_.FullName) }) -join "`n"

$used = New-Object System.Collections.Generic.HashSet[string]
foreach ($m in [regex]::Matches($sourceText, '"(ANB\.[A-Za-z]+)"')) { [void]$used.Add($m.Groups[1].Value) }

$keyed = New-Object System.Collections.Generic.HashSet[string]
foreach ($file in Get-ChildItem (Join-Path $root 'Languages\English\Keyed') -Filter *.xml) {
    [xml]$xml = [IO.File]::ReadAllText($file.FullName)
    foreach ($node in $xml.LanguageData.ChildNodes) {
        if ($node.NodeType -eq 'Element' -and -not $keyed.Add($node.Name)) { $failures.Add("Duplicate key $($node.Name)") }
    }
}

foreach ($key in $used) { if (-not $keyed.Contains($key)) { $failures.Add("Missing translation: $key") } }
foreach ($key in $keyed) { if (-not $used.Contains($key)) { $failures.Add("Unused translation: $key") } }

# PawnsFinder lists are shared buffers refilled on every call; looping over one directly breaks when naming reads it again
foreach ($file in $sourceFiles) {
    $text = [IO.File]::ReadAllText($file.FullName)
    foreach ($m in [regex]::Matches($text, 'foreach\s*\([^)]*\bin\s+PawnsFinder\.\w+\s*\)')) {
        $failures.Add("$($file.Name) loops directly over a PawnsFinder list: $($m.Value)")
    }
}

if ($sourceText -match 'GameFont\.Tiny') { $failures.Add('GameFont.Tiny is used') }

foreach ($version in '1.6', '1.7') {
    if (-not (Test-Path (Join-Path $root "$version\Assemblies\AutoNameBabies.dll"))) { $failures.Add("$version\Assemblies\AutoNameBabies.dll is missing") }
}
[xml]$about = [IO.File]::ReadAllText((Join-Path $root 'About\About.xml'))
$versions = @($about.ModMetaData.supportedVersions.li)
foreach ($version in '1.6', '1.7') { if ($versions -notcontains $version) { $failures.Add("About.xml supportedVersions does not list $version") } }

$preview = Join-Path $root 'About\Preview.png'
if ((Get-Item $preview).Length -ge 1000000) { $failures.Add('About\Preview.png is 1,000,000 bytes or more') }

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "FAIL $_" }
    exit 1
}
Write-Host "Test-All passed: $($keyed.Count) keys checked"
