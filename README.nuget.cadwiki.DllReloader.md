# cadwiki.DllReloader nuget package
This Readme file contains information on the cadwiki.DllReloader package  

## Overview
The DllReloader package let's developers reload the same dll multiple times into AutoCAD, as long as 1 criteria is met:
1.  The dll to be reloaded has a newer AssemblyVersion than any dll with the same name currently in AutoCAD's appdomain  

The project has a few System.Windows.Input.ICommands that can be used with AutoCAD Ribbon buttons.

See the cadwiki-nuget-examples repo for how to use this package.  
[https://github.com/cadwiki/cadwiki-nuget-examples](https://github.com/cadwiki/cadwiki-nuget-examples)  