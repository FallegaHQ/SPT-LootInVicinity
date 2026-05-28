<#
.SYNOPSIS
  Checks TarkovDir, required game/BepInEx DLLs, and optional SPT plugin layout.

.PARAMETER ProjectDir
  Mod project root (folder with .csproj). Defaults to parent of scripts/.

.PARAMETER TarkovDir
  SPT install path. If omitted, reads Directory.Build.props.user then Directory.Build.props.
#>
param(
    [string]$ProjectDir,
    [string]$TarkovDir
)

$ErrorActionPreference = 'Stop'

if ( [string]::IsNullOrWhiteSpace($ProjectDir))
{
    $ProjectDir = Split-Path $PSScriptRoot -Parent
}

function Read-TarkovDirFromProps
{
    param([string]$Dir)
    foreach ($file in @('Directory.Build.props.user', 'Directory.Build.props'))
    {
        $path = Join-Path $Dir $file
        if (-not (Test-Path $path))
        {
            continue
        }
        $text = [IO.File]::ReadAllText($path)
        if ($text -match '<TarkovDir[^>]*>([^<]+)</TarkovDir>')
        {
            return $Matches[1].Trim()
        }
    }
    return $null
}

if ( [string]::IsNullOrWhiteSpace($TarkovDir))
{
    $TarkovDir = Read-TarkovDirFromProps -Dir $ProjectDir
}

Write-Host ''
Write-Host '  Validate SPT install' -ForegroundColor Cyan
Write-Host ('  Project: ' + $ProjectDir) -ForegroundColor DarkGray
Write-Host ''

$errors = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()

if ( [string]::IsNullOrWhiteSpace($TarkovDir))
{
    $errors.Add('TarkovDir is not set. Add Directory.Build.props.user with <TarkovDir>...</TarkovDir>.')
}
else
{
    Write-Host "  TarkovDir: $TarkovDir"

    if (-not (Test-Path -LiteralPath $TarkovDir))
    {
        $errors.Add("Path does not exist: $TarkovDir")
    }
    else
    {
        $checks = @(
            @{ Label = 'Assembly-CSharp'; Path = Join-Path $TarkovDir 'EscapeFromTarkov_Data\Managed\Assembly-CSharp.dll' },
            @{ Label = 'BepInEx'; Path = Join-Path $TarkovDir 'BepInEx\core\BepInEx.dll' },
            @{ Label = 'spt-common'; Path = Join-Path $TarkovDir 'BepInEx\plugins\spt\spt-common.dll' },
            @{ Label = 'spt-reflection'; Path = Join-Path $TarkovDir 'BepInEx\plugins\spt\spt-reflection.dll' },
            @{ Label = '0Harmony'; Path = Join-Path $TarkovDir 'BepInEx\core\0Harmony.dll' }
        )

        foreach ($check in $checks)
        {
            if (Test-Path -LiteralPath $check.Path)
            {
                Write-Host ('    OK  ' + $check.Label) -ForegroundColor Green
            }
            else
            {
                $errors.Add("Missing $( $check.Label ): $( $check.Path )")
            }
        }

        $pluginsDir = Join-Path $TarkovDir 'BepInEx\plugins'
        if (Test-Path $pluginsDir)
        {
            $pluginCount = (Get-ChildItem -Path $pluginsDir -Directory -ErrorAction SilentlyContinue).Count
            Write-Host "    OK  plugins folder ($pluginCount subfolders)" -ForegroundColor Green
        }
        else
        {
            $warnings.Add("Plugins folder not found: $pluginsDir")
        }
    }
}

$csproj = Get-ChildItem -Path $ProjectDir -Filter '*.csproj' -File -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $csproj)
{
    $warnings.Add('No .csproj in project directory.')
}
else
{
    Write-Host ''
    Write-Host "  Project file: $( $csproj.Name )" -ForegroundColor DarkGray
    $text = [IO.File]::ReadAllText($csproj.FullName)
    if ($text -match '<Version>([^<]+)</Version>')
    {
        Write-Host "    Version: $( $Matches[1] )"
    }
    if ($text -match '<AssemblyName>([^<]+)</AssemblyName>')
    {
        Write-Host "    Assembly: $( $Matches[1] )"
    }
}

if ($warnings.Count -gt 0)
{
    Write-Host ''
    Write-Host '  Warnings' -ForegroundColor Yellow
    foreach ($w in $warnings)
    {
        Write-Host "    - $w" -ForegroundColor Yellow
    }
}

if ($errors.Count -gt 0)
{
    Write-Host ''
    Write-Host '  Failed' -ForegroundColor Red
    foreach ($e in $errors)
    {
        Write-Host "    - $e" -ForegroundColor Red
    }
    Write-Host ''
    exit 1
}

Write-Host ''
Write-Host '  Install check passed.' -ForegroundColor Green
Write-Host ''
exit 0
