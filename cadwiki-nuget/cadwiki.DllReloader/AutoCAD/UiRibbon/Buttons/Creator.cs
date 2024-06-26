using System.Drawing;
using System.Reflection;
using Autodesk.Windows;

namespace cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons
{
    public class Creator
    {
        public static RibbonButton Create(string name, string text, string tooltip, Bitmap bitMap, string assemblyName, string fullClassName, string methodName, object[] parameters, AutoCADAppDomainDllReloader appDomainReloader, Assembly iExtensionAppAssembly)
        {


            var ribbonButton = new RibbonButton();
            ribbonButton.Name = name;
            ribbonButton.ShowText = true;
            ribbonButton.Text = text;
            ribbonButton.Size = RibbonItemSize.Standard;

            if (bitMap is not null)
            {
                var image = NetUtils.Bitmaps.CreateBitmapSourceFromBitmap(bitMap);
                if (image is not null)
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