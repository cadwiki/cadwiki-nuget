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
            var dllButton = CreatePaletteButton();
            var snapshotButton = CreateSnapshotButton();
            var blockReplacerButton = CreateBlockReplacerButton();
            var exportBlocksButton = CreateExportBlocksButton();
            var listBlocksButton = CreateListBlocksButton();
            var ribbonPanelSource = new RibbonPanelSource();
            ribbonPanelSource.Title = "Tests";
            var row1 = new RibbonRowPanel();
            row1.IsTopJustified = true;
            row1.Items.Add(integrationTestsButton);
            row1.Items.Add(new RibbonRowBreak());
            row1.Items.Add(dllButton);
            row1.Items.Add(new RibbonRowBreak());
            row1.Items.Add(snapshotButton);
            row1.Items.Add(new RibbonRowBreak());
            row1.Items.Add(blockReplacerButton);
            row1.Items.Add(new RibbonRowBreak());
            row1.Items.Add(exportBlocksButton);
            row1.Items.Add(new RibbonRowBreak());
            row1.Items.Add(listBlocksButton);
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


        public static RibbonButton CreatePaletteButton()
        {
            var ribbonButton = cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons.Creator.Create(
                "Palette",
                "Palette",
                "Palette",
                null,
                "cadwiki.AC",
                "cadwiki.AC.Commands.Commands",
                "ShowSamplePallete",
                null,
                App.AcadAppDomainDllReloader,
                Assembly.GetExecutingAssembly()
            );
            return ribbonButton;
        }

        public static RibbonButton CreateSnapshotButton()
        {
            var ribbonButton = cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons.Creator.Create(
                "Snapshot",
                "Snapshot",
                "Snapshot",
                null,
                "cadwiki.AC",
                "cadwiki.AC.Commands.Commands",
                "TakeSnapshot",
                null,
                App.AcadAppDomainDllReloader,
                Assembly.GetExecutingAssembly()
            );
            return ribbonButton;
        }

        public static RibbonButton CreateBlockReplacerButton()
        {
            var ribbonButton = cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons.Creator.Create(
                "Block Replacer",
                "Block Replacer",
                "Replace block references with new block definitions from an external file",
                null,
                "cadwiki.AC",
                "cadwiki.AC.Commands.BlockReplaceCommands",
                "ShowBlockReplacerPalette",
                null,
                App.AcadAppDomainDllReloader,
                Assembly.GetExecutingAssembly()
            );
            return ribbonButton;
        }

        public static RibbonButton CreateExportBlocksButton()
        {
            var ribbonButton = cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons.Creator.Create(
                "Export Blocks",
                "Export Blocks",
                "Export all block definitions from current drawing to individual DWG files",
                null,
                "cadwiki.AC",
                "cadwiki.AC.Commands.BlockReplaceCommands",
                "ExportBlocks",
                null,
                App.AcadAppDomainDllReloader,
                Assembly.GetExecutingAssembly()
            );
            return ribbonButton;
        }

        public static RibbonButton CreateListBlocksButton()
        {
            var ribbonButton = cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons.Creator.Create(
                "List Blocks",
                "List Blocks",
                "List all block definitions in current drawing",
                null,
                "cadwiki.AC",
                "cadwiki.AC.Commands.BlockReplaceCommands",
                "ListBlocks",
                null,
                App.AcadAppDomainDllReloader,
                Assembly.GetExecutingAssembly()
            );
            return ribbonButton;
        }
    }
}
