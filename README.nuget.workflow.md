## Versioning Workflow (Central Schema + Auto-Increment)

The versioning is now centralized in `Directory.Build.props` and uses auto-increment with timestamps.

**Version format:** `maj.min.yyyyMMdd.HHmmss` (e.g., `25.0.20260503.180345`)

### Mode 1: Auto (Default) — Timestamp-Based Versioning

The build/revision components are computed from UTC time at build time.

```xml
<!-- In Directory.Build.props -->
<CadwikiVersionMajor>25</CadwikiVersionMajor>
<CadwikiVersionMinor>0</CadwikiVersionMinor>
<CadwikiVersionBuild>$([System.DateTime]::UtcNow.ToString('yyyyMMdd'))</CadwikiVersionBuild>
<CadwikiVersionRevision>$([System.DateTime]::UtcNow.ToString('HHmmss'))</CadwikiVersionRevision>
<CadwikiVersion>$(CadwikiVersionMajor).$(CadwikiVersionMinor).$(CadwikiVersionBuild).$(CadwikiVersionRevision)</CadwikiVersion>
```

### Mode 2: Manual — Pinned Versions

For release tagging, override via MSBuild parameters:

```bash
dotnet build /p:CadwikiVersionMode=Manual /p:CadwikiVersionBuild=0 /p:CadwikiVersionRevision=4
```

This produces version `25.0.0.4` (for tagging a stable release).

---

## 1. Update Directory.Build.props (if changing major/minor version)

Edit `Directory.Build.props` to update `CadwikiVersionMajor` and `CadwikiVersionMinor` as needed:

```xml
<CadwikiVersionMajor>25</CadwikiVersionMajor>
<CadwikiVersionMinor>0</CadwikiVersionMinor>
```

The build and revision numbers are **automatic** (based on current UTC timestamp).

---

## 2. Create Packages

Generate the version string and pass it via `-properties version=X.Y.Z`:

### Using Auto-Generated Timestamp

```bash
# PowerShell
$timestamp = Get-Date -Format "yy.0.yyyyMMdd.HHmmss"
nuget pack ./cadwiki.NUnitTestRunner/cadwiki.NUnitTestRunner.nuspec -properties "Configuration=Release;Platform=Any CPU;version=$timestamp"
nuget pack ./cadwiki.DllReloader/cadwiki.DllReloader.nuspec -properties "Configuration=Release;Platform=Any CPU;version=$timestamp"
nuget pack ./cadwiki.CadDevTools/cadwiki.CadDevTools.nuspec -properties "Configuration=Release;Platform=Any CPU;version=$timestamp"
```

Or Bash:

```bash
VERSION=$(date +"%y.0.%Y%m%d.%H%M%S")
nuget pack ./cadwiki.NUnitTestRunner/cadwiki.NUnitTestRunner.nuspec -properties "Configuration=Release;Platform=Any CPU;version=$VERSION"
nuget pack ./cadwiki.DllReloader/cadwiki.DllReloader.nuspec -properties "Configuration=Release;Platform=Any CPU;version=$VERSION"
nuget pack ./cadwiki.CadDevTools/cadwiki.CadDevTools.nuspec -properties "Configuration=Release;Platform=Any CPU;version=$VERSION"
```

### Using Manual Pinned Version

For stable releases, use a pinned version:

```bash
nuget pack ./cadwiki.NUnitTestRunner/cadwiki.NUnitTestRunner.nuspec -properties "Configuration=Release;Platform=Any CPU;version=25.0.0.4"
nuget pack ./cadwiki.DllReloader/cadwiki.DllReloader.nuspec -properties "Configuration=Release;Platform=Any CPU;version=25.0.0.4"
nuget pack ./cadwiki.CadDevTools/cadwiki.CadDevTools.nuspec -properties "Configuration=Release;Platform=Any CPU;version=25.0.0.4"
```

---

## 3. Publish

The output `.nupkg` filename will match the version passed via `-properties`:

```bash
nuget push ./cadwiki.NUnitTestRunner.25.0.20260503.180345.nupkg apikey -src https://www.nuget.org/  
nuget push ./cadwiki.DllReloader.25.0.20260503.180345.nupkg apikey -src https://www.nuget.org/  
nuget push ./cadwiki.CadDevTools.25.0.20260503.180345.nupkg apikey -src https://www.nuget.org/  
```

Or for pinned releases:

```bash
nuget push ./cadwiki.NUnitTestRunner.25.0.0.4.nupkg apikey -src https://www.nuget.org/  
nuget push ./cadwiki.DllReloader.25.0.0.4.nupkg apikey -src https://www.nuget.org/  
nuget push ./cadwiki.CadDevTools.25.0.0.4.nupkg apikey -src https://www.nuget.org/  
```

**Important:** Always use 4-digit versions (e.g., `25.0.0.4`). The AcRemoveCmdGroup .targets file depends on this format.
