using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Reflection;
using Microsoft.VisualBasic.CompilerServices;

namespace cadwiki.DllReloader.AutoCAD
{
    public abstract class AutodeskAppDomainReloader
    {

        public string _cadwikiAutoCADAppDomainDllReloaderFolderName = "cadwiki.AutoCADAppDomainDllReloader";
        public ReloaderLog ReloaderLog;
        public bool SkipCadwikiDlls;

        public enum LogMode
        {
            Off,
            Text,
            AcadDocEditor
        }

        public class Dependencies
        {
            public string IExtensionApplicationClassName = "";
            public Version AppVersion = Version.Parse("1.0.0.0");
            public int ReloadCount = 0;
            public string DllPath = "";
            public Assembly ReloadedAssembly = null;
            public string OriginalAppDirectory = "";
            public List<string> DLLsToReload = new List<string>();
            public List<string> SuccessfullyReloadedDlls = new List<string>();
            public List<string> DllsToSkip = new List<string>() { "cadwiki.AcRemoveCmdGroup.dll", "AcCoreMgd.dll", "AcCui.dll", "AcDbMgd.dll", "acdbmgdbrep.dll", "AcDx.dll", "AcMgd.dll", "AcMr.dll", "AcSeamless.dll", "AcTcMgd.dll", "AcWindows.dll", "AdUIMgd.dll", "AdUiPalettes.dll", "AdWindows.dll" };
            public bool Terminated = true;
            public LogMode LogMode = LogMode.Text;
        }

        public void ClearIni()
        {
            WriteDependecyValuesToIni(new Dependencies());
        }


        public Dependencies DependencyValues;
        public string CadwikiTempFolder;

        private string _iniPath;

        private string _sectionSettings = "Settings";
        private string _keyProjectName = "ProjectName";
        private string _keyAppVersion = "AppVersion";
        private string _keyReloadCount = "ReloadCount";
        private string _keyDllPath = "DllPath";
        private string _keyOriginalAppDirectory = "OriginalAppDirectory";
        private string _keyDLLsToReload = "DLLsToReload";
        private string _keyTerminated = "Terminated";
        private string _keyLogMode = "LogMode";



        public AutodeskAppDomainReloader()
        {
            CadwikiTempFolder = GetDllReloaderTempFolder();
            _iniPath = CadwikiTempFolder + @"\" + "AutodeskAppDomainDllReloader.ini";
            DependencyValues = new Dependencies();
        }

        public AutodeskAppDomainReloader(Dependencies dependencies)
        {
            CadwikiTempFolder = GetDllReloaderTempFolder();
            _iniPath = CadwikiTempFolder + @"\" + "AutodeskAppDomainDllReloader.ini";
            DependencyValues = dependencies;
        }


        public string GetIExtensionApplicationClassName()
        {
            return DependencyValues.IExtensionApplicationClassName;
        }

        public Version GetVersion()
        {
            return DependencyValues.AppVersion;
        }

        public int GetReloadCount()
        {
            return DependencyValues.ReloadCount;
        }

        public string GetReloadedAssemblyNameSafely(Assembly currentIExtensionAppAssembly)
        {
            string dllName = DependencyValues.ReloadedAssembly is null ? currentIExtensionAppAssembly.FullName : DependencyValues.ReloadedAssembly.FullName;
            return dllName;
        }

        public string GetDllPath()
        {
            return DependencyValues.DllPath;
        }

        public List<string> GetDllsToReload()
        {
            return DependencyValues.DLLsToReload;
        }
        public List<string> GetDllsThatWereSuccessfullyReloaded()
        {
            return DependencyValues.SuccessfullyReloadedDlls;
        }

        public LogMode GetLogMode()
        {
            return DependencyValues.LogMode;
        }

        public void AddDllToSkip(string dllName)
        {
            DependencyValues.DllsToSkip.Add(dllName);
        }

        public void AddDllToReload(string dllFilePath)
        {
            bool isThisADllToSkip = DependencyValues.DllsToSkip.Contains(Path.GetFileName(dllFilePath));
            if (!isThisADllToSkip)
            {
                DependencyValues.DLLsToReload.Add(dllFilePath);
            }
        }




        public void Terminate()
        {
            DependencyValues = new Dependencies();
            SetTerminated(true);
            WriteDependecyValuesToIni(DependencyValues);
        }



        public string UserInputGetDllPath()
        {
            var filePaths = new List<string>();
            string filePathOfCurrentExecutingAssembly = Assembly.GetExecutingAssembly().Location;
            string exeDir = "";
            if (string.IsNullOrEmpty(filePathOfCurrentExecutingAssembly))
            {
                exeDir = DependencyValues.OriginalAppDirectory;
            }
            else
            {
                exeDir = Path.GetDirectoryName(filePathOfCurrentExecutingAssembly);
            }

            if (!string.IsNullOrEmpty(exeDir))
            {
                string parentDir = Directory.GetParent(exeDir).FullName;
                string wildCardFileName = "*" + DependencyValues.IExtensionApplicationClassName + ".dll";
                var cadApps = NetUtils.Paths.GetAllWildcardFilesInAnySubfolder(parentDir, wildCardFileName);
                filePaths.AddRange(cadApps);
            }

            var window = new WpfUi.WindowGetFilePath(filePaths);
            window.Width = 1200d;
            window.Height = 300d;
            window.ShowDialog();
            bool wasOkClicked = window.WasOkayClicked;
            if (wasOkClicked)
            {
                string filePath = window.SelectedFolder;
                return filePath;
            }
            else
            {
                return null;
            }
        }

