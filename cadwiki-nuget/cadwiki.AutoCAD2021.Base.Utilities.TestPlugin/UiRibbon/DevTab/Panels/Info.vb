
Imports System.IO
Imports System.Reflection
Imports Autodesk.Windows
Imports cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons
Imports cadwiki.DllReloader.AutoCAD.AcadAssemblyUtils

Namespace cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.UiRibbon.DevTab.Panels
    Public Class Info
        Public Shared Function CreateInfoPanel(ByVal blankButton As RibbonButton) As RibbonPanel
            Dim row1 = New RibbonRowPanel()
            Dim row2 = New RibbonRowPanel()
            Dim row3 = New RibbonRowPanel()
            Dim row4 = New RibbonRowPanel()
            Dim currentIExtensionAppAssembly = Assembly.GetExecutingAssembly()
            Dim dllName As String = App.AcadAppDomainDllReloader.GetReloadedAssemblyNameSafely(currentIExtensionAppAssembly)
            Dim versionNumberStr = ""
            Dim exeName = ""
            If App.AcadAppDomainDllReloader.GetReloadCount() >= 1 Then
                exeName = Path.GetFileName(App.AcadAppDomainDllReloader.GetDllPath())
                versionNumberStr = GetAssemblyVersionFromFullName(dllName)
            Else
                Dim filePath As String = Assembly.GetExecutingAssembly().Location
                exeName = Path.GetFileName(filePath)
                versionNumberStr = GetAssemblyVersionFromFullName(currentIExtensionAppAssembly.FullName)
            End If
            Dim versionNumber = CreateVersionNumberButton(versionNumberStr)
            Dim assemblyName = CreateAssemblyNameButton(exeName)
            Dim reloadCount = CreateReloadCountButton(exeName)
            Dim ribbonPanelSource = New RibbonPanelSource()
            ribbonPanelSource.Title = "Info"
            ribbonPanelSource.Items.Add(row1)
            ribbonPanelSource.Items.Add(New RibbonRowBreak())
            ribbonPanelSource.Items.Add(row2)
            ribbonPanelSource.Items.Add(New RibbonRowBreak())
            ribbonPanelSource.Items.Add(row3)
            ribbonPanelSource.Items.Add(New RibbonRowBreak())
            ribbonPanelSource.Items.Add(row4)
            Dim ribbonPanel = New RibbonPanel()
            ribbonPanel.Source = ribbonPanelSource
            row1.Items.Add(versionNumber)
            row2.Items.Add(assemblyName)
            row3.Items.Add(reloadCount)
            row4.Items.Add(blankButton)
            row4.Items.Add(blankButton)
            Return ribbonPanel
        End Function

        Private Shared Function CreateVersionNumberButton(ByVal versionNumberStr As String) As RibbonButton
            Dim versionNumber = New RibbonButton()
            versionNumber.Name = "Version"
            versionNumber.ShowText = True
            versionNumber.Text = " v" & versionNumberStr & " "
            versionNumber.Size = RibbonItemSize.Standard
            versionNumber.IsEnabled = False
            Return versionNumber
        End Function

        'start here 4 - Reload button
        'this button handles the logic of Reloading a dll into AutoCAD's current application domain
        'once a new dll is reloaded, the AutoCADAppDomainDllReloader will
        'route future Ui clicks to the newly reloaded dlls methods
        Private Shared Function CreateReloadCountButton(ByVal exeName As String) As RibbonButton
            Dim button = New RibbonButton()
            button.Name = "ReloadCount"
            button.ShowText = True
            button.Text = " Reload Count: " & App.AcadAppDomainDllReloader.GetReloadCount().ToString()
            button.Size = RibbonItemSize.Standard
            button.CommandHandler = New DllReloadClickCommandHandler()
            button.ToolTip = "Reload the " & exeName & " dll into AutoCAD"
            Dim parameters = New Object() {}

            Dim uiRouter = New UiRouter("assemblyName: not used by DllReloadClickCommandHandler", "fullClassName: not used by DllReloadClickCommandHandler", "methodName: not used by DllReloadClickCommandHandler", parameters, App.AcadAppDomainDllReloader, Assembly.GetExecutingAssembly())


            button.CommandParameter = uiRouter
            Return button
        End Function

        Private Shared Function CreateAssemblyNameButton(ByVal exeName As String) As RibbonButton
            Dim assemblyName = New RibbonButton()
            assemblyName.Name = "ReleaseStatus"
            assemblyName.ShowText = True
            assemblyName.Text = " Dll: " & exeName
            assemblyName.Size = RibbonItemSize.Standard
            assemblyName.IsEnabled = False
            Return assemblyName
        End Function
    End Class
End Namespace
