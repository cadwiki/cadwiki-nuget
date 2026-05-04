<#
.SYNOPSIS
    One-shot script to pack and publish cadwiki NuGet packages.

.DESCRIPTION
    Reads CadwikiVersionMajor and CadwikiVersionMinor from Directory.Build.props,
    computes a timestamp-based version (maj.min.yyyyMMdd.HHmmss), then runs
    nuget pack and nuget push for all three packages:
      - cadwiki.NUnitTestRunner
      - cadwiki.DllReloader
      - cadwiki.CadDevTools

    Designed to be run from the Visual Studio Package Manager Console or any
    PowerShell terminal. No manual version edits needed.

.PARAMETER Configuration
    Build configuration. Default: Release.

.PARAMETER Platform
    Build platform. Default: "Any CPU".

.PARAMETER PackOnly
    If set, packs the .nupkg files but skips the push to nuget.org.

.PARAMETER PinnedVersion
    Use a specific version string instead of the auto-generated timestamp.
    Example: -PinnedVersion "25.0.0.4"

.PARAMETER NuGetApiKey
    API key for nuget.org. If omitted the script checks the NUGET_API_KEY
    environment variable. Required unless -PackOnly is specified.

.PARAMETER OutputDirectory
    Directory for .nupkg output. Default: .\nupkgs

.PARAMETER Source
    NuGet push source URL. Default: https://api.nuget.org/v3/index.json

.EXAMPLE
    # Auto-version, pack and push
    .\Publish-CadwikiPackages.ps1 -NuGetApiKey "my-key"

.EXAMPLE
    # Pack only (no push), Debug config
    .\Publish-CadwikiPackages.ps1 -PackOnly -Configuration Debug

.EXAMPLE
    # Use a pinned version
    .\Publish-CadwikiPackages.ps1 -PinnedVersion "25.0.0.4" -NuGetApiKey "my-key"
#>

[CmdletBinding()]
param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",

    [string]$Platform = "Any CPU",

    [switch]$PackOnly,

    [ValidatePattern('^\d+\.\d+\.\d+\.\d+$')]
    [string]$PinnedVersion,

    [string]$NuGetApiKey,

    [string]$OutputDirectory = ".\nupkgs",

    [string]$Source = "https://api.nuget.org/v3/index.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# --- Helpers ------------------------------------------------------------------

function Write-Step  { param([string]$Msg) Write-Host "`n $Msg" -ForegroundColor Cyan }
function Write-Ok    { param([string]$Msg) Write-Host "  OK $Msg" -ForegroundColor Green }
function Write-Err   { param([string]$Msg) Write-Host "  ERR $Msg" -ForegroundColor Red }
function Write-Info  { param([string]$Msg) Write-Host "  INFO $Msg" -ForegroundColor Yellow }

# --- Locate repo root (where this script lives) ------------------------------

$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
Push-Location $RepoRoot

