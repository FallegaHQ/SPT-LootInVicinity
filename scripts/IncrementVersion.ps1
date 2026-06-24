# Increments patch in the .csproj and regenerates src/PluginInfo.Version.g.cs (single version source).
param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectDir
)

$ErrorActionPreference = 'Stop'

$csproj = Get-ChildItem -Path $ProjectDir -Filter '*.csproj' -File | Select-Object -First 1
if (-not $csproj)
{
    throw "No .csproj found in $ProjectDir"
}

$csprojPath = $csproj.FullName
$csprojText = [System.IO.File]::ReadAllText($csprojPath)

if ($csprojText -notmatch '<RootNamespace>([^<]+)</RootNamespace>')
{
    throw 'RootNamespace not found in csproj.'
}
$rootNamespace = $Matches[1].Trim()

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

$versionFile = Join-Path $ProjectDir 'src\PluginInfo.Version.g.cs'
$versionContent = @"
namespace $rootNamespace;

internal static partial class PluginInfo
{
    public const string PLUGIN_VERSION = "$newVersion";
}
"@
[System.IO.File]::WriteAllText($versionFile, $versionContent, $utf8NoBom)

Write-Output $newVersion
