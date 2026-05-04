# cadwiki Nuget commands  
This readme contains all the commands used for creating and pushing nuget packages  

## Standard workflow for building and testing
1.) Clean project
2.) Build on Debug AnyCPU
3.) Generate version string (use timestamp format or pinned version)
4.) Run nuget pack commands with `-properties version=X.Y.Z`:
```
nuget pack ./cadwiki.NUnitTestRunner/cadwiki.NUnitTestRunner.nuspec -properties "Configuration=Debug;Platform=Any CPU;version=25.0.20260503.180345"
nuget pack ./cadwiki.DllReloader/cadwiki.DllReloader.nuspec -properties "Configuration=Debug;Platform=Any CPU;version=25.0.20260503.180345"
nuget pack ./cadwiki.CadDevTools/cadwiki.CadDevTools.nuspec -properties "Configuration=Debug;Platform=Any CPU;version=25.0.20260503.180345"
```
5.) Reference local nuget feed for testing
6.) Add any missing Autodesk references as needed

## Local feed clear
nuget locals all -list
nuget locals all -clear
C:\Users\{username}\.nuget\packages

## Nuget commands for creating new .nuspec file  
Cd into directory with .csproj or .vbproj  
```
cd cadwiki.NUnitTestRunner
cd cadwiki.DllReloader
cd cadwiki.CadDevTools
nuget spec
nuget spec -Force ./bin/Release/x64/cadwiki.NUnitTestRunner.dll
```

## Nuget commands for building the .nupkg locally

### Understanding `$version$` Token Substitution

The `.nuspec` files use `<version>$version$</version>` as a placeholder token. NuGet resolves this at pack time using the `-properties version=X.Y.Z` flag. This approach:

- Keeps the `.nuspec` as a reusable template (no manual edits before packing)
- Enables CI/CD automation (version comes from build pipeline)
- Prevents source control drift (version not hardcoded in file)

**Important:** If you forget the `-properties version=...` flag, the pack will fail with "The replacement token 'version' has no value."

### Create new package using relative .nuspec (with timestamp versioning)

The versioning schema uses `maj.min.yyyyMMdd.HHmmss` format. Example: `25.0.20260503.180345`

```bash
# Release builds with timestamped version
nuget pack ./cadwiki.NUnitTestRunner/cadwiki.NUnitTestRunner.nuspec -properties "Configuration=Release;Platform=Any CPU;version=25.0.20260503.180345"
nuget pack ./cadwiki.DllReloader/cadwiki.DllReloader.nuspec -properties "Configuration=Release;Platform=Any CPU;version=25.0.20260503.180345"
nuget pack ./cadwiki.CadDevTools/cadwiki.CadDevTools.nuspec -properties "Configuration=Release;Platform=Any CPU;version=25.0.20260503.180345"

# Debug builds with timestamped version
nuget pack ./cadwiki.NUnitTestRunner/cadwiki.NUnitTestRunner.nuspec -properties "Configuration=Debug;Platform=Any CPU;version=25.0.20260503.180345"
nuget pack ./cadwiki.DllReloader/cadwiki.DllReloader.nuspec -properties "Configuration=Debug;Platform=Any CPU;version=25.0.20260503.180345"
nuget pack ./cadwiki.CadDevTools/cadwiki.CadDevTools.nuspec -properties "Configuration=Debug;Platform=Any CPU;version=25.0.20260503.180345"
```

### Generate timestamp dynamically (PowerShell)
```powershell
$timestamp = Get-Date -Format "yy.0.yyyyMMdd.HHmmss"
nuget pack ./cadwiki.DllReloader/cadwiki.DllReloader.nuspec -properties "Configuration=Release;Platform=Any CPU;version=$timestamp"
```

### Generate timestamp dynamically (Bash)
```bash
VERSION=$(date +"%y.0.%Y%m%d.%H%M%S")
nuget pack ./cadwiki.DllReloader/cadwiki.DllReloader.nuspec -properties "Configuration=Release;Platform=Any CPU;version=$VERSION"
```

