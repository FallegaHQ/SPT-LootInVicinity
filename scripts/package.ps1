<#!
  Builds a portable release ZIP under Package\<AssemblyName>-<version>.zip

  Prerequisites:
    - dotnet build -c Package  (optimised DLL in bin\Package)
    - WinRAR with WinRAR.exe installed (GUI install adds it), OR set WINRAR to full path
    - Fallback: 7-Zip (7z) if WinRAR is not found

  Archive layout: extract at game root so BepInEx\plugins\<AssemblyName>\<AssemblyName>.dll lines up.
#>
$ErrorActionPreference = 'Stop'
$projectDir = $PSScriptRoot
if (-not (Test-Path (Join-Path $projectDir '*.csproj')))
{
    $projectDir = Split-Path $PSScriptRoot -Parent
}
Set-Location $projectDir

$packageDir = Join-Path $projectDir 'Package'
$artifactDir = Join-Path $projectDir 'bin\Package'
$stagingBepInEx = Join-Path $packageDir 'BepInEx'
$stagingPlugins = Join-Path $stagingBepInEx 'plugins'

$csproj = Get-ChildItem -Path $projectDir -Filter '*.csproj' | Where-Object { $_.Name -notmatch 'inspect|Inspect|Probe' } | Select-Object -First 1
if (-not $csproj)
{
    throw 'No mod .csproj found in project root.'
}

$csprojText = Get-Content $csproj.FullName -Raw
if ($csprojText -notmatch '<AssemblyName>([^<]+)</AssemblyName>')
{
    throw 'AssemblyName not found in csproj.'
}
$modName = $Matches[1]

if ($csprojText -notmatch '<Version>([^<]+)</Version>')
{
    throw 'Version not found in csproj.'
}
$modVersion = $Matches[1]

Write-Host "Packaging $modName v$modVersion"

$dllPath = Join-Path $artifactDir "$modName.dll"
if (-not (Test-Path $dllPath))
{
    throw @"
Build output not found: $dllPath
Run from repo root: dotnet build -c Package
Ensure Directory.Build.props (or .user) points TarkovDir at your install.
"@
}

if (Test-Path $stagingBepInEx)
{
    Remove-Item -Recurse -Force $stagingBepInEx
}

$stagingModDir = Join-Path $stagingPlugins $modName
New-Item -ItemType Directory -Path $stagingModDir -Force | Out-Null

Copy-Item -Path $dllPath -Destination (Join-Path $stagingModDir "$modName.dll") -Force

$localesSrc = Join-Path $projectDir 'locales'
if (Test-Path -LiteralPath $localesSrc)
{
    Copy-Item -LiteralPath $localesSrc -Destination $stagingModDir -Recurse -Force
}

$readmeSrc = Join-Path $projectDir 'README.md'
if (Test-Path -LiteralPath $readmeSrc)
{
    Copy-Item -LiteralPath $readmeSrc -Destination (Join-Path $stagingModDir 'README.md') -Force
}

$attributionSrc = Join-Path $projectDir 'TEMPLATE-ATTRIBUTION.md'
if (Test-Path -LiteralPath $attributionSrc)
{
    Copy-Item -LiteralPath $attributionSrc -Destination (Join-Path $stagingModDir 'TEMPLATE-ATTRIBUTION.md') -Force
}

$archivePath = Join-Path $packageDir "$modName-$modVersion.zip"
if (Test-Path $archivePath)
{
    Remove-Item -Force $archivePath
}

$winRarExe = $env:WINRAR
if ([string]::IsNullOrWhiteSpace($winRarExe) -or -not (Test-Path -LiteralPath $winRarExe))
{
    $winRarExe = $null
    foreach ($candidate in @(
        "${env:ProgramFiles}\WinRAR\WinRAR.exe",
        "${env:ProgramFiles(x86)}\WinRAR\WinRAR.exe"
    ))
    {
        if (Test-Path -LiteralPath $candidate)
        {
            $winRarExe = $candidate
            break
        }
    }
}

if ($winRarExe)
{
    Push-Location $packageDir
    try
    {
        $arguments = @('a', '-afzip', '-r', '-ibck', $archivePath, 'BepInEx')
        $process = Start-Process -FilePath $winRarExe -ArgumentList $arguments -Wait -PassThru -NoNewWindow
        if ($process.ExitCode -ge 2)
        {
            throw "WinRAR failed with exit code $( $process.ExitCode )."
        }
    }
    finally
    {
        Pop-Location
    }
}
else
{
    $sevenZip = Get-Command 7z -ErrorAction SilentlyContinue
    if (-not $sevenZip)
    {
        foreach ($p in @("${env:ProgramFiles}\7-Zip\7z.exe", "${env:ProgramFiles(x86)}\7-Zip\7z.exe"))
        {
            if (Test-Path $p)
            {
                $sevenZip = $p; break
            }
        }
    }
    if (-not $sevenZip)
    {
        throw 'WinRAR or 7-Zip required. Install WinRAR or 7-Zip, or set WINRAR to WinRAR.exe.'
    }
    $sevenZipExe = if ($sevenZip -is [string])
    {
        $sevenZip
    }
    else
    {
        $sevenZip.Source
    }
    & $sevenZipExe a $archivePath $stagingBepInEx | Out-Host
}

Write-Host "Created $archivePath"
Write-Host "Install: extract ZIP at game root so BepInEx\plugins\$modName\$modName.dll sits under your SPT/EFT folder."
