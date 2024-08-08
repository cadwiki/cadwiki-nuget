using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace AutoCADAddin
{
    public class AddinApp : IExtensionApplication
    {

        public void Initialize()
        {
            try
            {
                var dll = Assembly.GetExecutingAssembly();
            }
            catch (Exception ex)
            {
                ErrorHandler.Show(ex);
            }

        }

        public void Terminate()
        {

        }

        public static string GetLogsFolder()
        {
            var dllPath = Assembly.GetExecutingAssembly().Location;
            var dllName = Assembly.GetExecutingAssembly().GetName().Name;
            var tempFolder = System.IO.Path.GetTempPath() + "\\" + dllName  + "\\logs";
            
            if (dllPath != null)
            {
                var dllFolder = System.IO.Path.GetDirectoryName(dllPath);
                var logs = dllFolder + "\\" + "\\logs";
                if (!System.IO.Directory.Exists(logs))
                {
                    System.IO.Directory.CreateDirectory(logs);
                }
                return logs;
            }
            else
            {
                System.IO.Directory.CreateDirectory(tempFolder);
            }
            

            return tempFolder;
        }
    }
}