### Legacy: Create packages using relative .nuspec (old hardcoded version)
```
nuget pack ./cadwiki.NUnitTestRunner/cadwiki.NUnitTestRunner.nuspec -properties Configuration=Release -properties Platform="Any CPU"
nuget pack ./cadwiki.DllReloader/cadwiki.DllReloader.nuspec -properties Configuration=Release -properties Platform="Any CPU"
nuget pack ./cadwiki.CadDevTools/cadwiki.CadDevTools.nuspec -properties Configuration=Release -properties Platform="Any CPU"
nuget pack ./cadwiki.NUnitTestRunner/cadwiki.NUnitTestRunner.nuspec -properties Configuration=Debug -properties Platform="Any CPU"
nuget pack ./cadwiki.DllReloader/cadwiki.DllReloader.nuspec -properties Configuration=Debug -properties Platform="Any CPU"
nuget pack ./cadwiki.CadDevTools/cadwiki.CadDevTools.nuspec -properties Configuration=Debug -properties Platform="Any CPU"
```
### Create new package using relative .vbproj
```
nuget pack cadwiki.NUnitTestRunner.vbproj -IncludeReferencedProjects -properties Configuration=Release -properties Platform="Any CPU"
nuget pack cadwiki.DllReloader.vbproj -IncludeReferencedProjects -properties Configuration=Release -properties Platform="Any CPU"
```

### Uninstall local package from projects
```
Get-Package | Uninstall-Package -RemoveDependencies -Force
Get-Project -All | UnInstall-Package AutoCAD2021.Interop.Base
Only need to uninstall dev tools
Get-Project -All | UnInstall-Package cadwiki.CadDevTools
Get-Project -All | UnInstall-Package cadwiki.NUnitTestRunner
Get-Project -All | UnInstall-Package cadwiki.DllReloader
```

### Install local package to another project
Copy output .nupkg file path from command above for use later
Install nuget package in another solution / project with these commands 

```
Install-Package $Path\ToNuget\File\.nupkg
Get-Project -All | Install-Package $Path\ToNuget\File\.nupkg
Install-Package E:\GitHub\cadwiki\cadwiki-nuget\cadwiki-nuget\AutoCAD2021.Interop.Base\AutoCAD2021.Interop.Base.1.0.0.nupkg
Only need to install dev tools
dotnet nuget add source "E:\GitHub\cadwiki\cadwiki-nuget\cadwiki-nuget" --name Local
dotnet add package cadwiki.CadDevTools --version 25.0.0.4

Get-Project -All | UnInstall-Package cadwiki.CadDevTools
Get-Project -All | Install-Package E:\GitHub\cadwiki\cadwiki-nuget\cadwiki-nuget\cadwiki.CadDevTools.25.0.0.4.nupkg


Get-Project -All | Install-Package E:\GitHub\cadwiki\cadwiki-nuget\cadwiki-nuget\cadwiki.NUnitTestRunner.25.0.0.4.nupkg
Get-Project -All | Install-Package E:\GitHub\cadwiki\cadwiki-nuget\cadwiki-nuget\cadwiki.DllReloader.25.0.0.4.nupkg
```



### Push nuget package

Replace `{version}` with your actual version (e.g., `25.0.20260503.180345`):

```  
nuget push ./cadwiki.NUnitTestRunner.{version}.nupkg apikey -src https://www.nuget.org/  
nuget push ./cadwiki.DllReloader.{version}.nupkg apikey -src https://www.nuget.org/  
nuget push ./cadwiki.CadDevTools.{version}.nupkg apikey -src https://www.nuget.org/  
```

Example with actual timestamped version:
```
nuget push ./cadwiki.NUnitTestRunner.25.0.20260503.180345.nupkg apikey -src https://www.nuget.org/  
nuget push ./cadwiki.DllReloader.25.0.20260503.180345.nupkg apikey -src https://www.nuget.org/  
nuget push ./cadwiki.CadDevTools.25.0.20260503.180345.nupkg apikey -src https://www.nuget.org/  
```
