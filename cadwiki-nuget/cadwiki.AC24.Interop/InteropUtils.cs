using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.Interop.Common;

namespace cadwiki.AC24.Interop
{

    public class InteropUtils
    {
        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hwnd, ref IntPtr lpdwProcessId);

        private readonly string AutoCADProgId = "AutoCAD.Application.R24.3";
        private AcadApplication App;


        public bool IsAutoCADRunning()
        {
            var appAcad = GetRunningAutoCADInstance();
            if (appAcad is null)
            {
                return false;
            }
            return true;
        }

        public bool ConfigureRunningAutoCADForUsage()
        {
            if (App is null)
            {
                return false;
            }
            NetUtils.MessageFilter.Register();
            // SetAutoCADWindowToNormal()
            return true;
        }

        private string autocad2021ProgId = "AutoCAD.Application.24";

        public bool StartAutoCADApp(ProcessStartInfo autocadProcessInfo)
        {
            var pr = Process.Start(autocadProcessInfo);
            pr.WaitForInputIdle();
            AcadApplication appAcad = null;
            while (appAcad is null)
            {
                try
                {
                    appAcad = (AcadApplication)Marshal.GetActiveObject(autocad2021ProgId);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                    System.Threading.Thread.Sleep(1000);
                }
            }
            App = appAcad;
            NetUtils.MessageFilter.Register();
            // SetAutoCADWindowToNormal()
            return true;
        }

        public bool NetloadDll(string dllPath)
        {
            if (!System.IO.File.Exists(dllPath))
            {
                throw new Exception("Dll does not exist: " + dllPath);
            }
            var doc = App.ActiveDocument;
            if (doc is null)
            {
                doc = App.Documents.Add();
            }
            doc.SendCommand("(setvar \"secureload\" 0)" + Environment.NewLine);
            dllPath = dllPath.Replace(@"\", @"\\");
            doc.SendCommand("(command \"_netload\" \"" + dllPath + "\")" + Environment.NewLine);
            return true;
        }

        public bool OpenDrawingTemplate(string dwtFilePath, bool readOnlyMode)
        {
            if (App is null)
            {
                return false;
            }
            var doc = App.Documents.Open(dwtFilePath, readOnlyMode);
            if (doc != null)
            {
                return true;
            }
            return false;
        }

        public bool LoadCuiFile(string cuiMenuFileName, string menuGroupName)
        {
            if (App is null)
            {
                return false;
            }

            var mainCui = App.ActiveDocument.GetVariable("MENUNAME");
            string cuiFile = (string)(mainCui) + ".cui";

            var menuGroups = App.MenuGroups;
            var menuGroup = GetExistingMenuGroup(menuGroups, menuGroupName);

            if (menuGroup is null)
            {
                menuGroup = App.MenuGroups.Load(cuiMenuFileName);
                App.ActiveDocument.SendCommand("(command \"+RIBBON\" \"LOADER.RBNU_230_F2E64\")" + Environment.NewLine);
                return true;
            }
            else
            {
                return false;
            }
            return default;
        }

        public bool DoesMenuGroupExist(string groupName)
        {
            if (App is null)
            {
                return false;
            }
            var menuGroups = App.MenuGroups;
            int i = 0;
            AcadMenuGroup currentGroup;
            while (i < menuGroups.Count)
            {
                currentGroup = menuGroups.Item(i);
                if (currentGroup.Name.Equals(groupName))
                {
                    return true;
                }
                i += 1;
            }
            return false;
        }

        public bool CreateProfile(string profileName)
        {
            if (App is null)
            {
                return false;
            }
            bool doesThisProfileExist = DoesProfileExist(App, profileName);
            if (doesThisProfileExist)
            {
                SetProfileActive(App, profileName);
                AddTempToPreferencePaths(App);
            }
            else
            {
                CreateProfile(App, profileName);
                AddTempToPreferencePaths(App);
            }
            SetProfileActive(App, profileName);
            return true;
        }


        public bool SetAutoCADWindowToNormal()
        {
            if (App is null)
            {
                return false;
            }
            App.WindowState = AcWindowState.acNorm;
            return true;
        }


        private AcadApplication GetRunningAutoCADInstance()
        {
            AcadApplication appAcad = null;
            try
            {
                appAcad = (AcadApplication)Marshal.GetActiveObject(autocad2021ProgId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
            App = appAcad;
            return appAcad;
        }

        private AcadMenuGroup GetExistingMenuGroup(AcadMenuGroups menuGroups, string groupName)
        {

            int i = 0;
            AcadMenuGroup currentGroup;
            while (i < menuGroups.Count)
            {
                currentGroup = menuGroups.Item(i);
                if (currentGroup.Name.Equals(groupName))
                {
                    return currentGroup;
                }
                i += 1;
            }
            return null;
        }

        private AcadPopupMenu GetExistingPopupMenu(AcadPopupMenus menuGroup, string menuName)
        {
            int i = 0;
            AcadPopupMenu currentMenu;
            while (i < menuGroup.Count)
            {
                currentMenu = menuGroup.Item(i);
                if (currentMenu.Name.Equals(menuName))
                {
                    return currentMenu;
                }
                i += 1;
            }
            return null;
        }


        private void SetProfileActive(AcadApplication appAcad, string profileName)
        {
            var profiles = appAcad.Preferences.Profiles;
            profiles.ActiveProfile = profileName;
        }

        private void CreateProfile(AcadApplication appAcad, string profileName)
        {
            var profiles = appAcad.Preferences.Profiles;
            profiles.CopyProfile(profiles.ActiveProfile, profileName);
            profiles.ActiveProfile = profileName;
        }

        private bool DoesProfileExist(AcadApplication appAcad, string profileName)
        {
            var profiles = appAcad.Preferences.Profiles;
            object pNames = null;
            profiles.GetAllProfileNames(out pNames);
            string[] profileNames = (string[])pNames;
            foreach (string name in profileNames)
            {
                if (name.Equals(profileName))
                {
                    return true;
                }
            }
            return false;
        }

        private void AddTempToPreferencePaths(AcadApplication appAcad)
        {
            string trustedPathsString = (string)appAcad.ActiveDocument.GetVariable("TRUSTEDPATHS");
            string tempDirectory = System.IO.Path.GetTempPath();
            string Directory = tempDirectory + "";
            var newPaths = new List<string>() { Directory };

            if (!trustedPathsString.Contains(Directory))
            {
                AddTrustedPaths(appAcad, newPaths);
            }
        }

        private void AddTrustedPaths(AcadApplication appAcad, List<string> newPaths)
        {
            string trustedPathsString = (string)appAcad.ActiveDocument.GetVariable("TRUSTEDPATHS");
            var oldPaths = new List<string>();
            oldPaths = trustedPathsString.Split(';').ToList();
            string newTrustedPathsString = trustedPathsString;
            foreach (string newPath in newPaths)
            {
                bool pathAlreadyExists = trustedPathsString.Contains(newPath);
                if (!pathAlreadyExists)
                {
                    newTrustedPathsString = newPath + ";" + newTrustedPathsString;
                }
            }
            appAcad.ActiveDocument.SetVariable("TRUSTEDPATHS", newTrustedPathsString);
        }
    }
}