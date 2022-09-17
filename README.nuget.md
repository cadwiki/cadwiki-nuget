# cadwiki.DllReloader nuget package
This Readme file contains information on the cadwiki.DllReloader package

## AutoCAD
The cadwiki.DllReloader.AutoCAD namespace contains everything needed to reload dll's into AutoCAD's current AppDomain


## Nuget commands for creating new .nuspec file  
Cd into directory with .csproj or .vbproj  
```
cd cadwiki.NUnitTestRunner
cd cadwiki.DllReloader
nuget spec
nuget spec -Force ./bin/Release/x64/cadwiki.NUnitTestRunner.dll
```

## Nuget commands for building the .nupkg locally
### Create new package using relative .nuspec
```
nuget pack cadwiki.NUnitTestRunner.nuspec -properties Configuration=Release -properties Platform=x64
nuget pack cadwiki.DllReloader.nuspec -properties Configuration=Release -properties Platform=x64
nuget pack cadwiki.DllReloader.nuspec -properties Configuration=Debug -properties Platform=x64
```
### Create new package using relative .vbproj
```
nuget pack cadwiki.NUnitTestRunner.vbproj -IncludeReferencedProjects -properties Configuration=Release -properties Platform=x64
nuget pack cadwiki.DllReloader.vbproj -IncludeReferencedProjects -properties Configuration=Release -properties Platform=x64
```

### Uninstall local package from projects
```
Get-Project -All | UnInstall-Package cadwiki.NUnitTestRunner
Get-Project -All | UnInstall-Package cadwiki.DllReloader
```

### Install local package to another project
Copy output .nupkg file path from command above for use later
Install nuget package in another solution / project with these commands 

```
Install-Package $Path\ToNuget\File\.nupkg
Get-Project -All | Install-Package $Path\ToNuget\File\.nupkg
Get-Project -All | Install-Package E:\GitHub\cadwiki\cadwiki-nuget\cadwiki-nuget\cadwiki.NUnitTestRunner\cadwiki.NUnitTestRunner.1.0.0.nupkg
Get-Project -All | Install-Package E:\GitHub\cadwiki\cadwiki-nuget\cadwiki-nuget\cadwiki.DllReloader\cadwiki.DllReloader.1.0.0.nupkg
Get-Project -All | Install-Package E:\GitHub\cadwiki\cadwiki-nuget\cadwiki-nuget\cadwiki.DllReloader\cadwiki.DllReloader.1.0.2.nupkg
Get-Project -All | Install-Package E:\GitHub\cadwiki\cadwiki-nuget\cadwiki-nuget\cadwiki.DllReloader\cadwiki.DllReloader.1.0.3.nupkg
```



### Push nuget package 
```  
nuget push ./cadwiki.DllReloader.1.0.2.nupkg apikey -src https://www.nuget.org/  
```