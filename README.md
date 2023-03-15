# cadwiki-nuget README  
Tools for increasing productivity during Autodesk addin developement.  

## Blog  
[https://www.cadwiki.net](https://www.cadwiki.net)  

## Docs links  
[Automated Test Html Report sample](https://raw.githack.com/cadwiki/cadwiki-nuget/main/cadwiki-nuget/docs/Test_CreatePdf_ShouldCreatePdf-AutomatedTestEvidence__2023__03__14____22_02_49.html)

## GitHub Links  
[https://github.com/cadwiki/cadwiki-nuget-examples](https://github.com/cadwiki/cadwiki-nuget-examples)  
[https://github.com/cadwiki/cadwiki-nuget/tree/main/cadwiki-nuget/docs](https://github.com/cadwiki/cadwiki-nuget/tree/main/cadwiki-nuget/docs)  

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
CadDevTools is a set of user interfaces and Interop utilities for launching AutoCAD directly from visual studio.   
This is the main project that a CAD developer would need to leverage to get value from the other projects.  
This toolset lets a developer drive AutoCAD from an IDE and netload dll's automatically. 

### cadwiki.DllReloader  
The DllReloader package let's developers reload the same dll multiple times into AutoCAD, as long as 1 criteria is met:
1.  The dll to be reloaded has a newer AssemblyVersion than any dll with the same name currently in AutoCAD's appdomain  


### cadwiki.FileStore  
Resource files that are used by the other projects

### cadwiki.NetUtils  
Static utility methods.

### cadwiki.NUnitTestRunner  
NUnit test runner has ExecuteTestsAsync method that accepts a type array and executes tests and collects test evidence.  
The project has Engine, TestEvidenceCreator and screenshot functionality to capture test evidence automatically.  
<p>
	<a href="https://raw.githubusercontent.com/cadwiki/cadwiki-nuget/main/cadwiki-nuget/docs/AutomatedTestEvidence__2022__09__27____21_19_19.json">
	AutomatedTestEvidence.json
	</a>
</p>
<p>
	<a href="https://raw.githubusercontent.com/cadwiki/cadwiki-nuget/main/cadwiki-nuget/docs/AutomatedTestEvidence__2022__09__27____21_19_19.pdf">
	AutomatedTestEvidence.pdf
	</a>
</p>

### cadwiki.WpfUi  
Collection of Windows Presentation Foundation User Interface's.

### UnitTests  
MSUnit test project for validating the logic in each project works as expected.  
