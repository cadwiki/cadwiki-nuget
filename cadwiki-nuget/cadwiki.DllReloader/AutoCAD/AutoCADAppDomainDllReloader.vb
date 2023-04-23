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
    Public Class AutoCADAppDomainDllReloader
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
            Public LogMode As LogMode = LogMode.AcadDocEditor
        End Class

        Public Sub ClearIni()
            WriteDependecyValuesToIni(New Dependencies())
        End Sub

        Private _dependencyValues As Dependencies
        Private _cadwikiTempFolder As String = PathUtils.GetAutoCADAppDomainDllReloaderTempFolder()
        Private _iniPath As String = _cadwikiTempFolder + "\" + "AutoCADAppDomainDllReloader.ini"
        Private _tempFolder As String
        Private _sectionSettings As String = "Settings"
        Private _keyProjectName As String = "ProjectName"
        Private _keyAppVersion As String = "AppVersion"
        Private _keyReloadCount As String = "ReloadCount"
        Private _keyDllPath As String = "DllPath"
        Private _keyOriginalAppDirectory As String = "OriginalAppDirectory"
        Private _keyDLLsToReload As String = "DLLsToReload"
        Private _keyTerminated As String = "Terminated"
        Private _keyLogMode As String = "LogMode"

        Private _document As Document
        Private _textFileLog As New NetUtils.TextFileLog(PathUtils.GetAutoCADAppDomainDllReloaderTempFolderLogFilePath())

        Public Sub New()
            _dependencyValues = New Dependencies
        End Sub

        Public Sub New(dependencies As Dependencies)
            _dependencyValues = dependencies
        End Sub


        Public Function GetIExtensionApplicationClassName() As String
            Return _dependencyValues.IExtensionApplicationClassName
        End Function

        Public Function GetVersion() As Version
            Return _dependencyValues.AppVersion
        End Function

        Public Function GetReloadCount() As Integer
            Return _dependencyValues.ReloadCount
        End Function

        Public Function GetReloadedAssemblyNameSafely(currentIExtensionAppAssembly As Assembly) As String
            Dim dllName As String = If(_dependencyValues.ReloadedAssembly Is Nothing,
                currentIExtensionAppAssembly.FullName,
                _dependencyValues.ReloadedAssembly.FullName)
            Return dllName
        End Function

        Public Function GetDllPath() As String
            Return _dependencyValues.DllPath
        End Function

        Public Function GetDllsToReload() As List(Of String)
            Return _dependencyValues.DLLsToReload
        End Function
        Public Function GetDllsThatWereSuccessfullyReloaded() As List(Of String)
            Return _dependencyValues.SuccessfullyReloadedDlls
        End Function

        Public Function GetLogMode() As LogMode
            Return _dependencyValues.LogMode
        End Function

        Public Sub AddDllToSkip(dllName As String)
            _dependencyValues.DllsToSkip.Add(dllName)
        End Sub

        Private Sub AddDllToReload(dllFilePath As String)
            Dim isThisADllToSkip As Boolean = _dependencyValues.DllsToSkip.Contains(Path.GetFileName(dllFilePath))
            If Not isThisADllToSkip Then
                _dependencyValues.DLLsToReload.Add(dllFilePath)
            End If
        End Sub
        Public Sub Configure(currentIExtensionAppAssembly As Assembly,
                             loadAllDllsInAppAssemblyDirectory As Boolean)
            Try
                Log("---------------------------------------------")
                Log("---------------------------------------------")
                Log("Configure started.")
                ReadDependecyValuesFromIni()
                ' If Terminated = True
                ' And Versions don't match
                ' this is the first Initalize call from any IExtensionApplication in this AutoCAD session
                Dim wasLastReloaderStateTerminated As Boolean = GetTerminatedFlag()
                If wasLastReloaderStateTerminated = True Then
                    'Not NetReloader.GetVersion().Equals(iExtensionAppVersion) Then
                    SetReloadCount(0)
                    SetInitialValues(currentIExtensionAppAssembly)
                    If loadAllDllsInAppAssemblyDirectory = True Then
                        Dim dllPath As String = NetUtils.AssemblyUtils.GetFileLocationFromCodeBase(currentIExtensionAppAssembly)
                        ReloadAllDllsFoundInSameFolder(dllPath)
                    End If
                Else
                    SetReloadedValues(currentIExtensionAppAssembly)
                End If

                If String.IsNullOrEmpty(GetIExtensionApplicationClassName()) Then
                    SetIExtensionApplicationClassNameFromAssembly(currentIExtensionAppAssembly)
                End If
                Log("Configure complete.")
                Log("---------------------------------------------")
                Log("---------------------------------------------")
            Catch ex As Exception
                Dim window As WpfUi.Templates.WindowAutoCADException = New WpfUi.Templates.WindowAutoCADException(ex)
                window.Show()
            End Try

        End Sub

        Public Function ReloadAllDllsFoundInSameFolder(dllPath As String) As Tuple(Of Assembly, String)
            Try
                Log(String.Format("Reload count {0}.", _dependencyValues.ReloadCount))
                Dim newCount As Integer = _dependencyValues.ReloadCount + 1
                Dim dllRepository As String = Path.GetDirectoryName(dllPath)
                Dim now As DateTime = DateTime.Now
                _tempFolder = GetNewReloadFolder(newCount, now)
                IO.Directory.CreateDirectory(_tempFolder)
                Log("Created temp folder to copy dlls to for reloading: " + _tempFolder)
                Dim tempDlls As List(Of String) = CopyAllDllsToTempFolder(dllPath, _tempFolder)
                Dim tuple As Tuple(Of Assembly, String) = ReloadAll(tempDlls, newCount)
                Return tuple
            Catch ex As Exception
                Dim window As WpfUi.Templates.WindowAutoCADException = New WpfUi.Templates.WindowAutoCADException(ex)
                window.Show()
                Return Nothing
            End Try


        End Function

        Private Function CopyAllDllsToTempFolder(dllPath As String, tempFolder As String) As List(Of String)
            Dim tempDlls As New List(Of String)
            For Each dllFilePath As String In Directory.GetFiles(Path.GetDirectoryName(dllPath), "*.dll")
                Dim dllName As String = Path.GetFileName(dllFilePath)
                Dim tempFolderFilePath As String = tempFolder + "\" + dllName
                IO.File.Copy(dllFilePath, tempFolderFilePath)
                Log("Copied: " + tempFolderFilePath)
                tempDlls.Add(tempFolderFilePath)
            Next
            Return tempDlls
        End Function

        Public Sub Terminate()
            _dependencyValues = New Dependencies
            SetTerminated(True)
            WriteDependecyValuesToIni(_dependencyValues)
        End Sub



        Public Function UserInputGetDllPath() As String
            Dim filePaths As New List(Of String)
            Dim filePathOfCurrentExecutingAssembly As String = Assembly.GetExecutingAssembly.Location
            Dim exeDir As String = ""
            If String.IsNullOrEmpty(filePathOfCurrentExecutingAssembly) Then
                exeDir = _dependencyValues.OriginalAppDirectory
            Else
                exeDir = IO.Path.GetDirectoryName(filePathOfCurrentExecutingAssembly)
            End If

            If Not String.IsNullOrEmpty(exeDir) Then
                Dim parentDir As String = IO.Path.GetDirectoryName(exeDir)
                Dim wildCardFileName As String = "*" + _dependencyValues.IExtensionApplicationClassName + ".dll"
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

        Public Sub ReloadDll(doc As Document, iExtensionAppAssembly As Assembly, dllPath As String)

            If (dllPath IsNot Nothing) Then

                Try
                    _document = doc
                    Log("---------------------------------------------")
                    Log("---------------------------------------------")
                    Log("Dll reload started.")
                    WriteIniPathToDocEditor()
                    'Remove all commands from iExtensionAppAssembly
                    CommandRemover.RemoveAllCommandsFromiExtensionAppAssembly(doc, iExtensionAppAssembly, dllPath)
                    'RemoveAllCommandsFromAllAssembliesInAppDomain(doc, dllPath)
                    Dim tuple As Tuple(Of Assembly, String) = ReloadAllDllsFoundInSameFolder(dllPath)
                    Dim appAssembly As Assembly = tuple.Item1
                    Dim copiedMainDll As String = tuple.Item2

                    If _dependencyValues.OriginalAppDirectory Is Nothing Then
                        _dependencyValues.OriginalAppDirectory = IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly.Location)
                    End If
                    Dim originalDirectory As String = _dependencyValues.OriginalAppDirectory
                    Log(String.Format("Original app directory: {0}", originalDirectory))
                    Log(String.Format("Dll reload path: {0}", copiedMainDll))
                    Dim types As Type() = cadwiki.NetUtils.AssemblyUtils.GetTypesSafely(appAssembly)
                    WriteIniPathToDocEditor()
                    Log("Dll reload complete.")
                    Log("---------------------------------------------")
                    Log("---------------------------------------------")
                Catch ex As Exception
                    Log("Exception" + ex.Message)
                End Try
            End If
        End Sub

        Private Sub WriteIniPathToDocEditor()
            Log("Ini settings read from " + _iniPath)
        End Sub

        Private Sub RemoveAllCommandsFromAllAssembliesInAppDomain(doc As Document, dllPath As String)
            'Remove all commands from all assemblys in AppDomain
            For Each assembly As Assembly In AppDomain.CurrentDomain.GetAssemblies()
                Log("Attemping to remove commands from: " + assembly.GetName.Name)
                CommandRemover.RemoveAllCommandsFromiExtensionAppAssembly(doc, assembly, dllPath)
            Next
        End Sub

        Private Function GetTerminatedFlag() As Boolean
            Return _dependencyValues.Terminated
        End Function

        Private Sub SetReloadedAssembly(assembly As Assembly)
            _dependencyValues.ReloadedAssembly = assembly
        End Sub

        Private Sub SetOriginalAppDirectory(orginalAppDirectory As String)
            _dependencyValues.OriginalAppDirectory = orginalAppDirectory
        End Sub

        Private Sub SetDllPath(dllPath As String)
            _dependencyValues.DllPath = dllPath
        End Sub

        Private Sub SetAppVersion(appVersion As Version)
            _dependencyValues.AppVersion = appVersion
        End Sub

        Private Sub SetIExtensionApplicationClassName(projectName As String)
            _dependencyValues.IExtensionApplicationClassName = projectName
        End Sub

        Private Sub SetTerminated(terminated As Boolean)
            _dependencyValues.Terminated = terminated
        End Sub

        Private Sub SetReloadCount(count As Integer)
            _dependencyValues.ReloadCount = count
        End Sub
        Private Sub SetInitialValues(iExtensionAppAssembly As Assembly)
            'ReadDependecyValuesFromIni()
            'Only allow developer to set initial values when ReloadCount is 0
            If _dependencyValues.ReloadCount = 0 Then
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
                WriteDependecyValuesToIni(_dependencyValues)
            End If
        End Sub

        Private Sub SetIExtensionApplicationClassNameFromAssembly(iExtensionAppAssembly As Assembly)
            Dim iExtensionAppClassName As String = iExtensionAppAssembly.GetName().Name
            SetIExtensionApplicationClassName(iExtensionAppClassName)
        End Sub

        Private Sub SetReloadedValues(iExtensionAppAssembly As Assembly)
            ReadDependecyValuesFromIni()
            Dim appVersion As Version = cadwiki.NetUtils.AssemblyUtils.GetVersion(iExtensionAppAssembly)
            SetAppVersion(appVersion)
            SetReloadedAssembly(iExtensionAppAssembly)
        End Sub

        Private Sub WriteDependecyValuesToIni(dependencyValues As Dependencies)
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

        Private Sub CreateCadwikiTempFolderIfNotExists()
            If Not Directory.Exists(_cadwikiTempFolder) Then
                Directory.CreateDirectory(_cadwikiTempFolder)
            End If
        End Sub

        Private Sub ReadDependecyValuesFromIni()
            If Not File.Exists(_iniPath) Then
                WriteDependecyValuesToIni(_dependencyValues)
            End If
            Dim objIniFile As New cadwiki.NetUtils.IniFile(_iniPath)
            Dim stringValue As String
            stringValue = objIniFile.GetString(_sectionSettings, _keyProjectName, _dependencyValues.IExtensionApplicationClassName)
            _dependencyValues.IExtensionApplicationClassName = stringValue
            stringValue = objIniFile.GetString(_sectionSettings, _keyAppVersion, _dependencyValues.AppVersion.ToString)
            Dim version As Version = Version.Parse(stringValue)
            _dependencyValues.AppVersion = version
            stringValue = objIniFile.GetString(_sectionSettings, _keyReloadCount, _dependencyValues.ReloadCount.ToString)
            _dependencyValues.ReloadCount = CInt(stringValue)
            stringValue = objIniFile.GetString(_sectionSettings, _keyDllPath, _dependencyValues.DllPath)
            _dependencyValues.DllPath = stringValue
            stringValue = objIniFile.GetString(_sectionSettings, _keyOriginalAppDirectory, _dependencyValues.OriginalAppDirectory)
            _dependencyValues.OriginalAppDirectory = stringValue
            Dim DLLsConcat As String = String.Join(",", _dependencyValues.DLLsToReload)
            stringValue = objIniFile.GetString(_sectionSettings, _keyDLLsToReload, DLLsConcat)
            Dim dlls() As String = If(stringValue IsNot Nothing, stringValue.Split(CChar(",")), {})
            _dependencyValues.DLLsToReload = dlls.ToList
            stringValue = objIniFile.GetString(_sectionSettings, _keyTerminated, _dependencyValues.Terminated.ToString)
            _dependencyValues.Terminated = CType(stringValue, Boolean)
            stringValue = objIniFile.GetString(_sectionSettings, _keyLogMode, _dependencyValues.LogMode.ToString)
            LogMode.TryParse(stringValue, _dependencyValues.LogMode)
        End Sub

        Private Function ReloadAll(tempDlls As List(Of String), reloadCount As Integer) As Tuple(Of Assembly, String)
            Dim assemblies As Assembly() = AppDomain.CurrentDomain.GetAssemblies()
            Dim appAssembly As Assembly = Nothing
            Dim appAssemblyPath As String = ""
            Log("Looking for dlls to reload.")
            Log("Skipping these dlls: ")
            For Each dllToSkip As String In _dependencyValues.DllsToSkip
                Log(dllToSkip)
            Next
            appAssemblyPath = AddDllsToReloadToList(tempDlls, assemblies, appAssemblyPath)
            If String.IsNullOrEmpty(appAssemblyPath) Then
                Dim errorMessage As String = "Unable to locate the Assembly whose name contains: " + _dependencyValues.IExtensionApplicationClassName
                Log(errorMessage)
            Else
                'Add appAssembly as the last Item of the list, to ensure all other dlls are loaded before
                AddDllToReload(appAssemblyPath)
            End If
            WriteInfoAboutDllsToReload()
            'Remove all commands that will be reloaded into the app domain
            RemoveAllCommandsFromAnyAssemblyThatWillBeReloaded()
            ReloadDllsIntoAppDomain()
            Return New Tuple(Of Assembly, String)(appAssembly, appAssemblyPath)
        End Function

        Private Sub WriteInfoAboutDllsToReload()
            Log("Found " + _dependencyValues.DLLsToReload.Count.ToString +
                                    " dlls that are able to be loaded into the current appdomain.")
            If _dependencyValues.DLLsToReload.Count > 0 Then
                Log("These dlls have 1 of 2 qualities listed below:")
                Log("#1 They don't exist in the app domain yet.")
                Log("or")
                Log("#2 Their version number is newer than any assembly with the exact same name in the current app domain.")
                If _dependencyValues.DLLsToReload.Count > 0 Then
                    For Each dllToReload As String In _dependencyValues.DLLsToReload
                        Log("Dll to reload: " + dllToReload)
                    Next
                End If
            End If
        End Sub

        Private Sub RemoveAllCommandsFromAnyAssemblyThatWillBeReloaded()
            Dim assemblies As Assembly() = AppDomain.CurrentDomain.GetAssemblies()
            For Each dllFilePath As String In _dependencyValues.DLLsToReload
                Dim dllName As String = IO.Path.GetFileName(dllFilePath)
                Dim assemblyName As String = IO.Path.GetFileNameWithoutExtension(dllName)
                Dim newestAssemblyWithNameInAppDomain As Assembly =
                        AcadAssemblyUtils.GetNewestAssembly(assemblies, assemblyName, Nothing)
                'Remove any commands that need to be overwritten latter
                If Not newestAssemblyWithNameInAppDomain Is Nothing Then
                    CommandRemover.RemoveAllCommandsFromiExtensionAppAssembly(_document, newestAssemblyWithNameInAppDomain, _dependencyValues.OriginalAppDirectory)
                End If

            Next
        End Sub

        Private Function AddDllsToReloadToList(tempDlls As List(Of String), assemblies() As Assembly, appAssemblyPath As String) As String
            _dependencyValues.DLLsToReload.Clear()
            For Each tempDll As String In tempDlls
                If File.Exists(tempDll) Then
                    Dim dllName As String = IO.Path.GetFileName(tempDll)
                    Dim assemblyName As String = IO.Path.GetFileNameWithoutExtension(tempDll)
                    Dim newestAssemblyWithNameInAppDomain As Assembly =
                        AcadAssemblyUtils.GetNewestAssembly(assemblies, assemblyName, Nothing)
                    ' If there is not an existing assembly like this in the app domain
                    ' add to list
                    If newestAssemblyWithNameInAppDomain Is Nothing Then
                        AddDllToReload(tempDll)
                    Else
                        'skip all cadwiki dlls
                        If dllName.Contains("cadwiki.") Then
                            Log("Skipped cadwiki dll: " + dllName)
                            Continue For
                        End If
                        ' Else do version checks
                        Dim newestVersionInAppDomain As String =
                        AcadAssemblyUtils.GetAssemblyVersionFromFullName(newestAssemblyWithNameInAppDomain.FullName)
                        Dim copiedFvi As FileVersionInfo = FileVersionInfo.GetVersionInfo(tempDll)
                        Dim copiedVersion As String = copiedFvi.FileVersion
                        Dim isCopiedDllNewer As Integer = AcadAssemblyUtils.CompareFileVersion(copiedVersion, CStr(newestVersionInAppDomain))
                        ' For dlls that already exist in the app domain, only reload if the copy has a newer file version
                        ' which result in a 1 comparision between file versions
                        If isCopiedDllNewer = 1 Then
                            ' If dll name contains project name, don't store to list
                            If assemblyName.Contains(_dependencyValues.IExtensionApplicationClassName) Then
                                Log("Skipped iExtension app dll")
                                appAssemblyPath = tempDll
                            Else
                                Log(dllName + " added to list. App domain version:" + newestVersionInAppDomain + ", copied version:" + copiedVersion)
                                'Else store to list
                                AddDllToReload(tempDll)
                            End If
                        Else
                            Log(dllName + " skipped. App domain version:" + newestVersionInAppDomain + ", copied version:" + copiedVersion)
                        End If
                    End If

                Else
                End If
            Next

            Return appAssemblyPath
        End Function

        Private Function GetAssemblyByName(name As String, assemblies As Assembly()) As Assembly
            For Each assembly As Assembly In assemblies
                If assembly.GetName().Name.Equals(name) Then
                    Return assembly
                End If
            Next
            Return Nothing
        End Function
        Private Sub ReloadDllsIntoAppDomain()
            Dim assemblyWithIExtensionApp As Assembly = Nothing
            For Each dllPath As String In _dependencyValues.DLLsToReload
                Dim assemblyBytes As Byte() = Nothing
                Try
                    assemblyBytes = System.IO.File.ReadAllBytes(dllPath)
                Catch ex As Exception
                    Log("Error reading assembly to byte array: " + dllPath)
                    Log("Exception: " + ex.Message)
                End Try
                Try
                    If dllPath.Contains(_dependencyValues.IExtensionApplicationClassName) Then
                        'Update Reloader values
                        _dependencyValues.ReloadCount += 1
                        _dependencyValues.Terminated = False
                        WriteDependecyValuesToIni(_dependencyValues)
                        ' Upon loading the assemblyBytes from the IExtensionApplication class, the App.Initialize() method will be called
                        assemblyWithIExtensionApp = AppDomain.CurrentDomain.Load(assemblyBytes)
                        Log("Reloaded iExtensionAppAssembly dll: " + dllPath)
                        SetReloadedValues(assemblyWithIExtensionApp)
                        WriteDependecyValuesToIni(_dependencyValues)
                    Else
                        Dim reloadedAssembly As Assembly = AppDomain.CurrentDomain.Load(assemblyBytes)
                        Log("Reloaded dll: " + dllPath)
                    End If
                Catch ex As Exception
                    Log("Error loading assembly: " + dllPath)
                    Log("Exception: " + ex.Message)
                End Try
            Next
            Dim currentTypes As Type() = cadwiki.NetUtils.AssemblyUtils.GetTypesSafely(assemblyWithIExtensionApp)
            ' Create reference to the IExtensionApplication object
            Dim currentAppObject As Object = AcadAssemblyUtils.GetAppObjectSafely(currentTypes)
            'currentAppObject.Initialize()
        End Sub

        Private Function GetNewReloadFolder(count As Integer, time As DateTime) As String
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

        Private Function GetTimestampForReloadFolder(time As Date) As String
            Dim format As String = "yyyyMMdd--HH_mm_ss"
            Dim timeStamp As String = time.ToString(format)
            Return timeStamp
        End Function

        Public Sub Log(message As String)
            Dim mode As AutoCADAppDomainDllReloader.LogMode = GetLogMode()
            Select Case mode.Equals(AutoCADAppDomainDllReloader.LogMode.Off)
                Case mode.Equals(AutoCADAppDomainDllReloader.LogMode.Text)
                    LogToTextFile(message)
                Case mode.Equals(AutoCADAppDomainDllReloader.LogMode.AcadDocEditor)
                    LogToTextFile(message)
                    LogToEditor(message)
            End Select
        End Sub

        Private Sub LogToTextFile(message As String)
            If (_textFileLog Is Nothing) Then
                _textFileLog = New NetUtils.TextFileLog()
                _textFileLog.Write(message)
            Else
                _textFileLog.Write(message)
            End If
        End Sub

        Private Sub LogToEditor(message As String)
            If (_document Is Nothing) Then
                _document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument
                _document.Editor.WriteMessage(vbLf & message)
                _document.Editor.WriteMessage(vbLf)
            Else
                _document.Editor.WriteMessage(vbLf & message)
                _document.Editor.WriteMessage(vbLf)
            End If
        End Sub

        Public Sub LogException(ex As Exception)
            Dim mode As AutoCADAppDomainDllReloader.LogMode = GetLogMode()
            Select Case mode.Equals(AutoCADAppDomainDllReloader.LogMode.Off)
                Case mode.Equals(AutoCADAppDomainDllReloader.LogMode.Text)
                    LogExceptionToTextFile(ex)
                Case mode.Equals(AutoCADAppDomainDllReloader.LogMode.AcadDocEditor)
                    LogExceptionToTextFile(ex)
                    LogExceptionToEditor(ex)
            End Select
        End Sub

        Private Sub LogExceptionToTextFile(ex As Exception)
            If (_textFileLog Is Nothing) Then
                _textFileLog = New NetUtils.TextFileLog()
                _textFileLog.Exception(ex)
            Else
                _textFileLog.Exception(ex)
            End If
        End Sub

        Private Sub LogExceptionToEditor(ex As Exception)
            If (_document Is Nothing) Then
                _document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument
            End If

            Dim ed As Editor = _document.Editor
            Dim list As List(Of String) = NetUtils.Exceptions.GetPrettyStringList(ex)
            For Each str As String In list
                ed.WriteMessage(Environment.NewLine.ToString() & str)
            Next

        End Sub

    End Class

End Namespace


