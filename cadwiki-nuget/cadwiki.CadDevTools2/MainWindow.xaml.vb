Imports System.IO
Imports cadwiki.NetUtils
Imports cadwiki.AutoCAD2021.Interop.Utilities
Imports System.Windows
Imports System.Windows.Documents

Class MainWindow
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ReadCadDevToolsIniFromTemp()
        If Not previousAutoCADLocationValue.Equals(noneValue) And File.Exists(previousAutoCADLocationValue) Then
            ButtonLoadNewest.IsEnabled = True
            ButtonSelectLoad.IsEnabled = True
            EditRichTextBoxWithAutoCADLocation()
        Else
            ButtonLoadNewest.IsEnabled = False
            ButtonSelectLoad.IsEnabled = False
        End If
    End Sub

    Private iniFileName As String = "cadwiki.CadDevToolsSettings.ini"
    Private iniSubFolder As String = "cadwiki.CadDevTools"
    Private noneValue As String = "(none)"
    Private previousAutoCADLocationKey As String = "PREVIOUS-AUTOCAD-LOCATION"
    Private previousAutoCADLocationValue As String = noneValue

    Private Sub ReadCadDevToolsIniFromTemp()
        Dim iniFilePath As String = GetCadDevToolsIniFilePath()
        Dim objIniFile As New IniFile(iniFilePath)
        previousAutoCADLocationValue = objIniFile.GetString("Settings", previousAutoCADLocationKey, noneValue)
        acadLocation = previousAutoCADLocationValue
    End Sub

    Private Function GetCadDevToolsIniFilePath() As String
        Dim windowTempFolder As String = Path.GetTempPath()
        Dim iniFolder As String = windowTempFolder + "\" + iniSubFolder
        If Not Directory.Exists(iniFolder) Then
            Directory.CreateDirectory(iniFolder)
        End If
        Dim iniFilePath As String = iniFolder + "\" + iniFileName
        Return iniFilePath
    End Function

    Private ReadOnly acadLocations As New List(Of String) From {
        "C:\Program Files\Autodesk\AutoCAD 2021\acad.exe",
        "C:\Program Files\Autodesk\AutoCAD 2022\acad.exe",
        "E:\Program Files\Autodesk\AutoCAD 2021\acad.exe",
        "E:\Program Files\Autodesk\AutoCAD 2022\acad.exe"
        }

    Private acadLocation As String = ""

    Private Sub ButtonSelectAutoCADYear_Click(sender As Object, e As Windows.RoutedEventArgs)
        Dim window As cadwiki.WpfUi.WindowGetFilePath = New cadwiki.WpfUi.WindowGetFilePath(acadLocations)
        window.ShowDialog()
        Dim wasOkClicked As Boolean = window.WasOkayClicked
        If wasOkClicked Then
            acadLocation = window.SelectedFolder
            If File.Exists(acadLocation) Then
                ButtonLoadNewest.IsEnabled = True
                ButtonSelectLoad.IsEnabled = True
                EditRichTextBoxWithAutoCADLocation()
                Dim iniFilePath As String = GetCadDevToolsIniFilePath()
                Dim objIniFile As New IniFile(iniFilePath)
                objIniFile.WriteString("Settings", previousAutoCADLocationKey, acadLocation)
            Else
                cadwiki.WpfUi.Utils.SetErrorStatus(TextBlockStatus,
                    TextBlockMessage,
                    "Unable to start AutoCAD because file does not exist: " + acadLocation)
            End If

        Else
            cadwiki.WpfUi.Utils.SetErrorStatus(TextBlockStatus,
                TextBlockMessage,
                "Unable to start AutoCAD because File selection window was canceled.")
        End If
    End Sub

    Private Sub EditRichTextBoxWithAutoCADLocation()
        Dim flowDoc As FlowDocument = New FlowDocument()
        Dim paragraph1 As New Paragraph()
        paragraph1.Inlines.Add(New Run("Selected program: "))
        paragraph1.Inlines.Add(New Bold(New Run(acadLocation)))
        flowDoc.Blocks.Add(paragraph1)
        Dim paragraph2 As New Paragraph()
        paragraph2.Inlines.Add(New Run("You can now use the other buttons."))
        flowDoc.Blocks.Add(paragraph2)
        RichTextBoxSelectedAutoCAD.Document = flowDoc
    End Sub

    Private Sub ButtonLoadNewest_Click(sender As Object, e As RoutedEventArgs)
        Forms.Application.DoEvents()
        cadwiki.WpfUi.Utils.SetProcessingStatus(TextBlockStatus,
            TextBlockMessage,
            "Please wait until CAD launches and netloads the most recently built CadApp.dll in your solution directory.")
        Dim solutionDir As String = GetSolutionDirectory()
        Dim wildCardFileName As String = "*MainApp.dll"
        Dim cadApps As List(Of String) = Paths.GetAllWildcardFilesInVSubfolder(solutionDir, wildCardFileName)
        Dim cadAppDll As String = cadApps.FirstOrDefault
        If Not File.Exists(cadAppDll) Then
            Throw New Exception("Dll does not exist, try building or rebuilding the CadApp.")
        End If
        NetLoadDll(cadAppDll)
    End Sub

    Private Function GetSolutionDirectory() As String
        Dim folder As String = Directory.GetCurrentDirectory
        Dim parent As String = Path.GetDirectoryName(folder)
        parent = Path.GetDirectoryName(parent)
        parent = Path.GetDirectoryName(parent)
        parent = Path.GetDirectoryName(parent)
        Return parent
    End Function

    Private Sub ButtonSelectLoad_Click(sender As Object, e As RoutedEventArgs)
        Forms.Application.DoEvents()
        cadwiki.WpfUi.Utils.SetProcessingStatus(TextBlockStatus,
            TextBlockMessage,
            "Please wait until CAD launches and netloads your selected dll into AutoCAD.")
        Dim folder As String = Directory.GetCurrentDirectory
        Dim solutionDir As String = GetSolutionDirectory()
        Dim wildCardFileName As String = "*MainApp.dll"
        Dim cadApps As List(Of String) = Paths.GetAllWildcardFilesInVSubfolder(solutionDir, wildCardFileName)

        Dim window As cadwiki.WpfUi.WindowGetFilePath = New cadwiki.WpfUi.WindowGetFilePath(cadApps)
        window.Width = 1200
        window.Height = 300
        window.ShowDialog()
        Dim wasOkClicked As Boolean = window.WasOkayClicked
        If wasOkClicked Then
            Dim filePath As String = window.SelectedFolder
            NetLoadDll(filePath)
        Else
            cadwiki.WpfUi.Utils.SetErrorStatus(TextBlockStatus, TextBlockMessage, "User closed dll load menu.")
        End If
    End Sub


    Private Sub NetLoadDll(cadAppDll As String)

        cadwiki.WpfUi.Utils.SetProcessingStatus(TextBlockStatus,
            TextBlockMessage,
            "Please wait until CAD launches netloads the dll.")
        If acadLocation.Contains("2021") Then
            Dim interop As InteropUtils = New InteropUtils()
            Dim isAutoCADRunning As Boolean = interop.IsAutoCADRunning()
            If isAutoCADRunning = False Then
                System.Windows.Forms.Application.DoEvents()
                Dim processInfo As ProcessStartInfo = New ProcessStartInfo With {
                    .FileName = acadLocation
                    }
                interop.StartAutoCADApp(processInfo)
            End If
            interop.ConfigureRunningAutoCADForUsage()
            'interop.OpenDrawingTemplate(dwtFilePath, True)
            interop.NetloadDll(cadAppDll)
        Else
            cadwiki.WpfUi.Utils.SetErrorStatus(TextBlockStatus,
                TextBlockMessage,
                "Invalid AutoCAD location: " + acadLocation)
        End If

        cadwiki.WpfUi.Utils.SetSuccessStatus(TextBlockStatus, TextBlockMessage, "Dll netload complete: " + cadAppDll)
        Forms.Application.DoEvents()
    End Sub


    Private Sub ButtonOk_Click(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub
End Class
