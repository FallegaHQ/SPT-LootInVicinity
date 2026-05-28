<#
.SYNOPSIS
    Authenticode-signs a built assembly. Invoked by the MSBuild SignAssembly target.

.DESCRIPTION
    Certificate resolution order:
      1. PFX file when -PfxPath exists
      2. Unexpired code-signing cert in -CertStore matching -CertSubject
      3. Auto-create self-signed cert in -CertStore (optional PFX export)

    PFX password: use -PfxPassword, or set RAD_SIGNING_PFX_PASSWORD (preferred for MSBuild).
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string] $AssemblyPath,

    [string] $CertSubject = "CN=SelfSign",
    [string] $CertStore = "Cert:\CurrentUser\My",
    [string] $PfxPath = "",
    [string] $PfxPassword = "",
    [string] $PfxExportPath = "",
    [string] $TimestampUrl = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Step([string]$msg)
{
    Write-Host "  [Sign] $msg"
}
function Write-Ok([string]$msg)
{
    Write-Host "  [Sign] OK  $msg" -ForegroundColor Green
}
function Write-Warn([string]$msg)
{
    Write-Warning "  [Sign] $msg"
}

function Resolve-OptionalPath([string]$path)
{
    if ( [string]::IsNullOrWhiteSpace($path))
    {
        return ""
    }
    if ( [System.IO.Path]::IsPathRooted($path))
    {
        return $path
    }

    $repoRoot = Split-Path $PSScriptRoot -Parent
    return (Join-Path $repoRoot $path)
}

function Load-CertFromPfx([string]$path, [string]$pwd)
{
    Write-Step "Loading certificate from PFX: $path"

    if ($pwd)
    {
        $secure = ConvertTo-SecureString $pwd -AsPlainText -Force
        return New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(
        $path, $secure,
        [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable
        )
    }

    return New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(
    $path, [string]::Empty,
    [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable
    )
}

function Find-CertInStore([string]$store, [string]$subject)
{
    Write-Step "Searching '$store' for subject '$subject'..."

    $now = Get-Date
    return Get-ChildItem $store -ErrorAction SilentlyContinue |
            Where-Object {
                $_.Subject -eq $subject -and
                        $_.NotBefore -le $now -and
                        $_.NotAfter -gt $now -and
                        $_.HasPrivateKey
            } |
            Sort-Object NotAfter -Descending |
            Select-Object -First 1
}

function New-CodeSigningCert([string]$subject, [string]$store)
{
    Write-Step "No certificate found - creating self-signed code-signing cert..."

    $cert = New-SelfSignedCertificate `
        -Subject           $subject `
        -CertStoreLocation $store `
        -Type              CodeSigningCert `
        -KeyUsage          DigitalSignature `
        -KeyAlgorithm      RSA `
        -KeyLength         4096 `
        -HashAlgorithm     SHA256 `
        -NotAfter          (Get-Date).AddYears(10)

    Write-Ok "Created self-signed certificate."
    Write-Step "  Thumbprint : $( $cert.Thumbprint )"
    Write-Step "  Subject    : $( $cert.Subject )"
    Write-Step "  Valid until: $($cert.NotAfter.ToString('yyyy-MM-dd') )"
    return $cert
}

function Export-CertToPfx(
        [System.Security.Cryptography.X509Certificates.X509Certificate2]$cert,
        [string]$exportPath,
        [string]$pwd
)
{
    $dir = Split-Path $exportPath -Parent
    if ($dir -and -not (Test-Path $dir))
    {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }

    if ($pwd)
    {
        $secure = ConvertTo-SecureString $pwd -AsPlainText -Force
        $pfxBytes = $cert.Export(
                [System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx,
                $secure
        )
    }
    else
    {
        $pfxBytes = $cert.Export(
                [System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx,
                [string]::Empty
        )
    }

    [System.IO.File]::WriteAllBytes($exportPath, $pfxBytes)
    Write-Ok "Exported PFX -> $exportPath"
    Write-Warn "The exported PFX contains the private key. Keep it out of source control."
}

if (-not (Test-Path $AssemblyPath))
{
    Write-Error "Assembly not found: $AssemblyPath"
    exit 1
}

if (-not $PfxPassword -and $env:RAD_SIGNING_PFX_PASSWORD)
{
    $PfxPassword = $env:RAD_SIGNING_PFX_PASSWORD
}

$PfxPath = Resolve-OptionalPath $PfxPath
$PfxExportPath = Resolve-OptionalPath $PfxExportPath

$cert = $null

if ($PfxPath -and (Test-Path $PfxPath))
{
    $cert = Load-CertFromPfx -path $PfxPath -pwd $PfxPassword
    Write-Ok "Certificate loaded from PFX."
}
else
{
    if ($PfxPath)
    {
        Write-Warn "PFX not found at '$PfxPath' - falling back to certificate store."
    }

    $cert = Find-CertInStore -store $CertStore -subject $CertSubject

    if ($cert)
    {
        Write-Ok "Found certificate in store."
        Write-Step "  Thumbprint : $( $cert.Thumbprint )"
        Write-Step "  Valid until: $($cert.NotAfter.ToString('yyyy-MM-dd') )"
    }
    else
    {
        $cert = New-CodeSigningCert -subject $CertSubject -store $CertStore

        if ($PfxExportPath)
        {
            Export-CertToPfx -cert $cert -exportPath $PfxExportPath -pwd $PfxPassword
        }
    }
}

Write-Step "Signing: $AssemblyPath"

$sigParams = @{
    FilePath = $AssemblyPath
    Certificate = $cert
    HashAlgorithm = "SHA256"
}

if ($TimestampUrl)
{
    $sigParams["TimestampServer"] = $TimestampUrl
    Write-Step "Timestamping via: $TimestampUrl"
}

$sig = Set-AuthenticodeSignature @sigParams

$failStatuses = @(
    [System.Management.Automation.SignatureStatus]::NotSigned,
    [System.Management.Automation.SignatureStatus]::HashMismatch,
    [System.Management.Automation.SignatureStatus]::NotSupportedFileFormat,
    [System.Management.Automation.SignatureStatus]::Incompatible
)

if ($sig.Status -in $failStatuses)
{
    Write-Error "Signing FAILED - status: $( $sig.Status )  path: $AssemblyPath"
    exit 1
}

$statusNote = if ($sig.Status -eq "UnknownError")
{
    "(self-signed root - expected for local certs)"
}
else
{
    ""
}

Write-Ok "Signed successfully  [$( $sig.Status )] $statusNote"
Write-Ok "Output: $AssemblyPath"
