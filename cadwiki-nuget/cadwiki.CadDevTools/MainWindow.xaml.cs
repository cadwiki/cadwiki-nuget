using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using InteropUtils = cadwiki.AC25.Interop.InteropUtils;

using cadwiki.NetUtils;

namespace cadwiki.CadDevTools
{

    public partial class MainWindow : Window
    {

        public class Dependencies
        {
            public string AutoCADExePath;
            public string AutoCADStartupSwitches;
            public string DllFilePathsToNetloadCommaDelimited;
            public string CustomDirectoryToSearchForDllsToLoadFrom;
            public string DllWildCardSearchPattern;
            public bool SetAutocadWindowToNorm;
        }

        private Dependencies _dependencies;

        public MainWindow()
        {
            previousAutoCADLocationValue = noneValue;
            // This call is required by the designer.
            this.InitializeComponent();
            StandardOnStartOperations();
            this.TextBoxStartupSwitches.Text = "";
        }

        public MainWindow(Dependencies dependencies)
        {
            previousAutoCADLocationValue = noneValue;
            // This call is required by the designer.
            this.InitializeComponent();
            StandardOnStartOperations();

            if (!string.IsNullOrEmpty(dependencies.AutoCADExePath))
            {
                acadLocation = dependencies.AutoCADExePath;
                if (File.Exists(acadLocation))
                {
                    this.ButtonLaunch.IsEnabled = true;
                    EditRichTextBoxWithAutoCADLocation();
                }
            }

            if (!string.IsNullOrEmpty(dependencies.DllFilePathsToNetloadCommaDelimited))
            {
                this.TextBoxDllPath.Text = dependencies.DllFilePathsToNetloadCommaDelimited;
            }

            if (!string.IsNullOrEmpty(dependencies.AutoCADStartupSwitches))
            {
                this.TextBoxStartupSwitches.Text = dependencies.AutoCADStartupSwitches;
            }

            _dependencies = dependencies;

        }

        private void StandardOnStartOperations()
        {
            var autocadReloader = new DllReloader.AutoCAD.AutoCADAppDomainDllReloader();
            autocadReloader.ClearIni();
            ReadCadDevToolsIniFromTemp();
            var bitMap = cadwiki.FileStore.ResourceIcons._500x500_cadwiki_v1;
            var bitMapImage = Bitmaps.BitMapToBitmapImage(bitMap);
            this.Icon = bitMapImage;
            EnableOrDisableControlsOnStart(previousAutoCADLocationValue);
        }


        private void EnableOrDisableControlsOnStart(string acadLocation)
        {
            if (!acadLocation.Equals(noneValue) & File.Exists(acadLocation))
            {
                this.ButtonLaunch.IsEnabled = true;
                EditRichTextBoxWithAutoCADLocation();
            }
            else
            {
                this.ButtonLaunch.IsEnabled = false;
            }
        }

        private string iniFileName = "cadwiki.CadDevToolsSettings.ini";
        private string iniSubFolder = "cadwiki.CadDevTools";
        private string noneValue = "(none)";
        private string previousAutoCADLocationKey = "PREVIOUS-AUTOCAD-LOCATION";
        private string previousAutoCADLocationValue;

        private void ReadCadDevToolsIniFromTemp()
        {
            string iniFilePath = GetCadDevToolsIniFilePath();
            var objIniFile = new IniFile(iniFilePath);
            previousAutoCADLocationValue = objIniFile.GetString("Settings", previousAutoCADLocationKey, noneValue);
            acadLocation = previousAutoCADLocationValue;
        }

        private string GetCadDevToolsIniFilePath()
        {
            string windowTempFolder = Path.GetTempPath();
            string iniFolder = windowTempFolder + @"\" + iniSubFolder;
            if (!Directory.Exists(iniFolder))
            {
                Directory.CreateDirectory(iniFolder);
            }
            string iniFilePath = iniFolder + @"\" + iniFileName;
            return iniFilePath;
        }

