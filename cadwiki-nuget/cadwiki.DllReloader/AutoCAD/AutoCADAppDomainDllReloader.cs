using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Microsoft.VisualBasic;

namespace cadwiki.DllReloader.AutoCAD
{
    public class AutoCADAppDomainDllReloader : AutodeskAppDomainReloader
    {

        private Document _document;
        private string _tempFolder;

        public AutoCADAppDomainDllReloader()
        {
            _tempFolder = CadwikiTempFolder;
        }


        public new LogMode GetLogMode()
        {
            return DependencyValues.LogMode;
        }

        public void Configure(Assembly currentIExtensionAppAssembly)
        {
            try
            {
                Log("---------------------------------------------");
                Log("---------------------------------------------");
                Log("Configure started.");
                ReadDependecyValuesFromIni();
                // If Terminated = True
                // And Versions don't match
                // this is the first Initalize call from any IExtensionApplication in this AutoCAD session
                bool wasLastReloaderStateTerminated = GetTerminatedFlag();
                if (wasLastReloaderStateTerminated == true)
                {
                    // Not NetReloader.GetVersion().Equals(iExtensionAppVersion) Then
                    SetReloadCount(0);
                    SetInitialValues(currentIExtensionAppAssembly);
                }

                if (string.IsNullOrEmpty(GetIExtensionApplicationClassName()))
                {
                    SetIExtensionApplicationClassNameFromAssembly(currentIExtensionAppAssembly);
                }
                Log("Configure complete.");
                Log("---------------------------------------------");
                Log("---------------------------------------------");
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                var window = new WpfUi.Templates.WindowAutoCADException(ex);
                window.Show();
            }

        }












        public void Reload(Assembly currentIExtensionAppAssembly)
        {
            try
            {
                Log("---------------------------------------------");
                Log("---------------------------------------------");
                Log("Reload started.");
                // If Terminated = True
                // And Versions don't match
                // this is the first Initalize call from any IExtensionApplication in this AutoCAD session
                bool wasLastReloaderStateTerminated = GetTerminatedFlag();
                if (wasLastReloaderStateTerminated == true)
                {
                    // Not NetReloader.GetVersion().Equals(iExtensionAppVersion) Then
                    string dllPath = NetUtils.AssemblyUtils.GetFileLocationFromCodeBase(currentIExtensionAppAssembly);
                    ReloadAllDllsFoundInSameFolder(dllPath);
                }
                else
                {
                    SetReloadedValues(currentIExtensionAppAssembly);
                }

                if (string.IsNullOrEmpty(GetIExtensionApplicationClassName()))
                {
                    SetIExtensionApplicationClassNameFromAssembly(currentIExtensionAppAssembly);
                }
                Log("Reload complete.");
                Log("---------------------------------------------");
                Log("---------------------------------------------");
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                var window = new WpfUi.Templates.WindowAutoCADException(ex);
                window.Show();
            }
        }

        // Called from DllReloadClickCommandHandler
        public void ReloadDll(Document doc, Assembly iExtensionAppAssembly, string dllPath)
        {

            if (dllPath is not null)
            {

                try
                {
                    _document = doc;
                    Log("---------------------------------------------");
                    Log("---------------------------------------------");
                    Log("Dll reload started.");
                    WriteIniPathToDocEditor();
                    // Remove all commands from iExtensionAppAssembly
                    CommandRemover.RemoveAllCommandsFromiExtensionAppAssembly(doc, iExtensionAppAssembly, dllPath);
                    // RemoveAllCommandsFromAllAssembliesInAppDomain(doc, dllPath)
                    var tuple = ReloadAllDllsFoundInSameFolder(dllPath);
                    var appAssembly = tuple.Item1;
                    string copiedMainDll = tuple.Item2;

                    if (DependencyValues.OriginalAppDirectory is null)
                    {
                        DependencyValues.OriginalAppDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    }
                    string originalDirectory = DependencyValues.OriginalAppDirectory;
                    Log(string.Format("Original app directory: {0}", originalDirectory));
                    Log(string.Format("Dll reload path: {0}", copiedMainDll));
                    Type[] types = NetUtils.AssemblyUtils.GetTypesSafely(appAssembly);
                    WriteIniPathToDocEditor();
                    Log("Dll reload complete.");
                    Log("---------------------------------------------");
                    Log("---------------------------------------------");
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Log("Exception" + ex.Message);
                }
            }
        }






