param(
    [string]$GameRoot = 'F:/SteamLibrary/steamapps/common/RimWorld',
    [string]$WorkshopRoot = 'F:/SteamLibrary/steamapps/workshop/content/294100'
)
$ErrorActionPreference = 'Stop'
$modRoot = Split-Path $PSScriptRoot -Parent
$managed = Join-Path $GameRoot 'RimWorldWin64_Data/Managed'
$sdk = (& dotnet --list-sdks | Select-Object -Last 1) -replace '^([^ ]+) \[([^]]+)\]$', '$2/$1'
$compiler = Join-Path $sdk 'Roslyn/bincore/csc.dll'
$refs = @('mscorlib.dll','netstandard.dll','System.dll','System.Core.dll','System.Runtime.dll','System.Runtime.Serialization.dll','Assembly-CSharp.dll','UnityEngine.CoreModule.dll','UnityEngine.IMGUIModule.dll','UnityEngine.TextRenderingModule.dll') | ForEach-Object { Join-Path $managed $_ }


$arguments = @('/nologo','/target:library','/langversion:latest','/nostdlib+','/optimize+','/deterministic+',"/out:$modRoot/1.6/Assemblies/CampfireStories.dll")
$arguments += $refs | ForEach-Object { '/reference:' + $_ }
$arguments += Get-ChildItem $PSScriptRoot -Filter '*.cs' | ForEach-Object FullName
& dotnet $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
Write-Output 'CampfireStories.dll compiled successfully.'

$arguments = @('/nologo','/target:library','/langversion:latest','/nostdlib+','/optimize+','/deterministic+',"/out:$modRoot/Integrations/RimTalk/Assemblies/CampfireStories.RimTalk.dll")
$arguments += ($refs + "$modRoot/1.6/Assemblies/CampfireStories.dll" + "$WorkshopRoot/3551203752/1.6/Assemblies/RimTalk.dll") | ForEach-Object { '/reference:' + $_ }
$arguments += Get-ChildItem "$PSScriptRoot/RimTalk" -Filter '*.cs' | ForEach-Object FullName
& dotnet $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'Optional adapter compilation failed.' }
Write-Output 'Optional RimTalk adapter compiled successfully.'
