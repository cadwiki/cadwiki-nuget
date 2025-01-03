using System;
using System.Reflection;
using Autodesk.Windows;
using cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons;

namespace cadwiki.AC.TestPlugin.UiRibbon.DevTab.Panels
{
    public class Test
    {

        public static RibbonPanel CreateTestsPanel(RibbonButton blankButton)
        {
            var integrationTestsButton = CreateRegressionTestsButton();
            var ribbonPanelSource = new RibbonPanelSource();
            ribbonPanelSource.Title = "Tests";
            var row1 = new RibbonRowPanel();
            row1.IsTopJustified = true;
            row1.Items.Add(integrationTestsButton);
            row1.Items.Add(new RibbonRowBreak());
            row1.Items.Add(blankButton);
            row1.Items.Add(new RibbonRowBreak());
            row1.Items.Add(blankButton);
            row1.Items.Add(new RibbonRowBreak());
            row1.Items.Add(blankButton);
            ribbonPanelSource.Items.Add(row1);
            var ribbonPanel = new RibbonPanel();
            ribbonPanel.Source = ribbonPanelSource;
            return ribbonPanel;
        }

        public static RibbonButton CreateRegressionTestsButton()
        {

            var testRunner = new Workflows.NunitTestRunner();
            var allRegressionTests = typeof(Tests.RegressionTests);
            // Dim allIntegrationTests = GetType(Tests.RegressionTests)
            Type[] allRegressionTestTypes = new[] { allRegressionTests };
            // Dim allRegressionTestTypes = {allRegressionTests, allIntegrationTests}
            var ribbonButton = cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons.Creator.Create(
                "Regression Tests", "Regression Tests", "Runs regression tests from the current .dll", null,
                "cadwiki.AC", "cadwiki.AC.Workflows.NunitTestRunner", "Run", 
                new[] { allRegressionTestTypes }, App.AcadAppDomainDllReloader, Assembly.GetExecutingAssembly());

            return ribbonButton;
        }

    }
}