        private Tuple<Assembly, string> ReloadAllDllsFoundInSameFolder(string dllPath)
        {
            try
            {
                Log(string.Format("Reload count {0}.", DependencyValues.ReloadCount));
                int newCount = DependencyValues.ReloadCount + 1;
                string dllRepository = Path.GetDirectoryName(dllPath);
                var now = DateTime.Now;
                _tempFolder = GetNewReloadFolder(newCount, now);
                Directory.CreateDirectory(_tempFolder);
                Log("Created temp folder to copy dlls to for reloading: " + _tempFolder);
                var tempDlls = CopyAllDllsToTempFolder(dllPath, _tempFolder);
                var tuple = ReloadAll(tempDlls, newCount);
                return tuple;
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                var window = new WpfUi.Templates.WindowAutoCADException(ex);
                window.Show();
                return null;
            }
        }

        private List<string> CopyAllDllsToTempFolder(string dllPath, string tempFolder)
        {
            var tempDlls = new List<string>();
            foreach (string dllFilePath in Directory.GetFiles(Path.GetDirectoryName(dllPath), "*.dll"))
            {
                string dllName = Path.GetFileName(dllFilePath);
                string tempFolderFilePath = tempFolder + @"\" + dllName;
                File.Copy(dllFilePath, tempFolderFilePath);
                Log("Copied: " + tempFolderFilePath);
                tempDlls.Add(tempFolderFilePath);
            }
            return tempDlls;
        }

        private Tuple<Assembly, string> ReloadAll(List<string> tempDlls, int reloadCount)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly appAssembly = null;
            string appAssemblyPath = "";
            Log("Looking for dlls to reload.");
            Log("Skipping these dlls: ");
            foreach (string dllToSkip in DependencyValues.DllsToSkip)
                Log(dllToSkip);
            appAssemblyPath = AddDllsToReloadToList(tempDlls, assemblies, appAssemblyPath);
            if (string.IsNullOrEmpty(appAssemblyPath))
            {
                string errorMessage = "Unable to locate the Assembly whose name contains: " + DependencyValues.IExtensionApplicationClassName;
                Log(errorMessage);
            }
            else
            {
                // Add appAssembly as the last Item of the list, to ensure all other dlls are loaded before
                AddDllToReload(appAssemblyPath);
            }
            WriteInfoAboutDllsToReload();
            // Remove all commands that will be reloaded into the app domain
            RemoveAllCommandsFromAnyAssemblyThatWillBeReloaded();
            ReloadDllsIntoAppDomain();
            return new Tuple<Assembly, string>(appAssembly, appAssemblyPath);
        }


