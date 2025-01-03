
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            var cadApps = Directory.GetDirectories(tempDir).ToList().OrderByDescending(f => new FileInfo(f).CreationTime).ToList();
            DeleteFoldersOlderThanOneDay(cadApps);

            string wildCardFileName = "*" + "cadwiki.AC.TestPlugin.dll";
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

        private static void DeleteFoldersOlderThanOneDay(List<string> cadApps)
        {
            var dateNow = DateTime.Now;
            foreach (var cadApp in cadApps)
            {
                var dateCreated = new DirectoryInfo(cadApp).CreationTime;
                if (dateNow.Subtract(dateCreated).TotalDays > 1)
                {
                    try
                    {
                        Directory.Delete(cadApp, true);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}