# Increments the patch segment (third number) in Softwyx-LootInVicinity.csproj and src/LootInVicinityPlugin.cs.
# Edit major/minor in the csproj manually; each dotnet build bumps patch (e.g. 2.16.0 -> 2.16.1).
param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectDir
)

$ErrorActionPreference = 'Stop'

$csprojPath = Join-Path $ProjectDir 'Softwyx-LootInVicinity.csproj'
$pluginPath = Join-Path $ProjectDir 'src\LootInVicinityPlugin.cs'

if (-not (Test-Path $csprojPath))
{
    throw "Project file not found: $csprojPath"
}
if (-not (Test-Path $pluginPath))
{
    throw "Plugin file not found: $pluginPath"
}

$csprojText = [System.IO.File]::ReadAllText($csprojPath)
$versionMatch = [regex]::Match($csprojText, '<Version>(\d+)\.(\d+)\.(\d+)</Version>')

if (-not $versionMatch.Success)
{
    throw "Could not find <Version>major.minor.patch</Version> in $csprojPath"
}

$major = [int]$versionMatch.Groups[1].Value
$minor = [int]$versionMatch.Groups[2].Value
$patch = [int]$versionMatch.Groups[3].Value + 1
$newVersion = '{0}.{1}.{2}' -f $major, $minor, $patch

$utf8NoBom = New-Object System.Text.UTF8Encoding $false

$csprojText = [regex]::Replace(
        $csprojText,
        '<Version>\d+\.\d+\.\d+</Version>',
        "<Version>$newVersion</Version>",
        1
)
[System.IO.File]::WriteAllText($csprojPath, $csprojText, $utf8NoBom)

$pluginText = [System.IO.File]::ReadAllText($pluginPath)
$pluginText = [regex]::Replace(
        $pluginText,
        'PLUGIN_VERSION = "\d+\.\d+\.\d+"',
        "PLUGIN_VERSION = `"$newVersion`"",
        1
)
[System.IO.File]::WriteAllText($pluginPath, $pluginText, $utf8NoBom)

Write-Output $newVersion
