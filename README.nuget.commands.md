# cadwiki Nuget commands  
This readme contains all the commands used for creating and pushing nuget packages  

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
### Create new package using relative .nuspec
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
Get-Project -All | Install-Package E:\GitHub\cadwiki\cadwiki-nuget\cadwiki-nuget\cadwiki.CadDevTools.1.0.12.726.nupkg
Get-Project -All | Install-Package E:\GitHub\cadwiki\cadwiki-nuget\cadwiki-nuget\cadwiki.NUnitTestRunner.1.0.12.726.nupkg
Get-Project -All | Install-Package E:\GitHub\cadwiki\cadwiki-nuget\cadwiki-nuget\cadwiki.DllReloader.1.0.12.726.nupkg
```



### Push nuget package 
```  
nuget push ./cadwiki.NUnitTestRunner.1.0.12.726.nupkg apikey -src https://www.nuget.org/  
nuget push ./cadwiki.DllReloader.1.0.12.726.nupkg apikey -src https://www.nuget.org/  
nuget push ./cadwiki.CadDevTools.1.0.12.726.nupkg apikey -src https://www.nuget.org/  
```