        private readonly List<string> acadLocations = new List<string>() { @"C:\Program Files\Autodesk\AutoCAD 2021\acad.exe", @"C:\Program Files\Autodesk\AutoCAD 2022\acad.exe", @"E:\Program Files\Autodesk\AutoCAD 2021\acad.exe", @"E:\Program Files\Autodesk\AutoCAD 2022\acad.exe" };

        private string acadLocation = "";

        private void ButtonSelectAutoCADYear_Click(object sender, RoutedEventArgs e)
        {
            var window = new WpfUi.WindowGetFilePath(acadLocations);
            window.ShowDialog();
            bool wasOkClicked = window.WasOkayClicked;
            if (wasOkClicked)
            {
                acadLocation = window.SelectedFolder;
                if (File.Exists(acadLocation))
                {
                    this.ButtonLaunch.IsEnabled = true;
                    EditRichTextBoxWithAutoCADLocation();
                    string iniFilePath = GetCadDevToolsIniFilePath();
                    var objIniFile = new IniFile(iniFilePath);
                    objIniFile.WriteString("Settings", previousAutoCADLocationKey, acadLocation);
                    WpfUi.Utils.SetSuccessStatus(this.TextBlockStatus, this.TextBlockMessage, "File does exist: " + acadLocation);
                }
                else
                {
                    WpfUi.Utils.SetErrorStatus(this.TextBlockStatus, this.TextBlockMessage, "File does not exist: " + acadLocation);
                }
            }

            else
            {
                WpfUi.Utils.SetErrorStatus(this.TextBlockStatus, this.TextBlockMessage, "File selection window was canceled.");
            }
        }

        private void EditRichTextBoxWithAutoCADLocation()
        {
            var flowDoc = new FlowDocument();
            var paragraph1 = new Paragraph();
            paragraph1.Inlines.Add(new Run("Selected program: " + Environment.NewLine));
            paragraph1.Inlines.Add(new Bold(new Run(acadLocation)));
            paragraph1.Inlines.Add(new Run(Environment.NewLine + "You can now use the other buttons."));
            flowDoc.Blocks.Add(paragraph1);
            this.RichTextBoxSelectedAutoCAD.Document = flowDoc;
        }


        private void ButtonLaunch_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Application.DoEvents();
            WpfUi.Utils.SetProcessingStatus(this.TextBlockStatus, this.TextBlockMessage, "Please wait until CAD launches and netloads " + this.TextBoxDllPath.Text + " into AutoCAD.");
            LaunchAutocad();

            string filePath = this.TextBoxDllPath.Text;
            var paths = filePath.Split(new string[] { "," }, StringSplitOptions.None);
            if (paths.Count() > 1)
            {
                foreach (var path in paths)
                {
                    NetloadDll(path);
                }
            }
            else if (File.Exists(filePath))
            {
                NetloadDll(filePath);
            }
        }

        private void LaunchAutocad()
        {

            WpfUi.Utils.SetProcessingStatus(this.TextBlockStatus, this.TextBlockMessage, "Please wait until CAD launches.");
            var interop2024 = new InteropUtils();
            bool isAutoCADRunning = interop2024.IsAutoCADRunning();
            if (isAutoCADRunning == false)
            {
                System.Windows.Forms.Application.DoEvents();
                var processInfo = new ProcessStartInfo()
                {
                    FileName = acadLocation,
                    Arguments = this.TextBoxStartupSwitches.Text
                };
                interop2024.StartAutoCADApp(processInfo);
            }
            interop2024.ConfigureRunningAutoCADForUsage();
            if (_dependencies.SetAutocadWindowToNorm)
            {
                interop2024.SetAutoCADWindowToNormal();
            }
            // interop.OpenDrawingTemplate(dwtFilePath, True)
            else
            {
                WpfUi.Utils.SetErrorStatus(this.TextBlockStatus, this.TextBlockMessage, "Invalid AutoCAD location: " + acadLocation);
            }

            WpfUi.Utils.SetSuccessStatus(this.TextBlockStatus, this.TextBlockMessage, "Autocad Launch complete.");
            System.Windows.Forms.Application.DoEvents();
        }

