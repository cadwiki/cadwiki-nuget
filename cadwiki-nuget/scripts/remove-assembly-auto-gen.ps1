$patterns = @(
    '^\s*<GenerateAssemblyVersionAttribute>false</GenerateAssemblyVersionAttribute>\s*$',
    '^\s*<GenerateAssemblyFileVersionAttribute>false</GenerateAssemblyFileVersionAttribute>\s*$',
    '^\s*<GenerateAssemblyInformationalVersionAttribute>false</GenerateAssemblyInformationalVersionAttribute>\s*$',
    '^\s*<GenerateAssemblyCompanyAttribute>false</GenerateAssemblyCompanyAttribute>\s*$',
    '^\s*<GenerateAssemblyProductAttribute>false</GenerateAssemblyProductAttribute>\s*$',
    '^\s*<GenerateAssemblyTitleAttribute>false</GenerateAssemblyTitleAttribute>\s*$',
    '^\s*<GenerateAssemblyConfigurationAttribute>false</GenerateAssemblyConfigurationAttribute>\s*$'
)

Get-ChildItem -Path . -Recurse -Filter *.csproj | ForEach-Object {

    $file = $_.FullName
    $lines = Get-Content $file
    $originalCount = $lines.Count

    $filtered = $lines | Where-Object {
        $line = $_
        -not ($patterns | ForEach-Object { $line -match $_ } | Where-Object { $_ } )
    }

    if ($filtered.Count -ne $originalCount) {
        # backup
        Copy-Item $file "$file.bak" -Force

        # write updated file
        Set-Content -Path $file -Value $filtered -Encoding UTF8

        Write-Host "Updated: $file (backup created)"
    }
}