        private void RemoveAllCommandsFromAnyAssemblyThatWillBeReloaded()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (string dllFilePath in DependencyValues.DLLsToReload)
            {
                string dllName = Path.GetFileName(dllFilePath);
                string assemblyName = Path.GetFileNameWithoutExtension(dllName);
                var newestAssemblyWithNameInAppDomain = AcadAssemblyUtils.GetNewestAssembly(assemblies, assemblyName, null);
                // Remove any commands that need to be overwritten latter
                if (newestAssemblyWithNameInAppDomain is not null)
                {
                    CommandRemover.RemoveAllCommandsFromiExtensionAppAssembly(_document, newestAssemblyWithNameInAppDomain, DependencyValues.OriginalAppDirectory);
                }

            }
        }

        private string AddDllsToReloadToList(List<string> tempDlls, Assembly[] assemblies, string appAssemblyPath)
        {
            DependencyValues.DLLsToReload.Clear();
            foreach (string tempDll in tempDlls)
            {
                if (File.Exists(tempDll))
                {
                    string dllName = Path.GetFileName(tempDll);
                    string assemblyName = Path.GetFileNameWithoutExtension(tempDll);
                    var newestAssemblyWithNameInAppDomain = AcadAssemblyUtils.GetNewestAssembly(assemblies, assemblyName, null);
                    // If there is not an existing assembly like this in the app domain
                    // add to list
                    if (newestAssemblyWithNameInAppDomain is null)
                    {
                        AddDllToReload(tempDll);
                    }
                    else
                    {
                        // skip all cadwiki dlls
                        if (SkipCadwikiDlls & dllName.Contains("cadwiki."))
                        {
                            Log("Skipped cadwiki dll: " + dllName);
                            continue;
                        }
                        // Else do version checks
                        string newestVersionInAppDomain = AcadAssemblyUtils.GetAssemblyVersionFromFullName(newestAssemblyWithNameInAppDomain.FullName);
                        var copiedFvi = FileVersionInfo.GetVersionInfo(tempDll);
                        string copiedVersion = copiedFvi.FileVersion;
                        int isCopiedDllNewer = AcadAssemblyUtils.CompareFileVersion(copiedVersion, newestVersionInAppDomain);
                        // For dlls that already exist in the app domain, only reload if the copy has a newer file version
                        // which result in a 1 comparision between file versions
                        if (isCopiedDllNewer == 1)
                        {
                            // If dll name contains project name, don't store to list
                            if (assemblyName.Contains(DependencyValues.IExtensionApplicationClassName))
                            {
                                Log("Skipped iExtension app dll");
                                appAssemblyPath = tempDll;
                            }
                            else
                            {
                                Log(dllName + " added to list. App domain version:" + newestVersionInAppDomain + ", copied version:" + copiedVersion);
                                // Else store to list
                                AddDllToReload(tempDll);
                            }
                        }
                        else
                        {
                            Log(dllName + " skipped. App domain version:" + newestVersionInAppDomain + ", copied version:" + copiedVersion);
                        }
                    }
                }

                else
                {
                }
            }

            return appAssemblyPath;
        }

        private void ReloadDllsIntoAppDomain()
        {
            Assembly assemblyWithIExtensionApp = null;
            foreach (string dllPath in DependencyValues.DLLsToReload)
            {
                byte[] assemblyBytes = null;
                try
                {
                    assemblyBytes = File.ReadAllBytes(dllPath);
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Log("Error reading assembly to byte array: " + dllPath);
                    Log("Exception: " + ex.Message);
                }
                try
                {
                    if (dllPath.Contains(DependencyValues.IExtensionApplicationClassName))
                    {
                        // Update Reloader values
                        DependencyValues.ReloadCount += 1;
                        DependencyValues.Terminated = false;
                        WriteDependecyValuesToIni(DependencyValues);
                        // Upon loading the assemblyBytes from the IExtensionApplication class, the App.Initialize() method will be called
                        assemblyWithIExtensionApp = AppDomain.CurrentDomain.Load(assemblyBytes);
                        Log("Reloaded iExtensionAppAssembly dll: " + dllPath);
                        SetReloadedValues(assemblyWithIExtensionApp);
                        WriteDependecyValuesToIni(DependencyValues);
                    }
                    else
                    {
                        var reloadedAssembly = AppDomain.CurrentDomain.Load(assemblyBytes);
                        Log("Reloaded dll: " + dllPath);
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Log("Error loading assembly: " + dllPath);
                    Log("Exception: " + ex.Message);
                }
            }
            Type[] currentTypes = NetUtils.AssemblyUtils.GetTypesSafely(assemblyWithIExtensionApp);
            // Create reference to the IExtensionApplication object
            var currentAppObject = AcadAssemblyUtils.GetAppObjectSafely(currentTypes);
            // currentAppObject.Initialize()
        }



















        public new void Log(string message)
        {
            var mode = GetLogMode();
            switch (mode.Equals(LogMode.Off))
            {
                case object _ when mode.Equals(LogMode.Text):
                    {
                        LogToTextFile(message);
                        break;
                    }
                case object _ when mode.Equals(LogMode.AcadDocEditor):
                    {
                        LogToTextFile(message);
                        LogToEditor(message);
                        break;
                    }
            }
        }

        private new void LogToTextFile(string message)
        {
            if (ReloaderLog is null)
            {
                ReloaderLog = new ReloaderLog();
                ReloaderLog.LogDir = _tempFolder;
                ReloaderLog.Write(message);
            }
            else
            {
                ReloaderLog.LogDir = _tempFolder;
                ReloaderLog.Write(message);
            }
        }

        private void LogToEditor(string message)
        {
            if (_document is not null)
            {
                _document.Editor.WriteMessage(Environment.NewLine + message);
                _document.Editor.WriteMessage(Environment.NewLine);
            }
        }

        public new void LogException(Autodesk.AutoCAD.Runtime.Exception ex)
        {
            var mode = GetLogMode();
            switch (mode.Equals(LogMode.Off))
            {
                case object _ when mode.Equals(LogMode.Text):
                    {
                        LogExceptionToTextFile(ex);
                        break;
                    }
                case object _ when mode.Equals(LogMode.AcadDocEditor):
                    {
                        LogExceptionToTextFile(ex);
                        LogExceptionToEditor(ex);
                        break;
                    }
            }
        }

        private new void LogExceptionToTextFile(Autodesk.AutoCAD.Runtime.Exception ex)
        {
            if (ReloaderLog is null)
            {
                ReloaderLog = new ReloaderLog();
                ReloaderLog.LogDir = _tempFolder;
                ReloaderLog.Exception(ex);
            }
            else
            {
                ReloaderLog.LogDir = _tempFolder;
                ReloaderLog.Exception(ex);
            }
        }

        private void LogExceptionToEditor(Autodesk.AutoCAD.Runtime.Exception ex)
        {
            if (_document is null)
            {
                _document = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            }

            var ed = _document.Editor;
            var list = NetUtils.Exceptions.GetPrettyStringList(ex);
            foreach (string str in list)
                ed.WriteMessage(Environment.NewLine.ToString() + str);

        }




    }

}