        private void NetloadDll(string cadAppDll)
        {
            WpfUi.Utils.SetProcessingStatus(this.TextBlockStatus, this.TextBlockMessage, "Please wait until CAD launches netloads the" + cadAppDll + " dll.");
            var interopAcCw = new InteropUtils();
            bool isAutoCADRunning = interopAcCw.IsAutoCADRunning();
            if (isAutoCADRunning == false)
            {
            }
            interopAcCw.NetloadDll(cadAppDll);
            WpfUi.Utils.SetSuccessStatus(this.TextBlockStatus, this.TextBlockMessage, "Dll netload complete: " + cadAppDll);
            System.Windows.Forms.Application.DoEvents();
        }

        private void ButtonSelectDll_Click(object sender, RoutedEventArgs e)
        {
            string folder = Directory.GetCurrentDirectory();
            string solutionDir = Paths.TryGetSolutionDirectoryPath();
            string wildCardFileName = "*.dll";
            List<string> dlls;

            if (!string.IsNullOrEmpty(_dependencies.DllWildCardSearchPattern))
            {
                wildCardFileName = _dependencies.DllWildCardSearchPattern;
            }

            if (string.IsNullOrEmpty(_dependencies.CustomDirectoryToSearchForDllsToLoadFrom))
            {
                dlls = Paths.GetAllWildcardFilesInAnySubfolder(solutionDir, wildCardFileName);
            }
            else
            {
                dlls = Paths.GetAllWildcardFilesInAnySubfolder(_dependencies.CustomDirectoryToSearchForDllsToLoadFrom, wildCardFileName);
            }

            var window = new WpfUi.WindowGetFilePath(dlls);
            window.Width = 1200d;
            window.Height = 300d;
            window.ShowDialog();
            bool wasOkClicked = window.WasOkayClicked;
            if (wasOkClicked)
            {
                string filePath = window.SelectedFolder;
                if (!File.Exists(filePath))
                {
                    WpfUi.Utils.SetErrorStatus(this.TextBlockStatus, this.TextBlockMessage, "Dll does not exist: " + filePath);
                }
                WpfUi.Utils.SetSuccessStatus(this.TextBlockStatus, this.TextBlockMessage, "Selected dll to load: " + filePath);
                this.TextBoxDllPath.Text = filePath;
            }
            else
            {
                WpfUi.Utils.SetErrorStatus(this.TextBlockStatus, this.TextBlockMessage, "User closed dll load menu.");
            }
        }

        private void ButtonFindNewestDllByName_Click(object sender, RoutedEventArgs e)
        {
            string dllName = Path.GetFileName(this.TextBoxDllPath.Text);
            string mainAppDll = GetNewestDllInSolutionDirectorySubFoldersThatHaveAVInFolderName(dllName);
            if (!File.Exists(mainAppDll))
            {
                WpfUi.Utils.SetErrorStatus(this.TextBlockStatus, this.TextBlockMessage, "Dll does not exist: " + mainAppDll);
            }
            else
            {
                WpfUi.Utils.SetSuccessStatus(this.TextBlockStatus, this.TextBlockMessage, "Selected dll to load: " + mainAppDll);
                this.TextBoxDllPath.Text = mainAppDll;
            }

        }

        public string GetNewestDllInSolutionDirectorySubFoldersThatHaveAVInFolderName(string dllName)
        {
            string solutionDir = Paths.TryGetSolutionDirectoryPath();
            string wildCardFileName = "*" + dllName;
            var cadApps = Paths.GetAllWildcardFilesInVSubfolder(solutionDir, wildCardFileName);
            string cadAppDll = cadApps.FirstOrDefault();
            return cadAppDll;
        }

    }
}