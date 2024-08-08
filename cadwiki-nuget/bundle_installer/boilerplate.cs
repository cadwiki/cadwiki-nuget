       [CommandMethod("SEND_LOGS", CommandFlags.Session)]
       public static void SEND_LOGS()
       {
           try
           {
               var version = Assembly.GetExecutingAssembly().GetName().Version;
               string timeStamp = DateTime.Now.ToString("yyyy_MM_dd__hh_mm_ss");

               var emailTo = "";
               var emailCC = "";
               var emailSubject = "Revamp AutoCAD Plugin Logs -- v" + version;

               string zipFileName = ZipUpLogFolders(version, timeStamp);
               OpenOutlookWithZipAttached(emailTo, emailCC, emailSubject, zipFileName);

           }
           catch (Exception ex)
           {
               Net.Utilities.Globals.PluginLog.Exception(ex);
           }
       }

       private static void OpenOutlookWithZipAttached(string emailTo, string emailCC, string emailSubject, string zipFileName)
       {
           string zipName = Path.GetDirectoryName(zipFileName);

           var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
           var msg = "";
           msg = "Zip file created: " + zipFileName;
           doc.Editor.WriteMessage(Environment.NewLine + msg);
           Net.Utilities.Globals.PluginLog.Write(msg);

           Microsoft.Office.Interop.Outlook.Application app = new
                                     Microsoft.Office.Interop.Outlook.Application();
           MailItem item = app.CreateItem((OlItemType.olMailItem));
           item.BodyFormat = OlBodyFormat.olFormatHTML;
           item.To = emailTo;
           item.CC = emailCC;
           item.Subject = emailSubject;
           //item.Attachments.Add(folderPath, OlAttachmentType.olByValue, 1, "AttachedFolder");
           item.Attachments.Add(zipFileName, OlAttachmentType.olByValue, 1, zipName);
           item.Display();
       }

       private static string ZipUpLogFolders(Version version, string timeStamp)
       {
           var zipFolderName = "AutoCAD_logs_v" + version + "__" + timeStamp;
           var copyFolder = Path.GetTempPath() + zipFolderName;
           Directory.CreateDirectory(copyFolder);

           //https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/Error-Reporting.html
           var autocadCerPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Autodesk\\CER";
           try
           {
               CopyFolder(autocadCerPath, copyFolder);
           }
           catch (Exception ex)
           {
               Net.Utilities.Globals.PluginLog.Exception(ex);
           }
           try
           {
               CopyFolder(Globals.CDriveRevampFolder, copyFolder);
           }
           catch (Exception ex)
           {
               Net.Utilities.Globals.PluginLog.Exception(ex);
           }

           var zipFileName = copyFolder + ".zip";
           ZipFile.CreateFromDirectory(copyFolder, zipFileName);
           return zipFileName;
       }

       static void CopyFolder(string sourceFolder, string destinationFolder)
       {
           DirectoryInfo sourceDir = new DirectoryInfo(sourceFolder);
           DirectoryInfo[] sourceSubDirs = sourceDir.GetDirectories();

           // Recursively copy subfolders and their contents
           foreach (DirectoryInfo subDir in sourceSubDirs)
           {
               string subDirDestinationPath = Path.Combine(destinationFolder, subDir.Name);
               Directory.CreateDirectory(subDirDestinationPath);
               CopyFolder(subDir.FullName, subDirDestinationPath);
           }

           // Copy files in the source folder to the destination folder
           foreach (FileInfo file in sourceDir.GetFiles())
           {
               string destinationFilePath = Path.Combine(destinationFolder, file.Name);
               file.CopyTo(destinationFilePath, true);
           }


       }