## 1 update version
Discard all changes
undo skips to assembly info
RunAdmin Ops (ensure 4th digit is not zero, like this #.#.#.1)
Update buildThisFirst.csproj version number to match 

## 2 create package
```
nuget pack ./cadwiki.NUnitTestRunner/cadwiki.NUnitTestRunner.nuspec -properties Configuration=Release -properties Platform="Any CPU"
nuget pack ./cadwiki.DllReloader/cadwiki.DllReloader.nuspec -properties Configuration=Release -properties Platform="Any CPU"
nuget pack ./cadwiki.CadDevTools/cadwiki.CadDevTools.nuspec -properties Configuration=Release -properties Platform="Any CPU"
```

## 3 publish
!!!DON'T PUSH A PACKAGE THAT HAS ONLY 3 DIGIT VERSION NUMBER, THE AcRemoveCmdGroup TARGET WILL BREAK!!!

```  
nuget push ./cadwiki.NUnitTestRunner.4.1.0.1.nupkg apikey -src https://www.nuget.org/  
nuget push ./cadwiki.DllReloader.4.1.0.1.nupkg apikey -src https://www.nuget.org/  
nuget push ./cadwiki.CadDevTools.4.1.0.1.nupkg apikey -src https://www.nuget.org/  
```
