using System;
using Autodesk.Windows;
using Microsoft.VisualBasic;

namespace cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons
{
    public class DllReloadClickCommandHandler : System.Windows.Input.ICommand
    {


        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            doc.Editor.WriteMessage(Environment.NewLine + "DllReloadClickCommandHandler started..");

            try
            {
                if (parameter is RibbonButton)
                {
                    RibbonButton button = parameter as RibbonButton;
                    UiRouter uiRouter = button.CommandParameter as UiRouter;
                    if (uiRouter is null)
                    {
                        var window = new WpfUi.Templates.WindowAutoCADException(new Exception("Not able to cast parameter to UiRouter object."));
                        window.Show();
                        return;
                    }
                    if (doc is not null)
                    {
                        var netReloader = uiRouter.NetReloader;
                        var iExtensionAppAssembly = uiRouter.IExtensionAppAssembly;
                        string userInputDllPath = netReloader.UserInputGetDllPath();
                        if (string.IsNullOrEmpty(userInputDllPath))
                        {
                            return;
                        }
                        else
                        {
                            netReloader.ReloadDll(doc, iExtensionAppAssembly, userInputDllPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var window = new WpfUi.Templates.WindowAutoCADException(new Exception("Not able to cast parameter to UiRouter object."));
                window.Show();
            }

        }


    }



}