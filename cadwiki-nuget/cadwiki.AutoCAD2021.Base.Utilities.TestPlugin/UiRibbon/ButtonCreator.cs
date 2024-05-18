using System.Drawing;
using System.Reflection;
using Autodesk.Windows;
using cadwiki.DllReloader.AutoCAD;
using cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons;

namespace cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.UiRibbon
{
    internal class ButtonCreator
    {
        public static RibbonButton Create(string name, string text, string tooltip, Bitmap bitMap, string assemblyName, string fullClassName, string methodName, object[] parameters, AutoCADAppDomainDllReloader appDomainReloader, Assembly iExtensionAppAssembly)
        {


            var ribbonButton = new RibbonButton();
            ribbonButton.Name = name;
            ribbonButton.ShowText = true;
            ribbonButton.Text = text;
            ribbonButton.Size = RibbonItemSize.Standard;

            if (bitMap != null)
            {
                var image = FileStore.Bitmaps.CreateBitmapSourceFromGdiBitmapForAutoCADButtonIcon(bitMap);
                if (image != null)
                {
                    ribbonButton.Image = image;
                    ribbonButton.ShowImage = true;
                }
            }

            var uiRouter = new UiRouter(assemblyName, fullClassName, methodName, parameters, appDomainReloader, iExtensionAppAssembly);
            ribbonButton.CommandParameter = uiRouter;
            ribbonButton.CommandHandler = new GenericClickCommandHandler();
            ribbonButton.ToolTip = tooltip;
            return ribbonButton;
        }
    }
}