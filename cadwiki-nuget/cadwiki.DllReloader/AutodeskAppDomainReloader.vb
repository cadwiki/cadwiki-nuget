Option Strict On
Option Infer Off
Option Explicit On

Imports System.Reflection
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.Runtime
Imports System.IO
Imports System.Globalization
Imports Autodesk.AutoCAD.EditorInput

Namespace AutoCAD
    Public MustInherit Class AutodeskAppDomainReloader

        Public TextFileLog As New NetUtils.TextFileLog(GetDllReloaderTempFolderLogFilePath())
        Public SkipCadwikiDlls As Boolean

        Public Enum LogMode
            Off
            Text
            AcadDocEditor
        End Enum

        Public Class Dependencies
            Public IExtensionApplicationClassName As String = ""
            Public AppVersion As Version = Version.Parse("1.0.0.0")
            Public ReloadCount As Integer = 0
            Public DllPath As String = ""
            Public ReloadedAssembly As Assembly = Nothing
            Public OriginalAppDirectory As String = ""
            Public DLLsToReload As New List(Of String)
            Public SuccessfullyReloadedDlls As New List(Of String)
            Public DllsToSkip As New List(Of String) From {
                "cadwiki.AcRemoveCmdGroup.dll",
                "AcCoreMgd.dll",
                "AcCui.dll",
                "AcDbMgd.dll",
                "acdbmgdbrep.dll",
                "AcDx.dll",
                "AcMgd.dll",
                "AcMr.dll",
                "AcSeamless.dll",
                "AcTcMgd.dll",
                "AcWindows.dll",
                "AdUIMgd.dll",
                "AdUiPalettes.dll",
                "AdWindows.dll"
            }
            Public Terminated As Boolean = True
            Public LogMode As LogMode = LogMode.Text
        End Class

        Public Sub ClearIni()
            WriteDependecyValuesToIni(New Dependencies())
        End Sub

        Private _logFolderName As String = "cadwiki.AutoCADAppDomainDllReloader"
        Private _logFileName As String = "AutoCADAppDomainDllReloader.txt"

        Public DependencyValues As Dependencies
        Private _cadwikiTempFolder As String = GetDllReloaderTempFolder()
        Private _iniPath As String = _cadwikiTempFolder + "\" + "AutodeskAppDomainDllReloader.ini"

        Private _sectionSettings As String = "Settings"
        Private _keyProjectName As String = "ProjectName"
        Private _keyAppVersion As String = "AppVersion"
        Private _keyReloadCount As String = "ReloadCount"
        Private _keyDllPath As String = "DllPath"
        Private _keyOriginalAppDirectory As String = "OriginalAppDirectory"
        Private _keyDLLsToReload As String = "DLLsToReload"
        Private _keyTerminated As String = "Terminated"
        Private _keyLogMode As String = "LogMode"



        Public Sub New()
            DependencyValues = New Dependencies
        End Sub

        Public Sub New(dependencies As Dependencies)
            DependencyValues = dependencies
        End Sub


        Public Function GetIExtensionApplicationClassName() As String
            Return DependencyValues.IExtensionApplicationClassName
        End Function

        Public Function GetVersion() As Version
            Return DependencyValues.AppVersion
        End Function

        Public Function GetReloadCount() As Integer
            Return DependencyValues.ReloadCount
        End Function

        Public Function GetReloadedAssemblyNameSafely(currentIExtensionAppAssembly As Assembly) As String
            Dim dllName As String = If(DependencyValues.ReloadedAssembly Is Nothing,
                currentIExtensionAppAssembly.FullName,
                DependencyValues.ReloadedAssembly.FullName)
            Return dllName
        End Function

        Public Function GetDllPath() As String
            Return DependencyValues.DllPath
        End Function

        Public Function GetDllsToReload() As List(Of String)
            Return DependencyValues.DLLsToReload
        End Function
        Public Function GetDllsThatWereSuccessfullyReloaded() As List(Of String)
            Return DependencyValues.SuccessfullyReloadedDlls
        End Function

        Public Function GetLogMode() As LogMode
            Return DependencyValues.LogMode
        End Function

        Public Sub AddDllToSkip(dllName As String)
            DependencyValues.DllsToSkip.Add(dllName)
        End Sub

        Public Sub AddDllToReload(dllFilePath As String)
            Dim isThisADllToSkip As Boolean = DependencyValues.DllsToSkip.Contains(Path.GetFileName(dllFilePath))
            If Not isThisADllToSkip Then
                DependencyValues.DLLsToReload.Add(dllFilePath)
            End If
        End Sub




        Public Sub Terminate()
            DependencyValues = New Dependencies
            SetTerminated(True)
            WriteDependecyValuesToIni(DependencyValues)
        End Sub



        Public Function UserInputGetDllPath() As String
            Dim filePaths As New List(Of String)
            Dim filePathOfCurrentExecutingAssembly As String = Assembly.GetExecutingAssembly.Location
            Dim exeDir As String = ""
            If String.IsNullOrEmpty(filePathOfCurrentExecutingAssembly) Then
                exeDir = DependencyValues.OriginalAppDirectory
            Else
                exeDir = IO.Path.GetDirectoryName(filePathOfCurrentExecutingAssembly)
            End If

            If Not String.IsNullOrEmpty(exeDir) Then
                Dim parentDir As String = IO.Path.GetDirectoryName(exeDir)
                Dim wildCardFileName As String = "*" + DependencyValues.IExtensionApplicationClassName + ".dll"
                Dim cadApps As List(Of String) = cadwiki.NetUtils.Paths.GetAllWildcardFilesInVSubfolder(parentDir, wildCardFileName)
                filePaths.AddRange(cadApps)
            End If

            Dim window As cadwiki.WpfUi.WindowGetFilePath = New cadwiki.WpfUi.WindowGetFilePath(filePaths)
            window.Width = 1200
            window.Height = 300
            window.ShowDialog()
            Dim wasOkClicked As Boolean = window.WasOkayClicked
            If wasOkClicked Then
                Dim filePath As String = window.SelectedFolder
                Return filePath
            Else
                Return Nothing
            End If
        End Function

        Public Shared Function AssemblyResolve(sender As Object, args As ResolveEventArgs) As Assembly
            Dim assemblies As Assembly() = AppDomain.CurrentDomain.GetAssemblies()
            For Each assembly As Assembly In assemblies
                If String.Equals(assembly.FullName, args.Name) Then
                    Return assembly
                End If
            Next
            Return Nothing
        End Function



        Public Sub WriteIniPathToDocEditor()
            Log("Ini settings read from " + _iniPath)
        End Sub



        Public Function GetTerminatedFlag() As Boolean
            Return DependencyValues.Terminated
        End Function

        Public Sub SetReloadedAssembly(assembly As Assembly)
            DependencyValues.ReloadedAssembly = assembly
        End Sub

        Public Sub SetOriginalAppDirectory(orginalAppDirectory As String)
            DependencyValues.OriginalAppDirectory = orginalAppDirectory
        End Sub

        Public Sub SetDllPath(dllPath As String)
            DependencyValues.DllPath = dllPath
        End Sub

        Public Sub SetAppVersion(appVersion As Version)
            DependencyValues.AppVersion = appVersion
        End Sub

        Public Sub SetIExtensionApplicationClassName(projectName As String)
            DependencyValues.IExtensionApplicationClassName = projectName
        End Sub

        Public Sub SetTerminated(terminated As Boolean)
            DependencyValues.Terminated = terminated
        End Sub

        Public Sub SetReloadCount(count As Integer)
            DependencyValues.ReloadCount = count
        End Sub
        Public Sub SetInitialValues(iExtensionAppAssembly As Assembly)
            'ReadDependecyValuesFromIni()
            'Only allow developer to set initial values when ReloadCount is 0
            If DependencyValues.ReloadCount = 0 Then
                SetIExtensionApplicationClassNameFromAssembly(iExtensionAppAssembly)
                Dim appVersion As Version = cadwiki.NetUtils.AssemblyUtils.GetVersion(iExtensionAppAssembly)
                SetAppVersion(appVersion)
                SetReloadedAssembly(iExtensionAppAssembly)
                Dim assemblyPath As String = iExtensionAppAssembly.Location
                If Not String.IsNullOrEmpty(assemblyPath) Then
                    SetDllPath(assemblyPath)
                    SetOriginalAppDirectory(System.IO.Path.GetDirectoryName(assemblyPath))
                End If
                SetTerminated(False)
                WriteDependecyValuesToIni(DependencyValues)
            End If
        End Sub

        Public Sub SetIExtensionApplicationClassNameFromAssembly(iExtensionAppAssembly As Assembly)
            Dim iExtensionAppClassName As String = iExtensionAppAssembly.GetName().Name
            SetIExtensionApplicationClassName(iExtensionAppClassName)
        End Sub

        Public Sub SetReloadedValues(iExtensionAppAssembly As Assembly)
            ReadDependecyValuesFromIni()
            Dim appVersion As Version = cadwiki.NetUtils.AssemblyUtils.GetVersion(iExtensionAppAssembly)
            SetAppVersion(appVersion)
            SetReloadedAssembly(iExtensionAppAssembly)
        End Sub

        Public Sub WriteDependecyValuesToIni(dependencyValues As Dependencies)
            CreateCadwikiTempFolderIfNotExists()
            Dim objIniFile As New cadwiki.NetUtils.IniFile(_iniPath)
            objIniFile.WriteString(_sectionSettings, _keyProjectName, dependencyValues.IExtensionApplicationClassName)
            objIniFile.WriteString(_sectionSettings, _keyAppVersion, dependencyValues.AppVersion.ToString)
            objIniFile.WriteString(_sectionSettings, _keyReloadCount, dependencyValues.ReloadCount.ToString)
            objIniFile.WriteString(_sectionSettings, _keyDllPath, dependencyValues.DllPath)
            objIniFile.WriteString(_sectionSettings, _keyOriginalAppDirectory, dependencyValues.OriginalAppDirectory)
            Dim DLLsConcat As String = String.Join(",", dependencyValues.DLLsToReload)
            objIniFile.WriteString(_sectionSettings, _keyDLLsToReload, DLLsConcat)
            objIniFile.WriteString(_sectionSettings, _keyTerminated, dependencyValues.Terminated.ToString)
            objIniFile.WriteString(_sectionSettings, _keyLogMode, dependencyValues.LogMode.ToString)
        End Sub

        Public Sub CreateCadwikiTempFolderIfNotExists()
            If Not Directory.Exists(_cadwikiTempFolder) Then
                Directory.CreateDirectory(_cadwikiTempFolder)
            End If
        End Sub

        Public Sub ReadDependecyValuesFromIni()
            If Not File.Exists(_iniPath) Then
                WriteDependecyValuesToIni(DependencyValues)
            End If
            Dim objIniFile As New cadwiki.NetUtils.IniFile(_iniPath)
            Dim stringValue As String
            stringValue = objIniFile.GetString(_sectionSettings, _keyProjectName, DependencyValues.IExtensionApplicationClassName)
            DependencyValues.IExtensionApplicationClassName = stringValue
            stringValue = objIniFile.GetString(_sectionSettings, _keyAppVersion, DependencyValues.AppVersion.ToString)
            Dim version As Version = Version.Parse(stringValue)
            DependencyValues.AppVersion = version
            stringValue = objIniFile.GetString(_sectionSettings, _keyReloadCount, DependencyValues.ReloadCount.ToString)
            DependencyValues.ReloadCount = CInt(stringValue)
            stringValue = objIniFile.GetString(_sectionSettings, _keyDllPath, DependencyValues.DllPath)
            DependencyValues.DllPath = stringValue
            stringValue = objIniFile.GetString(_sectionSettings, _keyOriginalAppDirectory, DependencyValues.OriginalAppDirectory)
            DependencyValues.OriginalAppDirectory = stringValue
            Dim DLLsConcat As String = String.Join(",", DependencyValues.DLLsToReload)
            stringValue = objIniFile.GetString(_sectionSettings, _keyDLLsToReload, DLLsConcat)
            Dim dlls() As String = If(stringValue IsNot Nothing, stringValue.Split(CChar(",")), {})
            DependencyValues.DLLsToReload = dlls.ToList
            stringValue = objIniFile.GetString(_sectionSettings, _keyTerminated, DependencyValues.Terminated.ToString)
            DependencyValues.Terminated = CType(stringValue, Boolean)
            stringValue = objIniFile.GetString(_sectionSettings, _keyLogMode, DependencyValues.LogMode.ToString)
            LogMode.TryParse(stringValue, DependencyValues.LogMode)
        End Sub



        Public Sub WriteInfoAboutDllsToReload()
            Log("Found " + DependencyValues.DLLsToReload.Count.ToString +
                                    " dlls that are able to be loaded into the current appdomain.")
            If DependencyValues.DLLsToReload.Count > 0 Then
                Log("These dlls have 1 of 2 qualities listed below:")
                Log("#1 They don't exist in the app domain yet.")
                Log("or")
                Log("#2 Their version number is newer than any assembly with the exact same name in the current app domain.")
                If DependencyValues.DLLsToReload.Count > 0 Then
                    For Each dllToReload As String In DependencyValues.DLLsToReload
                        Log("Dll to reload: " + dllToReload)
                    Next
                End If
            End If
        End Sub





        Public Function GetAssemblyByName(name As String, assemblies As Assembly()) As Assembly
            For Each assembly As Assembly In assemblies
                If assembly.GetName().Name.Equals(name) Then
                    Return assembly
                End If
            Next
            Return Nothing
        End Function
        

        Public Function GetNewReloadFolder(count As Integer, time As DateTime) As String
            Dim timeStamp As String = GetTimestampForReloadFolder(time)
            Dim reloadFolder As String = _cadwikiTempFolder + "\" + timeStamp + "--Reload-" + count.ToString()
            Dim uniqueFolder As String = reloadFolder
            Dim int As Integer = 1
            'While necessary, add "(#) until unique folder is found
            Do While Directory.Exists(uniqueFolder)
                uniqueFolder = reloadFolder + "(" + int.ToString + ")"
                int = int + 1
            Loop
            Return uniqueFolder
        End Function

        Public Function GetTimestampForReloadFolder(time As Date) As String
            Dim format As String = "yyyyMMdd--HH_mm_ss"
            Dim timeStamp As String = time.ToString(format)
            Return timeStamp
        End Function

        Public Sub Log(message As String)
            Dim mode As AutodeskAppDomainReloader.LogMode = GetLogMode()
            Select Case mode.Equals(AutodeskAppDomainReloader.LogMode.Off)
                Case mode.Equals(AutodeskAppDomainReloader.LogMode.Text)
                    LogToTextFile(message)
            End Select
        End Sub

        Public Sub LogToTextFile(message As String)
            If (TextFileLog Is Nothing) Then
                TextFileLog = New NetUtils.TextFileLog()
                TextFileLog.Write(message)
            Else
                TextFileLog.Write(message)
            End If
        End Sub


        Public Sub LogException(ex As Exception)
            Dim mode As AutodeskAppDomainReloader.LogMode = GetLogMode()
            Select Case mode.Equals(AutodeskAppDomainReloader.LogMode.Off)
                Case mode.Equals(AutodeskAppDomainReloader.LogMode.Text)
                    LogExceptionToTextFile(ex)
            End Select
        End Sub

        Public Sub LogExceptionToTextFile(ex As Exception)
            If (TextFileLog Is Nothing) Then
                TextFileLog = New NetUtils.TextFileLog()
                TextFileLog.Exception(ex)
            Else
                TextFileLog.Exception(ex)
            End If
        End Sub

        Public Function GetDllReloaderTempFolder() As String
            Return System.IO.Path.GetTempPath() + _logFolderName
        End Function

        Public Function GetDllReloaderTempFolderLogFilePath() As String
            Return GetDllReloaderTempFolder() + "\" + _logFileName
        End Function

    End Class

End Namespace