try {

# --- 1. Read Directory.Build.props --------------------------------------------

Write-Step "Reading version info from Directory.Build.props"

$propsPath = Join-Path $RepoRoot "Directory.Build.props"
if (-not (Test-Path $propsPath)) {
    Write-Err "Directory.Build.props not found at: $propsPath"
    exit 1
}

[xml]$props = Get-Content $propsPath -Raw

function Get-PropValue {
    param($nodes)

    foreach ($n in $nodes) {
        $val = if ($n -is [System.Xml.XmlElement]) { $n.InnerText } else { $n }

        if ($val -and $val -notmatch '^\$\(') {
            return $val
        }
    }
    return $null
}

$major = Get-PropValue $props.Project.PropertyGroup.CadwikiVersionMajor
$minor = Get-PropValue $props.Project.PropertyGroup.CadwikiVersionMinor

if ([string]::IsNullOrWhiteSpace($major) -or [string]::IsNullOrWhiteSpace($minor)) {
    Write-Err "Could not read CadwikiVersionMajor / CadwikiVersionMinor from props file."
    exit 1
}

Write-Ok ("Major = {0}, Minor = {1}" -f $major, $minor)

# --- 2. Read hardcoded version for NuGet publish ----------------------------

Write-Step "Reading hardcoded version for NuGet publish"

$hardcodedBuild    = Get-PropValue $props.Project.PropertyGroup.CadwikiVersionBuild
$hardcodedRevision = Get-PropValue $props.Project.PropertyGroup.CadwikiVersionRevision

if (-not [string]::IsNullOrWhiteSpace($hardcodedBuild) -and -not [string]::IsNullOrWhiteSpace($hardcodedRevision)) {
    $publishVersion = "{0}.{1}.{2}.{3}" -f $major, $minor, $hardcodedBuild, $hardcodedRevision
    Write-Ok ("Hardcoded publish version from props: {0}" -f $publishVersion)
} else {
    Write-Err "Could not read hardcoded version from props. Ensure CadwikiVersionBuild and CadwikiVersionRevision are set."
    exit 1
}

# --- 3. Compute version for packing -------------------------------------------

Write-Step "Computing version for packing"

if ($PinnedVersion) {
    $version = $PinnedVersion
    Write-Info "Using pinned version: $version"
} else {
    $utcNow   = [System.DateTime]::UtcNow
    $build    = $utcNow.ToString("yyyyMMdd")
    $revision = $utcNow.ToString("HHmmss")
    $version  = "$major.$minor.$build.$revision"
    Write-Info "Auto-generated version: $version  (UTC: $($utcNow.ToString('u')))"
}

# --- 4. Validate API key (unless PackOnly) -----------------------------------

if (-not $PackOnly) {
    if ([string]::IsNullOrWhiteSpace($NuGetApiKey)) {
        $NuGetApiKey = $env:NUGET_API_KEY
    }
    if ([string]::IsNullOrWhiteSpace($NuGetApiKey)) {
        Write-Err "No NuGet API key provided. Pass -NuGetApiKey or set NUGET_API_KEY env var."
        Write-Err "Or use -PackOnly to skip push."
        exit 1
    }
    Write-Ok "API key found (length $($NuGetApiKey.Length))"
}

# --- 5. Ensure output directory ----------------------------------------------

$outDir = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $OutputDirectory))
if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}
Write-Info "Output directory: $outDir"

# --- 6. Verify nuget.exe is available ----------------------------------------

Write-Step "Checking for nuget.exe"

$nuget = Get-Command nuget -ErrorAction SilentlyContinue
if (-not $nuget) {
    $nuget = Get-Command nuget.exe -ErrorAction SilentlyContinue
}
if (-not $nuget) {
    Write-Err "nuget.exe not found on PATH. Install it or add it to PATH."
    Write-Err "Download: https://www.nuget.org/downloads"
    exit 1
}
Write-Ok "nuget.exe found: $($nuget.Source)"

# --- 7. Define packages ------------------------------------------------------

$packages = @(
    @{
        Name    = "cadwiki.NUnitTestRunner"
        NuSpec  = Join-Path $RepoRoot "cadwiki.NUnitTestRunner\cadwiki.NUnitTestRunner.nuspec"
    },
    @{
        Name    = "cadwiki.DllReloader"
        NuSpec  = Join-Path $RepoRoot "cadwiki.DllReloader\cadwiki.DllReloader.nuspec"
    },
    @{
        Name    = "cadwiki.CadDevTools"
        NuSpec  = Join-Path $RepoRoot "cadwiki.CadDevTools\cadwiki.CadDevTools.nuspec"
    }
)

# --- 8. Pack ------------------------------------------------------------------

Write-Step "Packing NuGet packages (Configuration=$Configuration, PackVersion=$version)"

$packedFiles = @()
$packFailed  = $false

foreach ($pkg in $packages) {
    Write-Host ""
    Write-Info "Packing $($pkg.Name)..."

    if (-not (Test-Path $pkg.NuSpec)) {
        Write-Err "Nuspec not found: $($pkg.NuSpec)"
        $packFailed = $true
        continue
    }

    $packArgs = @(
        "pack"
        $pkg.NuSpec
        "-Properties", "configuration=$Configuration;version=$version"
        "-OutputDirectory", $outDir
        "-NonInteractive"
    )

    try {
        & nuget @packArgs 2>&1 | ForEach-Object { Write-Host "    $_" }

        if ($LASTEXITCODE -ne 0) {
            Write-Err "nuget pack failed for $($pkg.Name) (exit code $LASTEXITCODE)"
            $packFailed = $true
            continue
        }

        # Find the generated .nupkg
        $nupkg = Get-ChildItem -Path $outDir -Filter "$($pkg.Name).$version.nupkg" -ErrorAction SilentlyContinue |
                 Sort-Object LastWriteTime -Descending |
                 Select-Object -First 1

        if ($nupkg) {
            Write-Ok "Created: $($nupkg.Name)"
            $packedFiles += $nupkg.FullName
        } else {
            Write-Err "Package file not found after pack for $($pkg.Name)"
            $packFailed = $true
        }
    }
    catch {
        Write-Err "Exception packing $($pkg.Name): $_"
        $packFailed = $true
    }
}

