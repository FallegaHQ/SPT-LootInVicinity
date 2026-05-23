# Writes text files as UTF-8 without BOM and LF line endings (.editorconfig).
param(
    [string]$ProjectDir = (Split-Path $PSScriptRoot -Parent)
)

$ErrorActionPreference = 'Stop'
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
$extensions = @(
    '.cs', '.csproj', '.props', '.md', '.ps1', '.sln', '.yml', '.yaml', '.json',
    '.mdc', '.editorconfig', '.gitignore', '.gitattributes'
)

$files = Get-ChildItem -Path $ProjectDir -Recurse -File | Where-Object {
    $_.FullName -notmatch '\\(bin|obj|\.git)\\' -and ($extensions -contains $_.Extension)
}

foreach ($file in $files)
{
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    {
        $text = $utf8NoBom.GetString($bytes, 3, $bytes.Length - 3)
    }
    else
    {
        $text = $utf8NoBom.GetString($bytes)
    }

    $text = $text -replace "`r`n", "`n" -replace "`r", "`n"
    [System.IO.File]::WriteAllText($file.FullName, $text, $utf8NoBom)
}

Write-Host "Re-encoded $( $files.Count ) files to UTF-8 (no BOM) + LF under $ProjectDir"
