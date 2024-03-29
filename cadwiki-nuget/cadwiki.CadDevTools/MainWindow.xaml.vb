﻿Imports System.IO
Imports cadwiki.NetUtils

Imports System.Windows
Imports System.Windows.Documents
Imports InteropUtils2021 = cadwiki.AutoCAD2021.Interop.Utilities.InteropUtils
Imports InteropUtils2022 = cadwiki.AutoCAD2021.Interop.Utilities.InteropUtils
Imports System.Windows.Media.Imaging

Class MainWindow

    Public Class Dependencies
        Public AutoCADExePath As String
        Public AutoCADStartupSwitches As String
        Public DllFilePathToNetload As String
        Public CustomDirectoryToSearchForDllsToLoadFrom As String
        Public DllWildCardSearchPattern As String
        Public SetAutocadWindowToNorm As Boolean
    End Class

    Private _dependencies As Dependencies

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        StandardOnStartOperations()
        TextBoxStartupSwitches.Text = ""
    End Sub

    Public Sub New(dependencies As Dependencies)
        ' This call is required by the designer.
        InitializeComponent()
        StandardOnStartOperations()

        If Not String.IsNullOrEmpty(dependencies.AutoCADExePath) Then
            acadLocation = dependencies.AutoCADExePath
        End If

        If Not String.IsNullOrEmpty(dependencies.DllFilePathToNetload) Then
            TextBoxDllPath.Text = dependencies.DllFilePathToNetload
        End If

        If Not String.IsNullOrEmpty(dependencies.AutoCADStartupSwitches) Then
            TextBoxStartupSwitches.Text = dependencies.AutoCADStartupSwitches
        End If

        _dependencies = dependencies

    End Sub

    Private Sub StandardOnStartOperations()
        Dim autocadReloader As New DllReloader.AutoCAD.AutoCADAppDomainDllReloader()
        autocadReloader.ClearIni()
        ReadCadDevToolsIniFromTemp()
        Dim bitMap As Drawing.Bitmap = FileStore.My.Resources.ResourceIcons._500x500_cadwiki_v1
        Dim bitMapImage As BitmapImage = Bitmaps.BitMapToBitmapImage(bitMap)
        Me.Icon = bitMapImage
        EnableOrDisableControlsOnStart(previousAutoCADLocationValue)
    End Sub


    Private Sub EnableOrDisableControlsOnStart(acadLocation As String)
        If Not acadLocation.Equals(noneValue) And File.Exists(acadLocation) Then
            ButtonLaunch.IsEnabled = True
            EditRichTextBoxWithAutoCADLocation()
        Else
            ButtonLaunch.IsEnabled = False
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
        Dim window As WpfUi.WindowGetFilePath = New WpfUi.WindowGetFilePath(acadLocations)
        window.ShowDialog()
        Dim wasOkClicked As Boolean = window.WasOkayClicked
        If wasOkClicked Then
            acadLocation = window.SelectedFolder
            If File.Exists(acadLocation) Then
                ButtonLaunch.IsEnabled = True
                EditRichTextBoxWithAutoCADLocation()
                Dim iniFilePath As String = GetCadDevToolsIniFilePath()
                Dim objIniFile As New IniFile(iniFilePath)
                objIniFile.WriteString("Settings", previousAutoCADLocationKey, acadLocation)
                WpfUi.Utils.SetSuccessStatus(TextBlockStatus,
                    TextBlockMessage,
                    "File does exist: " + acadLocation)
            Else
                WpfUi.Utils.SetErrorStatus(TextBlockStatus,
                    TextBlockMessage,
                    "File does not exist: " + acadLocation)
            End If

        Else
            WpfUi.Utils.SetErrorStatus(TextBlockStatus,
                TextBlockMessage,
                "File selection window was canceled.")
        End If
    End Sub

    Private Sub EditRichTextBoxWithAutoCADLocation()
        Dim flowDoc As FlowDocument = New FlowDocument()
        Dim paragraph1 As New Paragraph()
        paragraph1.Inlines.Add(New Run("Selected program: " + Environment.NewLine))
        paragraph1.Inlines.Add(New Bold(New Run(acadLocation)))
        paragraph1.Inlines.Add(New Run(Environment.NewLine + "You can now use the other buttons."))
        flowDoc.Blocks.Add(paragraph1)
        RichTextBoxSelectedAutoCAD.Document = flowDoc
    End Sub


    Private Sub ButtonLaunch_Click(sender As Object, e As RoutedEventArgs)
        Forms.Application.DoEvents()
        WpfUi.Utils.SetProcessingStatus(TextBlockStatus,
            TextBlockMessage,
            "Please wait until CAD launches and netloads " + TextBoxDllPath.Text + " into AutoCAD.")

        Dim filePath As String = TextBoxDllPath.Text
        If Not File.Exists(filePath) Then
            LaunchAutocad()
        Else
            LaunchAutocad()
            NetloadDll(filePath)
        End If
    End Sub

    Private Sub LaunchAutocad()

        WpfUi.Utils.SetProcessingStatus(TextBlockStatus,
            TextBlockMessage,
            "Please wait until CAD launches.")
        If acadLocation.Contains("2021") Then
            Dim interop2021 As InteropUtils2021 = New InteropUtils2021()
            Dim isAutoCADRunning As Boolean = interop2021.IsAutoCADRunning()
            If isAutoCADRunning = False Then
                System.Windows.Forms.Application.DoEvents()
                Dim processInfo As ProcessStartInfo = New ProcessStartInfo With {
                    .FileName = acadLocation,
                    .Arguments = TextBoxStartupSwitches.Text
                }

                interop2021.StartAutoCADApp(processInfo)
            End If
            interop2021.ConfigureRunningAutoCADForUsage()
            If _dependencies.SetAutocadWindowToNorm Then
                interop2021.SetAutoCADWindowToNormal()
            End If
            'interop.OpenDrawingTemplate(dwtFilePath, True)
        ElseIf acadLocation.Contains("2022") Then
            Dim interop2022 As InteropUtils2022 = New InteropUtils2022()
            Dim isAutoCADRunning As Boolean = interop2022.IsAutoCADRunning()
            If isAutoCADRunning = False Then
                System.Windows.Forms.Application.DoEvents()
                Dim processInfo As ProcessStartInfo = New ProcessStartInfo With {
                    .FileName = acadLocation,
                    .Arguments = TextBoxStartupSwitches.Text
                    }
                interop2022.StartAutoCADApp(processInfo)
            End If
            interop2022.ConfigureRunningAutoCADForUsage()
            If _dependencies.SetAutocadWindowToNorm Then
                interop2022.SetAutoCADWindowToNormal()
            End If
            'interop.OpenDrawingTemplate(dwtFilePath, True)
        Else
            WpfUi.Utils.SetErrorStatus(TextBlockStatus,
                TextBlockMessage,
                "Invalid AutoCAD location: " + acadLocation)
        End If

        WpfUi.Utils.SetSuccessStatus(TextBlockStatus, TextBlockMessage, "Autocad Launch complete.")
        Forms.Application.DoEvents()
    End Sub

    Private Sub NetloadDll(cadAppDll As String)
        WpfUi.Utils.SetProcessingStatus(TextBlockStatus,
            TextBlockMessage,
            "Please wait until CAD launches netloads the" + cadAppDll + " dll.")
        If acadLocation.Contains("2021") Then
            Dim interop2021 As InteropUtils2021 = New InteropUtils2021()
            Dim isAutoCADRunning As Boolean = interop2021.IsAutoCADRunning()
            If isAutoCADRunning = False Then
            End If
            interop2021.NetloadDll(cadAppDll)
        ElseIf acadLocation.Contains("2022") Then
            Dim interop2022 As InteropUtils2022 = New InteropUtils2022()
            Dim isAutoCADRunning As Boolean = interop2022.IsAutoCADRunning()
            If isAutoCADRunning = False Then
            End If
            interop2022.NetloadDll(cadAppDll)
        End If

        WpfUi.Utils.SetSuccessStatus(TextBlockStatus, TextBlockMessage, "Dll netload complete: " + cadAppDll)
        Forms.Application.DoEvents()
    End Sub

    Private Sub ButtonSelectDll_Click(sender As Object, e As RoutedEventArgs)
        Dim folder As String = Directory.GetCurrentDirectory
        Dim solutionDir As String = Paths.TryGetSolutionDirectoryPath()
        Dim wildCardFileName As String = "*.dll"
        Dim dlls As List(Of String)

        If Not String.IsNullOrEmpty(_dependencies.DllWildCardSearchPattern) Then
            wildCardFileName = _dependencies.DllWildCardSearchPattern
        End If

        If String.IsNullOrEmpty(_dependencies.CustomDirectoryToSearchForDllsToLoadFrom) Then
            dlls = Paths.GetAllWildcardFilesInAnySubfolder(solutionDir, wildCardFileName)
        Else
            dlls = Paths.GetAllWildcardFilesInAnySubfolder(_dependencies.CustomDirectoryToSearchForDllsToLoadFrom, wildCardFileName)
        End If

        Dim window As WpfUi.WindowGetFilePath = New WpfUi.WindowGetFilePath(dlls)
        window.Width = 1200
        window.Height = 300
        window.ShowDialog()
        Dim wasOkClicked As Boolean = window.WasOkayClicked
        If wasOkClicked Then
            Dim filePath As String = window.SelectedFolder
            If Not File.Exists(filePath) Then
                WpfUi.Utils.SetErrorStatus(TextBlockStatus, TextBlockMessage, "Dll does not exist: " + filePath)
            End If
            WpfUi.Utils.SetSuccessStatus(TextBlockStatus, TextBlockMessage, "Selected dll to load: " + filePath)
            TextBoxDllPath.Text = filePath
        Else
            WpfUi.Utils.SetErrorStatus(TextBlockStatus, TextBlockMessage, "User closed dll load menu.")
        End If
    End Sub

    Private Sub ButtonFindNewestDllByName_Click(sender As Object, e As RoutedEventArgs)
        Dim dllName As String = System.IO.Path.GetFileName(TextBoxDllPath.Text)
        Dim mainAppDll As String = GetNewestDllInSolutionDirectorySubFoldersThatHaveAVInFolderName(dllName)
        If Not File.Exists(mainAppDll) Then
            WpfUi.Utils.SetErrorStatus(TextBlockStatus, TextBlockMessage, "Dll does not exist: " + mainAppDll)
        Else
            WpfUi.Utils.SetSuccessStatus(TextBlockStatus, TextBlockMessage, "Selected dll to load: " + mainAppDll)
            TextBoxDllPath.Text = mainAppDll
        End If

    End Sub

    Public Function GetNewestDllInSolutionDirectorySubFoldersThatHaveAVInFolderName(dllName As String) As String
        Dim solutionDir As String = Paths.TryGetSolutionDirectoryPath()
        Dim wildCardFileName As String = "*" + dllName
        Dim cadApps As List(Of String) = Paths.GetAllWildcardFilesInVSubfolder(solutionDir, wildCardFileName)
        Dim cadAppDll As String = cadApps.FirstOrDefault
        Return cadAppDll
    End Function

End Class
