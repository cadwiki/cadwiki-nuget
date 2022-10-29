Imports System.IO
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports cadwiki.NetUtils

Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim asm As Assembly = Assembly.GetExecutingAssembly()
        Dim assemblyName = asm.GetName()
        Dim version As Version = assemblyName.Version
        Dim versionString As String = version.ToString()
        LabelCurrentVersion.Text = versionString
        Dim newVersion As Version = version
        newVersion = newVersion.IncrementBuild
        Dim time As DateTime = DateTime.Now
        Dim format As String = "HHmm"
        Dim timeStamp As String = time.ToString(format)
        newVersion = newVersion.SetVersion(newVersion.Major, newVersion.Minor, newVersion.Build, CInt(timeStamp))
        TextBoxNewVersion.Text = newVersion.ToString()

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim asm As Assembly = Assembly.GetExecutingAssembly()
        Dim dllPath As String = asm.Location
        Dim dllFolder As String = Path.GetDirectoryName(dllPath)
        Dim root As String = Paths.TryGetSolutionDirectoryPath(dllFolder)
        Dim repoFolder As String = New DirectoryInfo(root).Parent.FullName
        Dim folders As List(Of String) =
            System.IO.Directory.GetDirectories(repoFolder, "*", System.IO.SearchOption.AllDirectories).ToList()
        folders.Reverse()
        folders.Add(repoFolder)
        folders.Reverse()
        Dim projectsToUpdate As New List(Of String) From {
            "*cadwiki-nuget",
            "*CadDevToolsDriver*",
            "*cadwiki.AdminUtils*",
            "*cadwiki.CadDevTools*",
            "*cadwiki.DllReloader*",
            "*cadwiki.FileStore*",
            "*cadwiki.NetUtils*",
            "*cadwiki.NUnitTestRunner*",
            "*cadwiki.WpfUi*"
        }
        Dim wildCardPatterns As New List(Of String) From {
             "*apikeys.txt",
            "*README.nuget.commands.md",
            "*AssemblyInfo.vb",
            "*.nuspec",
            "*.targets"
        }
        For Each folder In folders
            Dim folderName As String = Path.GetFileName(folder)
            Dim isThisAProjectToUpdate As Boolean = False
            For Each project In projectsToUpdate
                If StringContains(project, folder) Then
                    isThisAProjectToUpdate = True
                    Exit For
                End If
            Next

            If (isThisAProjectToUpdate) Then
                For Each fileName In Directory.GetFiles(folder)
                    For Each wildCardPattern In wildCardPatterns
                        If StringContains(wildCardPattern, fileName) Then
                            ReplaceText(LabelCurrentVersion.Text,
                                        TextBoxNewVersion.Text, fileName)
                        End If
                    Next
                Next
            End If

        Next

    End Sub

    Private Sub ReplaceText(oldText As String, newText As String, fileName As String)
        Dim lines As List(Of String) = IO.File.ReadAllLines(fileName).ToList
        For index As Integer = 0 To lines.Count - 1
            If lines(index).Contains(oldText) Then
                lines(index) = lines(index).Replace(oldText, newText)
            End If
        Next
        IO.File.WriteAllLines(fileName, lines.ToArray)
    End Sub

    Public Function StringContains(ByVal searchPattern As String,
                             ByVal inputText As String) As Boolean
        Dim regexText As String = WildcardToRegex(searchPattern)
        Dim regex As Regex = New Regex(regexText, RegexOptions.IgnoreCase)

        If regex.IsMatch(inputText) Then
            Return True
        End If
        Return False
    End Function

    Public Shared Function WildcardToRegex(ByVal pattern As String) As String
        Return "^" & Regex.Escape(pattern).Replace("\*", ".*").Replace("\?", ".") & "$"
    End Function

End Class
