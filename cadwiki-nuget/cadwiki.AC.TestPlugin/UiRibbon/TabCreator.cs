using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using Microsoft.VisualBasic;

namespace cadwiki.AC.TestPlugin.UiRibbon.Tabs
{
    public class TabCreator
    {
        public static bool AddDevTab(Document doc)
        {
            var devTab = DevTab.Create();
            DevTab.AddAllPanels(devTab);
            var allRibbonTabs = new List<RibbonTab>(new RibbonTab[] { devTab });
            return AddTabs(doc, allRibbonTabs);
        }

        private static bool AddTabs(Document doc, List<RibbonTab> ribbonTabs)
        {
            doc.Editor.WriteMessage(Environment.NewLine + "Adding tabs...");
            var wasTabAdded = false;
            var wereAllTabsAdded = true;
            foreach (var ribbonTab in ribbonTabs)
            {
                wasTabAdded = AddTab(doc, ribbonTab);
                if (!wasTabAdded)
                {
                    doc.Editor.WriteMessage(Environment.NewLine + "Failed to add tab..." + ribbonTab.Name);
                    wereAllTabsAdded = false;
                }
            }
            return wereAllTabsAdded;
        }

        private static bool AddTab(Document doc, RibbonTab ribbonTab)
        {
            doc.Editor.WriteMessage(Environment.NewLine + "Add tab...");
            var ribbonControl = ComponentManager.Ribbon;
            if (ribbonControl != null && ribbonTab != null)
            {
                if (Equals(ribbonTab.Name, null))
                {
                    throw new Exception("Ribbon Tab does not have a name. Please add a name to the ribbon tab.");
                }
                string tabName = ribbonTab.Name;
                doc.Editor.WriteMessage(Environment.NewLine + tabName);
                var doesTabAlreadyExist = ribbonControl.FindTab(tabName);
                if (doesTabAlreadyExist != null)
                {
                    ribbonControl.Tabs.Remove(doesTabAlreadyExist);
                }
                ribbonControl.Tabs.Add(ribbonTab);
                ribbonTab.IsActive = true;
                return true;
            }
            else
            {
                doc.Editor.WriteMessage(Environment.NewLine + "Ribbon is not shown...type RIBBON into AutoCAD command line.");
            }
            return false;
        }
    }
}