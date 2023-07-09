
Imports System.Reflection
Imports Autodesk.Windows
Imports cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons

Namespace cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.UiRibbon.DevTab.Panels
    Public Class Test

        Public Shared Function CreateTestsPanel(ByVal blankButton As RibbonButton) As RibbonPanel
            Dim integrationTestsButton = CreateRegressionTestsButton()
            Dim ribbonPanelSource = New RibbonPanelSource()
            ribbonPanelSource.Title = "Tests"
            Dim row1 = New RibbonRowPanel()
            row1.IsTopJustified = True
            row1.Items.Add(integrationTestsButton)
            row1.Items.Add(New RibbonRowBreak())
            row1.Items.Add(blankButton)
            row1.Items.Add(New RibbonRowBreak())
            row1.Items.Add(blankButton)
            row1.Items.Add(New RibbonRowBreak())
            row1.Items.Add(blankButton)
            ribbonPanelSource.Items.Add(row1)
            Dim ribbonPanel = New RibbonPanel()
            ribbonPanel.Source = ribbonPanelSource
            Return ribbonPanel
        End Function

        Public Shared Function CreateRegressionTestsButton() As RibbonButton
            Dim ribbonButton = New RibbonButton()
            ribbonButton.Name = "Regression Tests"
            ribbonButton.ShowText = True
            ribbonButton.Text = "Regression Tests"

            Dim testRunner = New Workflows.NunitTestRunner()
            Dim allRegressionTests = GetType(Tests.RegressionTests)
            'Dim allIntegrationTests = GetType(Tests.RegressionTests)
            Dim allRegressionTestTypes = {allRegressionTests}
            'Dim allRegressionTestTypes = {allRegressionTests, allIntegrationTests}
            Dim uiRouter = New UiRouter(
                "cadwiki.AutoCAD2021.Base.Utilities",
                "cadwiki.AutoCAD2021.Base.Utilities.Workflows.NunitTestRunner",
                "Run",
                {allRegressionTestTypes},
                App.AcadAppDomainDllReloader,
                Assembly.GetExecutingAssembly())



            ribbonButton.CommandParameter = uiRouter
            ribbonButton.CommandHandler = New GenericClickCommandHandler()
            ribbonButton.ToolTip = "Runs regression tests from the current .dll"
            Return ribbonButton
        End Function

    End Class
End Namespace
