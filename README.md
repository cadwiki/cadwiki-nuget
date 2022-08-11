## Nuget commands
### Create new package using relative .nuspec and .vbproj
```
nuget pack cadwiki.DllReloader.vbproj -IncludeReferencedProjects -properties Configuration=Release -properties Platform=x64
```

### Install local package to another project
Copy output file path from command above for use later
Install nuget package in another solution / project with this command 

```
Install-Package $Path\ToNuget\File\.nupkg
```