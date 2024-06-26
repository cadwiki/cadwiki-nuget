using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using Microsoft.VisualBasic;

namespace cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.UiRibbon.Tabs
{
    public class TabCreator
    {
        public static void AddDevTab(Document doc)
        {
            var devTab = DevTab.Create();
            DevTab.AddAllPanels(devTab);
            var allRibbonTabs = new List<RibbonTab>(new RibbonTab[] { devTab });
            AddTabs(doc, allRibbonTabs);
        }

        private static void AddTabs(Document doc, List<RibbonTab> ribbonTabs)
        {
            doc.Editor.WriteMessage(Constants.vbLf + "Adding tabs...");
            foreach (var ribbonTab in ribbonTabs)
                AddTab(doc, ribbonTab);
        }

        private static void AddTab(Document doc, RibbonTab ribbonTab)
        {
            doc.Editor.WriteMessage(Constants.vbLf + "Add tab...");
            var ribbonControl = ComponentManager.Ribbon;
            if (ribbonControl is not null & ribbonTab is not null)
            {
                if (Equals(ribbonTab.Name, null))
                {
                    throw new Exception("Ribbon Tab does not have a name. Please add a name to the ribbon tab.");
                }
                string tabName = ribbonTab.Name;
                doc.Editor.WriteMessage(Constants.vbLf + tabName);
                var doesTabAlreadyExist = ribbonControl.FindTab(tabName);
                if (doesTabAlreadyExist is not null)
                {
                    ribbonControl.Tabs.Remove(doesTabAlreadyExist);
                }
                ribbonControl.Tabs.Add(ribbonTab);
                ribbonTab.IsActive = true;
            }
        }
    }
}