
Imports Autodesk.Windows
Imports cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.UiRibbon.DevTab.Panels
Imports cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.UiRibbon.DevTab.Panels

Namespace cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.UiRibbon.Tabs
    Public Class DevTab
        Private Shared ReadOnly tabName As String = "DevTab"

        Public Shared Function Create() As RibbonTab
            Dim ribbonTab = New RibbonTab()
            ribbonTab.Title = tabName
            ribbonTab.Id = tabName
            ribbonTab.Name = tabName
            Return ribbonTab
        End Function

        Public Shared Sub AddAllPanels(ByVal ribbonTab As RibbonTab)
            Dim blankButton = New RibbonButton()
            blankButton.Name = "BlankButton1"
            blankButton.Size = RibbonItemSize.Standard
            blankButton.IsEnabled = False
            Dim infoRibbonPanel = Info.CreateInfoPanel(blankButton)
            ribbonTab.Panels.Add(infoRibbonPanel)
            Dim testPanel = Test.CreateTestsPanel(blankButton)
            ribbonTab.Panels.Add(testPanel)
        End Sub
    End Class


End Namespace
