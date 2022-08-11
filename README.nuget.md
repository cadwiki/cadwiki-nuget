# cadwiki.DllReloader nuget package
This Readme file contains information on the cadwiki.DllReloader package

## AutoCAD
The cadwiki.DllReloader.AutoCAD namespace contains everything needed to reload dll's into AutoCAD's current AppDomain

## Nuget commands for building the .nupkg locally
### Create new package using relative .nuspec
```
nuget pack cadwiki.DllReloader.nuspec -properties Configuration=Release -properties Platform=x64
```
### Create new package using relative .vbproj
```
nuget pack cadwiki.DllReloader.vbproj -IncludeReferencedProjects -properties Configuration=Release -properties Platform=x64
```

### Install local package to another project
Copy output .nupkg file path from command above for use later
Install nuget package in another solution / project with these commands 

```
Install-Package $Path\ToNuget\File\.nupkg
Get-Project -All | Install-Package $Path\ToNuget\File\.nupkg
Get-Project -All | Install-Package E:\GitHub\cadwiki\cadwiki-nuget\cadwiki-nuget\cadwiki.DllReloader\cadwiki.DllReloader.1.0.0.nupkg
```

### Uninstall local package from projects
```
Get-Project -All | UnInstall-Package cadwiki.DllReloader
```