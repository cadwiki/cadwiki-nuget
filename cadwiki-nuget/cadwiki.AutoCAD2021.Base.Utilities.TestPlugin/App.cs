using System;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using cadwiki.DllReloader.AutoCAD;
using cadwiki.NetUtils;
using Microsoft.VisualBasic;

namespace cadwiki.AutoCAD2021.Base.Utilities.TestPlugin
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
            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            doc.Editor.WriteMessage(Environment.NewLine + "App initialize called...");
            // This Event Handler allows the IExtensionApplication to Resolve any Assemblies
            // The AssemblyResolve method finds the correct assembly in the AppDomain when there are multiple assemblies
            // with the same name and differing version number
            AppDomain.CurrentDomain.AssemblyResolve += AutodeskAppDomainReloader.AssemblyResolve;
            var iExtensionAppAssembly = Assembly.GetExecutingAssembly();
            var iExtensionAppVersion = AssemblyUtils.GetVersion(iExtensionAppAssembly);
            AcadAppDomainDllReloader.SkipCadwikiDlls = false;
            AcadAppDomainDllReloader.Configure(iExtensionAppAssembly);
            AcadAppDomainDllReloader.Reload(iExtensionAppAssembly);
            doc.Editor.WriteMessage(Environment.NewLine + "App " + iExtensionAppVersion.ToString() + " initialized...");
            doc.Editor.WriteMessage(Environment.NewLine);

            cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.UiRibbon.Tabs.TabCreator.AddDevTab(doc);

            // Dim allRegressionTests As Type = GetType(Tests.RegressionTests)
            // Dim allIntegrationTests As Type = GetType(MainApp.IntegrationTests.Tests)
            // Dim allTestTypes As Type() = {allRegressionTests}

            // Dim testRunner As Workflows.NunitTestRunner = New Workflows.NunitTestRunner()
            // testRunner.Run(allTestTypes)

        }


        // start here 3 - IExtensionApplication.Terminate
        // add a call to terminate the AcadAppDomainDllReloader
        public void Terminate()
        {
            AcadAppDomainDllReloader.Terminate();
        }

    }
}