        public static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly @assembly in assemblies)
            {
                if (string.Equals(assembly.FullName, args.Name))
                {
                    return assembly;
                }
            }
            return null;
        }



        public void WriteIniPathToDocEditor()
        {
            Log("Ini settings read from " + _iniPath);
        }



        public bool GetTerminatedFlag()
        {
            return DependencyValues.Terminated;
        }

        public void SetReloadedAssembly(Assembly @assembly)
        {
            DependencyValues.ReloadedAssembly = assembly;
        }

        public void SetOriginalAppDirectory(string orginalAppDirectory)
        {
            DependencyValues.OriginalAppDirectory = orginalAppDirectory;
        }

        public void SetDllPath(string dllPath)
        {
            DependencyValues.DllPath = dllPath;
        }

        public void SetAppVersion(Version appVersion)
        {
            DependencyValues.AppVersion = appVersion;
        }

        public void SetIExtensionApplicationClassName(string projectName)
        {
            DependencyValues.IExtensionApplicationClassName = projectName;
        }

        public void SetTerminated(bool terminated)
        {
            DependencyValues.Terminated = terminated;
        }

        public void SetReloadCount(int count)
        {
            DependencyValues.ReloadCount = count;
        }
        public void SetInitialValues(Assembly iExtensionAppAssembly)
        {
            // ReadDependecyValuesFromIni()
            // Only allow developer to set initial values when ReloadCount is 0
            if (DependencyValues.ReloadCount == 0)
            {
                SetIExtensionApplicationClassNameFromAssembly(iExtensionAppAssembly);
                var appVersion = NetUtils.AssemblyUtils.GetVersion(iExtensionAppAssembly);
                SetAppVersion(appVersion);
                SetReloadedAssembly(iExtensionAppAssembly);
                string assemblyPath = iExtensionAppAssembly.Location;
                if (!string.IsNullOrEmpty(assemblyPath))
                {
                    SetDllPath(assemblyPath);
                    SetOriginalAppDirectory(Path.GetDirectoryName(assemblyPath));
                }
                SetTerminated(false);
                WriteDependecyValuesToIni(DependencyValues);
            }
        }

        public void SetIExtensionApplicationClassNameFromAssembly(Assembly iExtensionAppAssembly)
        {
            string iExtensionAppClassName = iExtensionAppAssembly.GetName().Name;
            SetIExtensionApplicationClassName(iExtensionAppClassName);
        }

        public void SetReloadedValues(Assembly iExtensionAppAssembly)
        {
            ReadDependecyValuesFromIni();
            var appVersion = NetUtils.AssemblyUtils.GetVersion(iExtensionAppAssembly);
            SetAppVersion(appVersion);
            SetReloadedAssembly(iExtensionAppAssembly);
        }

        public void WriteDependecyValuesToIni(Dependencies dependencyValues)
        {
            CreateCadwikiTempFolderIfNotExists();
            var objIniFile = new NetUtils.IniFile(_iniPath);
            objIniFile.WriteString(_sectionSettings, _keyProjectName, dependencyValues.IExtensionApplicationClassName);
            objIniFile.WriteString(_sectionSettings, _keyAppVersion, dependencyValues.AppVersion.ToString());
            objIniFile.WriteString(_sectionSettings, _keyReloadCount, dependencyValues.ReloadCount.ToString());
            objIniFile.WriteString(_sectionSettings, _keyDllPath, dependencyValues.DllPath);
            objIniFile.WriteString(_sectionSettings, _keyOriginalAppDirectory, dependencyValues.OriginalAppDirectory);
            string DLLsConcat = string.Join(",", dependencyValues.DLLsToReload);
            objIniFile.WriteString(_sectionSettings, _keyDLLsToReload, DLLsConcat);
            objIniFile.WriteString(_sectionSettings, _keyTerminated, dependencyValues.Terminated.ToString());
            objIniFile.WriteString(_sectionSettings, _keyLogMode, dependencyValues.LogMode.ToString());
        }

        public void CreateCadwikiTempFolderIfNotExists()
        {
            if (!Directory.Exists(CadwikiTempFolder))
            {
                Directory.CreateDirectory(CadwikiTempFolder);
            }
        }

        public void ReadDependecyValuesFromIni()
        {
            if (!File.Exists(_iniPath))
            {
                WriteDependecyValuesToIni(DependencyValues);
            }
            var objIniFile = new NetUtils.IniFile(_iniPath);
            string stringValue;
            stringValue = objIniFile.GetString(_sectionSettings, _keyProjectName, DependencyValues.IExtensionApplicationClassName);
            DependencyValues.IExtensionApplicationClassName = stringValue;
            stringValue = objIniFile.GetString(_sectionSettings, _keyAppVersion, DependencyValues.AppVersion.ToString());
            var version = Version.Parse(stringValue);
            DependencyValues.AppVersion = version;
            stringValue = objIniFile.GetString(_sectionSettings, _keyReloadCount, DependencyValues.ReloadCount.ToString());
            DependencyValues.ReloadCount = Conversions.ToInteger(stringValue);
            stringValue = objIniFile.GetString(_sectionSettings, _keyDllPath, DependencyValues.DllPath);
            DependencyValues.DllPath = stringValue;
            stringValue = objIniFile.GetString(_sectionSettings, _keyOriginalAppDirectory, DependencyValues.OriginalAppDirectory);
            DependencyValues.OriginalAppDirectory = stringValue;
            string DLLsConcat = string.Join(",", DependencyValues.DLLsToReload);
            stringValue = objIniFile.GetString(_sectionSettings, _keyDLLsToReload, DLLsConcat);
            string[] dlls = stringValue is not null ? stringValue.Split(',') : Array.Empty<string>();
            DependencyValues.DLLsToReload = dlls.ToList();
            stringValue = objIniFile.GetString(_sectionSettings, _keyTerminated, DependencyValues.Terminated.ToString());
            DependencyValues.Terminated = Conversions.ToBoolean(stringValue);
            stringValue = objIniFile.GetString(_sectionSettings, _keyLogMode, DependencyValues.LogMode.ToString());
            Enum.TryParse(stringValue, out DependencyValues.LogMode);
        }



        public void WriteInfoAboutDllsToReload()
        {
            Log("Found " + DependencyValues.DLLsToReload.Count.ToString() + " dlls that are able to be loaded into the current appdomain.");
            if (DependencyValues.DLLsToReload.Count > 0)
            {
                Log("These dlls have 1 of 2 qualities listed below:");
                Log("#1 They don't exist in the app domain yet.");
                Log("or");
                Log("#2 Their version number is newer than any assembly with the exact same name in the current app domain.");
                if (DependencyValues.DLLsToReload.Count > 0)
                {
                    foreach (string dllToReload in DependencyValues.DLLsToReload)
                        Log("Dll to reload: " + dllToReload);
                }
            }
        }





        public Assembly GetAssemblyByName(string name, Assembly[] assemblies)
        {
            foreach (Assembly @assembly in assemblies)
            {
                if (assembly.GetName().Name.Equals(name))
                {
                    return assembly;
                }
            }
            return null;
        }


        public string GetNewReloadFolder(int count, DateTime time)
        {
            string timeStamp = GetTimestampForReloadFolder(time);
            string reloadFolder = CadwikiTempFolder + @"\" + timeStamp + "--Reload-" + count.ToString();
            string uniqueFolder = reloadFolder;
            int @int = 1;
            // While necessary, add "(#) until unique folder is found
            while (Directory.Exists(uniqueFolder))
            {
                uniqueFolder = reloadFolder + "(" + @int.ToString() + ")";
                @int = @int + 1;
            }
            return uniqueFolder;
        }

        public string GetTimestampForReloadFolder(DateTime time)
        {
            string format = "yyyyMMdd--HH_mm_ss";
            string timeStamp = time.ToString(format);
            return timeStamp;
        }

        public void Log(string message)
        {
            var mode = GetLogMode();
            switch (mode.Equals(LogMode.Off))
            {
                case object _ when mode.Equals(LogMode.Text):
                    {
                        LogToTextFile(message);
                        break;
                    }
            }
        }

        public void LogToTextFile(string message)
        {
            if (ReloaderLog is null)
            {
                ReloaderLog = new ReloaderLog();
                ReloaderLog.LogDir = CadwikiTempFolder;
                ReloaderLog.Write(message);
            }
            else
            {
                ReloaderLog.LogDir = CadwikiTempFolder;
                ReloaderLog.Write(message);
            }
        }


        public void LogException(Autodesk.AutoCAD.Runtime.Exception ex)
        {
            var mode = GetLogMode();
            switch (mode.Equals(LogMode.Off))
            {
                case object _ when mode.Equals(LogMode.Text):
                    {
                        LogExceptionToTextFile(ex);
                        break;
                    }
            }
        }

        public void LogExceptionToTextFile(Autodesk.AutoCAD.Runtime.Exception ex)
        {
            if (ReloaderLog is null)
            {
                ReloaderLog = new ReloaderLog();
                ReloaderLog.LogDir = CadwikiTempFolder;
                ReloaderLog.Exception(ex);
            }
            else
            {
                ReloaderLog.LogDir = CadwikiTempFolder;
                ReloaderLog.Exception(ex);
            }
        }

        public string GetDllReloaderTempFolder()
        {
            return Path.GetTempPath() + _cadwikiAutoCADAppDomainDllReloaderFolderName;
        }
    }

}