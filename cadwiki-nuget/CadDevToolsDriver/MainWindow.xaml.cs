
using System.Reflection;
using System.Windows;

namespace CadDevToolsDriver
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // This call is required by the designer.
            this.InitializeComponent();
            this.Hide();
            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDir = System.IO.Path.GetDirectoryName(exePath);
            string tempDir = System.IO.Path.GetTempPath() + "cadwiki.TestPlugin";

            string wildCardFileName = "*" + "cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.dll";
            string testPluginDll = cadwiki.NetUtils.Paths.GetNewestDllInAnySubfolderOfSolutionDirectory(tempDir, wildCardFileName);

            var dependencies = new cadwiki.CadDevTools.MainWindow.Dependencies();
            dependencies.AutoCADExePath = @"C:\Program Files\Autodesk\AutoCAD 2024\acad.exe";
            dependencies.AutoCADStartupSwitches = "/p VANILLA";
            dependencies.DllFilePathToNetload = testPluginDll;
            dependencies.CustomDirectoryToSearchForDllsToLoadFrom = tempDir;
            dependencies.DllWildCardSearchPattern = wildCardFileName;

            Window Window = new cadwiki.CadDevTools.MainWindow(dependencies);
            Window.Show();
        }

    }
}