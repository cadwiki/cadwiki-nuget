Option Strict On
Option Infer Off
Option Explicit On
Imports System.IO

Public Class Paths

    Public Shared Function GetNewestDllInVsubfoldersOfSolutionDirectory(solutionDir As String, wildCardFileName As String) As String
        Dim dlls As List(Of String) = Paths.GetAllWildcardFilesInVSubfolder(solutionDir, wildCardFileName)
        Dim dll As String = dlls.FirstOrDefault
        Return dll
    End Function

    Public Shared Function GetAllWildcardFilesInAnySubfolder(directoryPath As String, wildCardFileName As String) As List(Of String)
        Dim cadApps As List(Of String) = Directory.GetFiles(directoryPath, wildCardFileName, SearchOption.AllDirectories).
                                                    OrderByDescending(Function(f) New FileInfo(f).LastWriteTime).ToList
        Return cadApps
    End Function

    Public Shared Function GetAllWildcardFilesInVSubfolder(directoryPath As String, wildCardFileName As String) As List(Of String)
        Dim cadApps As List(Of String) = Directory.GetFiles(directoryPath, wildCardFileName, SearchOption.AllDirectories).
                                                    OrderByDescending(Function(f) New FileInfo(f).LastWriteTime).ToList
        Dim numRemoved As Integer = cadApps.RemoveAll(AddressOf cadwiki.NetUtils.Strings.NotContainsBackSlashVInSubFolder)
        Return cadApps
    End Function

    Public Shared Function TryGetSolutionDirectoryPath(ByVal Optional currentPath As String = Nothing) As String

        Dim currentDirectory As String = System.IO.Directory.GetCurrentDirectory()
        If String.IsNullOrEmpty(currentDirectory) Then
            Throw New Exception("Current directory failed.")
        End If
        Dim directoryInfo As DirectoryInfo = New DirectoryInfo(If(currentPath, currentDirectory))
        While directoryInfo IsNot Nothing AndAlso Not directoryInfo.GetFiles("*.sln").Any()
            directoryInfo = directoryInfo.Parent
        End While
        Return directoryInfo.FullName
    End Function

    Public Shared Function ReplaceAllillegalCharsForWindowsOSInFileName(fileName As String, newChar As String) As String
        fileName.Replace("<", newChar)
        fileName.Replace(">", newChar)
        fileName.Replace(":", newChar)
        fileName.Replace("""", newChar)
        fileName.Replace("/", newChar)
        fileName.Replace("\\", newChar)
        fileName.Replace("|", newChar)
        fileName.Replace("?", newChar)
        fileName.Replace("*", newChar)
        Return fileName
    End Function

    Public Shared Function GetUniqueFilePath(filePath As String) As String
        Dim doesFileExist As Boolean = File.Exists(filePath)
        If doesFileExist Then
            Dim time As DateTime = DateTime.Now()
            Dim format As String = "yyyy__MM__dd____HH_mm_ss"
            Dim timeStamp As String = DateTime.Now.ToString(format)
            Dim fileDirectory As String = Path.GetDirectoryName(filePath)
            Dim fileNameNoExt As String = Path.GetFileNameWithoutExtension(filePath)
            Dim fileExt As String = Path.GetExtension(filePath)
            filePath = fileDirectory + "\" + fileNameNoExt + "__" + timeStamp + fileExt
        End If
        Return filePath
    End Function

    Public Shared Function GetTempFile(baseFileName As String) As String
        Dim tempFolder As String = Path.GetTempPath()
        Dim tempFile As String = tempFolder + baseFileName
        Return GetUniqueFilePath(tempFile)
    End Function
End Class
