using System.Reflection;
using Autodesk.Windows;
using cadwiki.DllReloader.AutoCAD;
using cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{

    [TestClass()]
    public class TestAutoCADNetReloader
    {

        [TestMethod()]
        public void Test_Configure_FromAnEmptyIniFile_ShouldReturnNonEmptyString()
        {
            string actual;

            var appAssembly = Assembly.GetExecutingAssembly();
            var reloader = new AutoCADAppDomainDllReloader();
            reloader.DependencyValues.LogMode = AutodeskAppDomainReloader.LogMode.Text;
            reloader.ClearIni();
            reloader.Configure(appAssembly);
            actual = reloader.GetIExtensionApplicationClassName();

            Assert.AreNotEqual("", actual);
            Assert.AreNotEqual(null, actual);

        }
        [TestMethod()]
        public void Test_GenericCommandHandler_WithNoArguments_ShouldPass()
        {

            ExecuteRibbonButtonGenericCommandHandler("cadwiki.NetUtils", "cadwiki.NetUtils.AssemblyUtils", "GetCurrentlyExecutingAssembly", null);

        }

        [TestMethod()]
        public void Test_GenericCommandHandler_WithOneArgument_ShouldPass()
        {
            var appAssembly = Assembly.GetExecutingAssembly();
            object[] parameters = new[] { appAssembly };

            ExecuteRibbonButtonGenericCommandHandler("cadwiki.NetUtils", "cadwiki.NetUtils.AssemblyUtils", "GetTypesSafely", parameters);
        }

        private static void ExecuteRibbonButtonGenericCommandHandler(string assemblyName, string fullClassName, string methodName, object[] parameters)
        {

            var appAssembly = Assembly.GetExecutingAssembly();
            var reloader = new AutoCADAppDomainDllReloader();
            reloader.ClearIni();
            reloader.Configure(appAssembly);
            var uiRouter = new UiRouter(assemblyName, fullClassName, methodName, parameters, reloader, Assembly.GetExecutingAssembly());
            var ribbonButton = new RibbonButton();
            ribbonButton.CommandParameter = uiRouter;
            ribbonButton.CommandHandler = new GenericClickCommandHandler();
            ribbonButton.CommandHandler.Execute(ribbonButton);
        }


        [TestMethod()]
        public void Test_Configure_WithTrueLoadFlag_ShouldFindMoreThan0AssembliesAndLoad0Assemblies()
        {

            var appAssembly = Assembly.GetExecutingAssembly();
            var reloader = new AutoCADAppDomainDllReloader();
            reloader.ClearIni();
            reloader.Configure(appAssembly);
            reloader.Reload(appAssembly);
            var dllFound = reloader.GetDllsToReload();
            var reloadedDlls = reloader.GetDllsThatWereSuccessfullyReloaded();

            // Assert.AreNotEqual(dllFound.Count, 0)
            Assert.AreEqual(reloadedDlls.Count, 0);

        }

    }
}