if ($packFailed) {
    Write-Host ""
    Write-Err "One or more packages failed to pack. Aborting."
    exit 1
}

Write-Host ""
Write-Ok "All $($packedFiles.Count) packages packed successfully."

# --- 9. Push (with hardcoded version only) ------------------------------------

if ($PackOnly) {
    Write-Step "PackOnly mode  skipping push"
    Write-Info "Packages are in: $outDir"
} else {
    Write-Step "Pushing packages to $Source"
    Write-Info "Using hardcoded publish version from props: $publishVersion"

    # Only allow pushing with the hardcoded version, not auto-generated timestamps
    if ($version -ne $publishVersion) {
        Write-Host ""
        Write-Info "Pack version ($version) differs from hardcoded publish version ($publishVersion)"
        Write-Info "Searching for $publishVersion packages to push..."

        # Look for .nupkg files with the hardcoded version
        $publishPackages = @()
        foreach ($pkg in $packages) {
            $publishNupkg = Get-ChildItem -Path $outDir -Filter "$($pkg.Name).$publishVersion.nupkg" -ErrorAction SilentlyContinue |
                           Sort-Object LastWriteTime -Descending |
                           Select-Object -First 1
            if ($publishNupkg) {
                $publishPackages += $publishNupkg.FullName
            } else {
                Write-Err "Expected publish package not found: $($pkg.Name).$publishVersion.nupkg"
                Write-Err "You can only push the hardcoded version ($publishVersion) to NuGet."
                exit 1
            }
        }
    } else {
        # Pack version matches publish version, use packed files
        $publishPackages = $packedFiles
        Write-Info "Using packed files (versions match)"
    }

    $pushFailed = $false

    foreach ($nupkg in $publishPackages) {
        $fileName = [System.IO.Path]::GetFileName($nupkg)
        Write-Info "Pushing $fileName..."

        $pushArgs = @(
            "push"
            $nupkg
            "-ApiKey", $NuGetApiKey
            "-Source", $Source
            "-NonInteractive"
            "-SkipDuplicate"
        )

        try {
            & nuget @pushArgs 2>&1 | ForEach-Object { Write-Host "    $_" }

            if ($LASTEXITCODE -ne 0) {
                Write-Err "nuget push failed for $fileName (exit code $LASTEXITCODE)"
                $pushFailed = $true
                continue
            }

            Write-Ok "Pushed: $fileName"
        }
        catch {
            Write-Err "Exception pushing $fileName`: $_"
            $pushFailed = $true
        }
    }

    if ($pushFailed) {
        Write-Host ""
        Write-Err "One or more packages failed to push."
        exit 1
    }

    Write-Host ""
    Write-Ok "All packages pushed to $Source with version $publishVersion"
}

# --- 10. Summary --------------------------------------------------------------

Write-Host ""
Write-Host ("=" * 60) -ForegroundColor Cyan
Write-Host "  SUMMARY" -ForegroundColor Cyan
Write-Host ("=" * 60) -ForegroundColor Cyan
Write-Host "  Pack Version:    $version"
Write-Host "  Publish Version: $publishVersion"
Write-Host "  Configuration:   $Configuration"
Write-Host "  Packages:"
foreach ($f in $packedFiles) {
    Write-Host "     $([System.IO.Path]::GetFileName($f))"
}
if ($PackOnly) {
    Write-Host "  Status:        PACKED (push skipped)" -ForegroundColor Yellow
    Write-Host "  Output:        $outDir"
} else {
    Write-Host "  Status:        PUBLISHED OK" -ForegroundColor Green
    Write-Host "  Source:        $Source"
    Write-Host "  Note:          Only version $publishVersion pushed to NuGet (hardcoded in props)" -ForegroundColor Yellow
}
Write-Host ("=" * 60) -ForegroundColor Cyan
Write-Host ""

} finally {
    Pop-Location
}
