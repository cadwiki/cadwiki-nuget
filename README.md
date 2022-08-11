## Nuget commands
### Create new package using relative .nuspec and .vbproj
```
nuget pack cadwiki.DllReloader.vbproj -IncludeReferencedProjects -properties Configuration=Release -properties Platform=x64
```

### Install local package to another project
Copy output file path from command above for use later
Install nuget package in another solution / project with these commands 

```
Install-Package $Path\ToNuget\File\.nupkg
Get-Project -All | Install-Package $Path\ToNuget\File\.nupkg
```

### Uninstall local package from projects
```
Get-Project -All | UnInstall-Package cadwiki.DllReloader
```