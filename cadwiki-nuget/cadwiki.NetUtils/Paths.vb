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

End Class
