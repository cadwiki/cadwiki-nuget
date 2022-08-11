## Nuget commands
### Create new package using relative .nuspec
```
nuget pack cadwiki.DllReloader.dll.nuspec -properties Configuration=Release -properties Platform=x64
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