using System;
using System.Reflection;
using System.Windows.Controls;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using cadwiki.DllReloader.AutoCAD;
using cadwiki.DllReloader.AutoCAD.UiRibbon;
using cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons;
using cadwiki.NetUtils;
using Microsoft.VisualBasic;

namespace cadwiki.AC.TestPlugin
{

    public class App : IExtensionApplication
    {


        // start here 1 - AutoCADAppDomainDllReloader
        // this variable handles routing the Ui clicks on a AutoCAD ribbon button to your methods found in an Assembly
        public static AutoCADAppDomainDllReloader AcadAppDomainDllReloader = new AutoCADAppDomainDllReloader();

        // start here 2 - IExtensionApplication.Initialize
        // once the AcadAppDomainDllReloader is configured with the current Assembly, it will be able to route Ui clicks
        // to the correct method
        public void Initialize()
        {
            try
            {
                var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                doc.Editor.WriteMessage(System.Environment.NewLine + "App initialize called...");
                // This Event Handler allows the IExtensionApplication to Resolve any Assemblies
                // The AssemblyResolve method finds the correct assembly in the AppDomain when there are multiple assemblies
                // with the same name and differing version number
                AppDomain.CurrentDomain.AssemblyResolve += AutodeskAppDomainReloader.AssemblyResolve;
                var iExtensionAppAssembly = Assembly.GetExecutingAssembly();
                var iExtensionAppVersion = cadwiki.NetUtils.AssemblyUtils.GetVersion(iExtensionAppAssembly);
                AcadAppDomainDllReloader.SkipCadwikiDlls = false;
                AcadAppDomainDllReloader.Configure(iExtensionAppAssembly);
                AcadAppDomainDllReloader.Reload(iExtensionAppAssembly);
                doc.Editor.WriteMessage(System.Environment.NewLine + "App " + iExtensionAppVersion.ToString() + " initialized...");
                doc.Editor.WriteMessage(System.Environment.NewLine);
                DevRibbon.Show(doc, AcadAppDomainDllReloader, Assembly.GetExecutingAssembly(),
                    pipelineAction: () =>
                    {
                        CustomPipeLine(iExtensionAppAssembly);
                    });

                var test1Button = RibbonButtonDefinition.Builder("Button v" + iExtensionAppVersion)
                    .Text("Test v" + iExtensionAppVersion)
                    .OnClick(() =>
                    {
                        ButtonAction();
                    })
                    .Build();

                var testPanel = new RibbonPanelDefinition("Panel v" + iExtensionAppVersion)
                    .Add(test1Button);

                // ── Tab ─────────────────────────────────────────────────────────
                RibbonBuilder.InjectPanel("cw Dev", testPanel);

                if (doc != null)
                {
                    var reactors = new ReactorsRibbonCreate();
                    reactors.AttachQuiescentReactors(doc);
                }



            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void ButtonAction()
        {
            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var userInput = Interaction.InputBox("Enter something:", "Test Input", "Default value");
            doc.Editor.WriteMessage($"\nYou entered: {userInput}");
        }

        private static void CustomPipeLine(Assembly iExtensionAppAssembly)
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            doc.Editor.WriteMessage(System.Environment.NewLine + "CustomPipeLine started..");
            try
            {
                if (doc != null)
                {
                    var netReloader = AcadAppDomainDllReloader;
                    string userInputDllPath = netReloader.UserInputGetDllPath();
                    if (string.IsNullOrEmpty(userInputDllPath))
                    {
                        return;
                    }
                    else
                    {
                        var fileNameNoExt = System.IO.Path.GetFileNameWithoutExtension(userInputDllPath);
                        var fileName = System.IO.Path.GetFileName(userInputDllPath);
                        var dirName = System.IO.Path.GetDirectoryName(userInputDllPath);
                        var pipeline = new cadwiki.DllReloader.PluginReloadService.PluginReloadPipeline(
                            pluginName: fileNameNoExt,
                            sourceBuildDir: dirName,
                            mainDllName: fileName,
                            reloader: netReloader
                        );

                        var result = pipeline.Execute(doc);

                        if (!result.IsSuccess)
                        {
                            var window = new cadwiki.WpfUi.Templates.WindowAutoCADException(
                                new System.Exception($"Reload failed: {result.Error?.Message}\nSee log: {result.LogFilePath}"));
                            window.Show();
                            return;
                        }
                        else if (!result.ReloadTriggered)
                        {
                            var window = new cadwiki.WpfUi.Templates.WindowAutoCADException(
                                new System.Exception($"Staged OK but reload not triggered.\nStaged at: {result.StagingResult?.StagingFolder}"));
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
            catch (System.Exception ex)
            {
                var window = new cadwiki.WpfUi.Templates.WindowAutoCADException(ex);
                window.Show();
            }
        }




        // start here 3 - IExtensionApplication.Terminate
        // add a call to terminate the AcadAppDomainDllReloader
        public void Terminate()
        {
            AcadAppDomainDllReloader.Terminate();
        }

    }
}