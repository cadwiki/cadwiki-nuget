using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cadwiki.NetUtils
{

    public class Paths
    {

        public static string GetNewestDllInAnySubfolderOfSolutionDirectory(string solutionDir, string wildCardFileName)
        {
            var dlls = GetAllWildcardFilesInAnySubfolder(solutionDir, wildCardFileName);
            string dll = dlls.FirstOrDefault();
            return dll;
        }

        public static string GetNewestDllInVsubfoldersOfSolutionDirectory(string solutionDir, string wildCardFileName)
        {
            var dlls = GetAllWildcardFilesInVSubfolder(solutionDir, wildCardFileName);
            string dll = dlls.FirstOrDefault();
            return dll;
        }

        public static List<string> GetAllWildcardFilesInAnySubfolder(string directoryPath, string wildCardFileName)
        {
            var cadApps = Directory.GetFiles(directoryPath, wildCardFileName, SearchOption.AllDirectories).OrderByDescending(f => new FileInfo(f).LastWriteTime).ToList();
            return cadApps;
        }

        public static List<string> GetAllWildcardFilesInVSubfolder(string directoryPath, string wildCardFileName)
        {
            var cadApps = Directory.GetFiles(directoryPath, wildCardFileName, SearchOption.AllDirectories).OrderByDescending(f => new FileInfo(f).LastWriteTime).ToList();
            int numRemoved = cadApps.RemoveAll(Strings.NotContainsBackSlashVInSubFolder);
            return cadApps;
        }

        public static string TryGetSolutionDirectoryPath(string currentPath = null)
        {

            string currentDirectory = Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(currentDirectory))
            {
                throw new Exception("Current directory failed.");
            }
            var directoryInfo = new DirectoryInfo(currentPath ?? currentDirectory);
            while (directoryInfo is not null && !directoryInfo.GetFiles("*.sln").Any())
                directoryInfo = directoryInfo.Parent;
            return directoryInfo.FullName;
        }

        public static string ReplaceAllillegalCharsForWindowsOSInFileName(string fileName, string newChar)
        {
            fileName.Replace("<", newChar);
            fileName.Replace(">", newChar);
            fileName.Replace(":", newChar);
            fileName.Replace("\"", newChar);
            fileName.Replace("/", newChar);
            fileName.Replace(@"\\", newChar);
            fileName.Replace("|", newChar);
            fileName.Replace("?", newChar);
            fileName.Replace("*", newChar);
            return fileName;
        }

        public static string GetUniqueFilePath(string filePath)
        {
            bool doesFileExist = File.Exists(filePath);
            if (doesFileExist)
            {
                var time = DateTime.Now;
                string format = "yyyy__MM__dd____HH_mm_ss";
                string timeStamp = DateTime.Now.ToString(format);
                string fileDirectory = Path.GetDirectoryName(filePath);
                string fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
                string fileExt = Path.GetExtension(filePath);
                filePath = fileDirectory + @"\" + fileNameNoExt + "__" + timeStamp + fileExt;
            }
            return filePath;
        }

        public static string GetTempFile(string baseFileName)
        {
            string tempFolder = Path.GetTempPath();
            string tempFile = tempFolder + baseFileName;
            return GetUniqueFilePath(tempFile);
        }
    }
}