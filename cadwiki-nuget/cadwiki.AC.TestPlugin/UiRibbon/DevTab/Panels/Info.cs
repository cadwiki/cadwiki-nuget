
using System.IO;
using System.Reflection;
using Autodesk.Windows;
using static cadwiki.DllReloader.AutoCAD.AcadAssemblyUtils;
using cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons;
using System.Windows;
using System;

namespace cadwiki.AC.TestPlugin.UiRibbon.DevTab.Panels
{
    public class Info
    {
        public static RibbonPanel CreateInfoPanel(RibbonButton blankButton)
        {
            var row1 = new RibbonRowPanel();
            var row2 = new RibbonRowPanel();
            var row3 = new RibbonRowPanel();
            var row4 = new RibbonRowPanel();
            var currentIExtensionAppAssembly = Assembly.GetExecutingAssembly();
            string dllName = App.AcadAppDomainDllReloader.GetReloadedAssemblyNameSafely(currentIExtensionAppAssembly);
            string versionNumberStr = "";
            string exeName = "";
            if (App.AcadAppDomainDllReloader.GetReloadCount() >= 1)
            {
                exeName = Path.GetFileName(App.AcadAppDomainDllReloader.GetDllPath());
                versionNumberStr = GetAssemblyVersionFromFullName(dllName);
            }
            else
            {
                string filePath = Assembly.GetExecutingAssembly().Location;
                exeName = Path.GetFileName(filePath);
                versionNumberStr = GetAssemblyVersionFromFullName(currentIExtensionAppAssembly.FullName);
            }
            var versionNumber = CreateVersionNumberButton(versionNumberStr);
            var assemblyName = CreateAssemblyNameButton(exeName);
            var reloadCount = CreateReloadCountButton(exeName);
            var pipelineCount = CreateReloadPipelineCountButton(exeName);
            var ribbonPanelSource = new RibbonPanelSource();
            ribbonPanelSource.Title = "Info";
            ribbonPanelSource.Items.Add(row1);
            ribbonPanelSource.Items.Add(new RibbonRowBreak());
            ribbonPanelSource.Items.Add(row2);
            ribbonPanelSource.Items.Add(new RibbonRowBreak());
            ribbonPanelSource.Items.Add(row3);
            ribbonPanelSource.Items.Add(new RibbonRowBreak());
            ribbonPanelSource.Items.Add(row4);
            var ribbonPanel = new RibbonPanel();
            ribbonPanel.Source = ribbonPanelSource;
            row1.Items.Add(versionNumber);
            row2.Items.Add(assemblyName);
            row3.Items.Add(reloadCount);
            row4.Items.Add(pipelineCount);
            row4.Items.Add(blankButton);
            return ribbonPanel;
        }

        private static RibbonButton CreateVersionNumberButton(string versionNumberStr)
        {
            var versionNumber = new RibbonButton();
            versionNumber.Name = "Version";
            versionNumber.ShowText = true;
            versionNumber.Text = " v" + versionNumberStr + " ";
            versionNumber.Size = RibbonItemSize.Standard;
            versionNumber.IsEnabled = false;
            return versionNumber;
        }

        // start here 4 - Reload button
        // this button handles the logic of Reloading a dll into AutoCAD's current application domain
        // once a new dll is reloaded, the AutoCADAppDomainDllReloader will
        // route future Ui clicks to the newly reloaded dlls methods
        private static RibbonButton CreateReloadCountButton(string exeName)
        {
            var button = new RibbonButton();
            button.Name = "ReloadCount";
            button.ShowText = true;
            button.Text = " Reload Count: " + App.AcadAppDomainDllReloader.GetReloadCount().ToString();
            button.Size = RibbonItemSize.Standard;
            button.CommandHandler = new DllReloadClickCommandHandler();
            button.ToolTip = "Reload the " + exeName + " dll into AutoCAD";
            object[] parameters = new object[] { };

            var uiRouter = new UiRouter("assemblyName: not used by DllReloadClickCommandHandler", "fullClassName: not used by DllReloadClickCommandHandler", "methodName: not used by DllReloadClickCommandHandler", parameters, App.AcadAppDomainDllReloader, Assembly.GetExecutingAssembly());


            button.CommandParameter = uiRouter;
            return button;
        }

        private static RibbonButton CreateAssemblyNameButton(string exeName)
        {
            var assemblyName = new RibbonButton();
            assemblyName.Name = "ReleaseStatus";
            assemblyName.ShowText = true;
            assemblyName.Text = " Dll: " + exeName;
            assemblyName.Size = RibbonItemSize.Standard;
            assemblyName.IsEnabled = false;
            return assemblyName;
        }


        // start here 5 - Reload button
        // this button handles the logic of Reloading a dll into AutoCAD's current application domain
        // once a new dll is reloaded, the AutoCADAppDomainDllReloader will
        // route future Ui clicks to the newly reloaded dlls methods
        private static RibbonButton CreateReloadPipelineCountButton(string exeName)
        {
            var button = new RibbonButton();
            button.Name = "PipelineReloadCount";
            button.ShowText = true;
            button.Text = "Pipeline Count: " + App.AcadAppDomainDllReloader.GetReloadCount().ToString();
            button.Size = RibbonItemSize.Standard;
            button.CommandHandler = new PipelineReloadClickCommandHandler();
            button.ToolTip = "Reload pipeline into AutoCAD";
            object[] parameters = new object[] { };

            var uiRouter = new UiRouter("assemblyName: not used by CreateReloadPipelineCountButton", "fullClassName: not used by CreateReloadPipelineCountButton", "methodName: not used by CreateReloadPipelineCountButton", parameters, App.AcadAppDomainDllReloader, Assembly.GetExecutingAssembly());


            button.CommandParameter = uiRouter;
            return button;
        }
    }

    public class PipelineReloadClickCommandHandler : System.Windows.Input.ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            doc.Editor.WriteMessage(Environment.NewLine + "PipelineReloadClickCommandHandler started..");

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
                    if (doc != null)
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
                            var fileNameNoExt = Path.GetFileNameWithoutExtension(userInputDllPath);
                            var fileName = Path.GetFileName(userInputDllPath);
                            var dirName = Path.GetDirectoryName(userInputDllPath);
                            var pipeline = new cadwiki.DllReloader.PluginReloadService.PluginReloadPipeline(
                                pluginName: fileNameNoExt,
                                sourceBuildDir: dirName,
                                mainDllName: fileName,
                                reloader: netReloader
                            );

                            var result = pipeline.Execute(doc);

                            if (!result.IsSuccess)
                            {
                                var window = new WpfUi.Templates.WindowAutoCADException(
                                    new Exception($"Reload failed: {result.Error?.Message}\nSee log: {result.LogFilePath}"));
                                window.Show();
                                return;
                            }
                            else if (!result.ReloadTriggered)
                            {
                                var window = new WpfUi.Templates.WindowAutoCADException(
                                    new Exception($"Staged OK but reload not triggered.\nStaged at: {result.StagingResult?.StagingFolder}"));
                                window.Show();
                                return;
                            }
                            else
                            {
                                doc.Editor.WriteMessage($"\nPlugin reloaded in {result.Elapsed.TotalMilliseconds:0}ms\n");
                            }
                                
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var window = new WpfUi.Templates.WindowAutoCADException(ex);
                window.Show();
            }

        }


    }



}