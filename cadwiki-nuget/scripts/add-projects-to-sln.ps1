param(
    [string]$SolutionPath = "C:\temp2\Example.sln",
    [string]$SearchRoot = "C:\temp\cadwiki-nuget"
)

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet CLI not found in PATH"
}

$SolutionPath = (Resolve-Path $SolutionPath).Path
$SearchRoot   = (Resolve-Path $SearchRoot).Path
$SolutionDir  = Split-Path $SolutionPath -Parent

Write-Host "Solution : $SolutionPath"
Write-Host "Searching: $SearchRoot"

# Build set of already-added projects
$existing = @{}
& dotnet sln $SolutionPath list |
    ForEach-Object { $_.Trim() } |
    Where-Object   { $_ -match "\.csproj$" } |
    ForEach-Object {
        # Try as relative-to-solution first; fall back to as-is (absolute)
        $candidate = if ([System.IO.Path]::IsPathRooted($_)) { $_ }
                     else { Join-Path $SolutionDir $_ }
        if (Test-Path $candidate) {
            $existing[(Resolve-Path $candidate).Path.ToLowerInvariant()] = $true
        }
    }

$projects = Get-ChildItem -Path $SearchRoot -Recurse -Filter *.csproj |
    Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\" }

foreach ($proj in $projects) {
    $fullPath = (Resolve-Path $proj.FullName).Path
    $key      = $fullPath.ToLowerInvariant()

    if ($existing.ContainsKey($key)) {
        Write-Host "Skipping (already in solution): $fullPath"
        continue
    }

    Write-Host "Adding: $fullPath"
    & dotnet sln $SolutionPath add --in-root "$fullPath"   # <-- key flag
    $existing[$key] = $true
}

Write-Host "Done."