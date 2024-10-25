
using Autodesk.Windows;
using cadwiki.AC.TestPlugin.UiRibbon.DevTab.Panels;

namespace cadwiki.AC.TestPlugin.UiRibbon.Tabs
{
    public class DevTab
    {
        private static readonly string tabName = "DevTab";

        public static RibbonTab Create()
        {
            var ribbonTab = new RibbonTab();
            ribbonTab.Title = tabName;
            ribbonTab.Id = tabName;
            ribbonTab.Name = tabName;
            return ribbonTab;
        }

        public static void AddAllPanels(RibbonTab ribbonTab)
        {
            var blankButton = new RibbonButton();
            blankButton.Name = "BlankButton1";
            blankButton.Size = RibbonItemSize.Standard;
            blankButton.IsEnabled = false;
            var infoRibbonPanel = Info.CreateInfoPanel(blankButton);
            ribbonTab.Panels.Add(infoRibbonPanel);
            var testPanel = Test.CreateTestsPanel(blankButton);
            ribbonTab.Panels.Add(testPanel);
        }
    }


}