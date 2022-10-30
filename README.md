# cadwiki-nuget README  
Tools for increasing productivity during Autodesk addin developement  



## Table of Contents  
[docs](https://github.com/cadwiki/cadwiki-nuget/tree/main/cadwiki-nuget/docs)  
[CadDevTools Readme](https://github.com/cadwiki/cadwiki-nuget/blob/main/README.nuget.cadwiki.CadDevTools.md)  
[DllReloader Readme](https://github.com/cadwiki/cadwiki-nuget/blob/main/README.nuget.cadwiki.DllReloader.md)  
[NUnitTestRunner Readme](https://github.com/cadwiki/cadwiki-nuget/blob/main/README.nuget.cadwiki.NUnitTestRunner.md)  

![NotFound](./cadwiki-nuget/icons/500x500-cadwiki-v1.png)  

## Solution overview
This solution splits the logic into 12 projects that each handle a single responsiblity:  

## Project details  

### CadDevToolsDriver  
Project used for developer testing of the CadDevTools project.  

### cadwiki.AcRemoveCmdGroup  
C++ project that calls part of the AutoCAD C++ api to unload a command.  

### cadwiki.AdminUtils  
Admin Utilities for managing Assembly version strings.  

### cadwiki.AutoCAD2021.Interop.Utilities  
Utilities for launching and controlling AutoCAD 2021.   

### cadwiki.AutoCAD2022.Interop.Utilities  
Utilities for launching and controlling AutoCAD 2022.   

### cadwiki.CadDevTools  
Single project that references all the other projects.  
This is the main project that a CAD developer would need to leverage to get value from the other projects.  

### cadwiki.DllReloader  
Reloads dll's into Autodesk products Application Domain.

### cadwiki.FileStore  
Resource files that are used by the other projects

### cadwiki.NetUtils  
Static utility methods.

### cadwiki.NUnitTestRunner  
NUnit test runner that accepts a type array and executes tests and collects test evidence.
<iframe src="https://github.com/cadwiki/cadwiki-nuget/blob/main/cadwiki-nuget/docs/AutomatedTestEvidence__2022__09__27____21_19_19.pdf"></iframe>   
![NotFound](./cadwiki-nuget/docs/AutomatedTestEvidence__2022__09__27____21_19_19.json)    
![NotFound](./cadwiki-nuget/docs/cadwiki.CadDevTools.pdf)  
![NotFound](./cadwiki-nuget/docs/AutomatedTestEvidence__2022__09__27____21_19_19.json)  
### cadwiki.WpfUi  
Collection of Windows Presentation Foundation User Interface's.

### UnitTests  
MSUnit test project for validating the logic in each project works as expected.  