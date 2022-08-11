Option Strict On
Option Infer Off
Option Explicit On

Imports System.Reflection
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.Runtime
Imports System.IO

Namespace AutoCAD
    Public Class AutoCADAppDomainDllReloader
        Public Class Dependencies
            Public IExtensionApplicationClassName As String = ""
            Public AppVersion As Version = Version.Parse("1.0.0.0")
            Public ReloadCount As Integer = 0
            Public DllPath As String = ""
            Public ReloadedAssembly As Assembly = Nothing
            Public OriginalAppDirectory As String = ""
            Public DLLsToReload As New List(Of String) From {""}
            Public Terminated As Boolean = False
        End Class

        Private _dependencyValues As Dependencies
        Private _iniPath As String = Path.GetTempPath() + "NetReloader.ini"
        Private _tempFolder As String
        Private _sectionSettings As String = "Settings"
        Private _keyProjectName As String = "ProjectName"
        Private _keyAppVersion As String = "AppVersion"
        Private _keyReloadCount As String = "ReloadCount"
        Private _keyDllPath As String = "DllPath"
        Private _keyOriginalAppDirectory As String = "OriginalAppDirectory"
        Private _keyDLLsToReload As String = "DLLsToReload"
        Private _keyTerminated As String = "Terminated"
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

        Public Sub Configure(currentIExtensionAppAssembly As Assembly)
            ReadDependecyValuesFromIni()
            ' If Terminated = True
            ' And Versions don't match
            ' this is the first Initalize call from any IExtensionApplication in this AutoCAD session
            If GetTerminatedFlag() = True Then
                'Not NetReloader.GetVersion().Equals(iExtensionAppVersion) Then
                SetReloadCount(0)
                SetInitialValues(currentIExtensionAppAssembly)
            Else
                SetReloadedValues(currentIExtensionAppAssembly)
            End If
        End Sub

        Public Sub Terminate()
            _dependencyValues = New Dependencies
            SetTerminated(True)
            WriteDependecyValuesToIni()
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
                    'Remove all commands
                    CommandRemover.RemoveAllCommandsFromiExtensionAppAssembly(doc, iExtensionAppAssembly, dllPath)
                    'Dim currentAssembly As Assembly = cadwiki.NetUtils.AssemblyUtils.GetCurrentlyExecutingAssembly()
                    'Dim currentTypes As Type() = cadwiki.NetUtils.AssemblyUtils.GetTypesSafely(currentAssembly)
                    ' Create reference to the IExtensionApplication object
                    'Dim currentAppObject As App = CType(AcadAssemblyUtils.GetAppObjectSafely(currentTypes), App)
                    doc.Editor.WriteMessage(vbLf & "Reload count {0}.", _dependencyValues.ReloadCount)
                    Dim newCount As Integer = _dependencyValues.ReloadCount + 1
                    Dim dllRepository As String = Path.GetDirectoryName(dllPath)
                    _tempFolder = GetNewTempFolder()
                    IO.Directory.CreateDirectory(_tempFolder)
                    Dim reloadFolder As String = _tempFolder + "\" + "Reload-" + newCount.ToString()
                    IO.Directory.CreateDirectory(reloadFolder)
                    _tempFolder = reloadFolder
                    Dim tuple As Tuple(Of Assembly, String) = ReloadAll(doc, _tempFolder, dllRepository, newCount)
                    'AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf AssemblyResolve
                    Dim appAssembly As Assembly = tuple.Item1
                    Dim copiedMainDll As String = tuple.Item2

                    If _dependencyValues.OriginalAppDirectory Is Nothing Then
                        _dependencyValues.OriginalAppDirectory = IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly.Location)
                    End If
                    Dim originalDirectory As String = _dependencyValues.OriginalAppDirectory
                    doc.Editor.WriteMessage(vbLf & "Original app directory: {0}", originalDirectory)
                    doc.Editor.WriteMessage(vbLf & "Dll reload path: {0}", copiedMainDll)
                    Dim types As Type() = cadwiki.NetUtils.AssemblyUtils.GetTypesSafely(appAssembly)
                    ' Create reference to the IExtensionApplication object
                    'Dim appObject As Object = AcadAssemblyUtils.GetAppObjectSafely(types)
                    doc.Editor.WriteMessage(vbLf & "Dll reload complete.")
                Catch ex As Exception
                    doc.Editor.WriteMessage(vbLf & "Exception" + ex.Message)
                End Try
            End If
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
                Dim iExtensionAppClassName As String = iExtensionAppAssembly.GetName().Name
                SetIExtensionApplicationClassName(iExtensionAppClassName)
                Dim appVersion As Version = cadwiki.NetUtils.AssemblyUtils.GetVersion(iExtensionAppAssembly)
                SetAppVersion(appVersion)
                SetReloadedAssembly(iExtensionAppAssembly)
                Dim assemblyPath As String = iExtensionAppAssembly.Location
                If Not String.IsNullOrEmpty(assemblyPath) Then
                    SetDllPath(assemblyPath)
                    SetOriginalAppDirectory(System.IO.Path.GetDirectoryName(assemblyPath))
                End If
                SetTerminated(False)
                WriteDependecyValuesToIni()
            End If
        End Sub

        Private Sub SetReloadedValues(iExtensionAppAssembly As Assembly)
            ReadDependecyValuesFromIni()
            Dim appVersion As Version = cadwiki.NetUtils.AssemblyUtils.GetVersion(iExtensionAppAssembly)
            SetAppVersion(appVersion)
            SetReloadedAssembly(iExtensionAppAssembly)
        End Sub

        Private Sub WriteDependecyValuesToIni()
            Dim objIniFile As New cadwiki.NetUtils.IniFile(_iniPath)
            objIniFile.WriteString(_sectionSettings, _keyProjectName, _dependencyValues.IExtensionApplicationClassName)
            objIniFile.WriteString(_sectionSettings, _keyAppVersion, _dependencyValues.AppVersion.ToString)
            objIniFile.WriteString(_sectionSettings, _keyReloadCount, _dependencyValues.ReloadCount.ToString)
            objIniFile.WriteString(_sectionSettings, _keyDllPath, _dependencyValues.DllPath)
            objIniFile.WriteString(_sectionSettings, _keyOriginalAppDirectory, _dependencyValues.OriginalAppDirectory)
            Dim DLLsConcat As String = String.Join(",", _dependencyValues.DLLsToReload)
            objIniFile.WriteString(_sectionSettings, _keyDLLsToReload, DLLsConcat)
            objIniFile.WriteString(_sectionSettings, _keyTerminated, _dependencyValues.Terminated.ToString)
        End Sub

        Private Sub ReadDependecyValuesFromIni()
            If Not File.Exists(_iniPath) Then
                WriteDependecyValuesToIni()
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
        End Sub

        Private Function ReloadAll(doc As Document, newTempFolder As String, dllRepository As String, reloadCount As Integer) As Tuple(Of Assembly, String)
            Dim assemblies As Assembly() = AppDomain.CurrentDomain.GetAssemblies()
            Dim appAssembly As Assembly = Nothing
            Dim appAssemblyPath As String = ""
            _dependencyValues.DLLsToReload.Clear()
            For Each loadedAssembly As Assembly In assemblies
                If Not loadedAssembly.IsDynamic Then
                    Dim dllPath As String = loadedAssembly.Location
                    Dim dllName As String = Path.GetFileName(dllPath)
                    Dim dllToReload As String = dllRepository + "\" + dllName
                    If File.Exists(dllToReload) Then
                        Dim copedFileName As String = dllName
                        Dim codeBaseFolder As String = AcadAssemblyUtils.GetFolderLocationFromCodeBase(loadedAssembly)
                        Dim tempFolderFilePath As String = newTempFolder + "\" + copedFileName
                        IO.File.Copy(dllToReload, tempFolderFilePath)
                        Dim assemblyName As String = IO.Path.GetFileNameWithoutExtension(dllName)
                        Dim newestAssemblyWithNameInAppDomain As Assembly = AutoCAD.UiRibbon.Buttons.GenericClickCommandHandler.GetNewestAssembly(assemblies, assemblyName, Nothing)
                        Dim newestVersionInAppDomain As String = AutoCAD.UiRibbon.Buttons.GenericClickCommandHandler.GetAssemblyVersionFromFullName(newestAssemblyWithNameInAppDomain.FullName)
                        Dim copiedFvi As FileVersionInfo = FileVersionInfo.GetVersionInfo(tempFolderFilePath)
                        Dim copiedVersion As String = copiedFvi.FileVersion
                        Dim isCopiedDllNewer As Integer = AutoCAD.UiRibbon.Buttons.GenericClickCommandHandler.CompareFileVersion(copiedVersion, CStr(newestVersionInAppDomain))
                        ' For dlls that already exist in the app domain, only reload if the copy has a newer file version
                        ' which result in a 1 comparision between file versions
                        If isCopiedDllNewer = 1 Then
                            ' If dll name contains project name, don't store to list
                            If dllName.Contains(_dependencyValues.IExtensionApplicationClassName) Then
                                appAssemblyPath = tempFolderFilePath
                            Else
                                'Else store to list
                                _dependencyValues.DLLsToReload.Add(tempFolderFilePath)
                            End If
                        End If
                    End If
                End If
            Next
            If String.IsNullOrEmpty(appAssemblyPath) Then
                Dim errorMessage As String = "Unable to locate the Assembly whose name contains: " + _dependencyValues.IExtensionApplicationClassName
                doc.Editor.WriteMessage(vbLf & errorMessage)
            Else
                'Add appAssembly as the last Item of the list, to ensure all other dlls are loaded before
                _dependencyValues.DLLsToReload.Add(appAssemblyPath)
            End If
            ReloadDllsIntoAppDomain(doc)
            Return New Tuple(Of Assembly, String)(appAssembly, appAssemblyPath)
        End Function

        Private Sub ReloadDllsIntoAppDomain(doc As Document)
            Dim assemblyWithIExtensionApp As Assembly = Nothing
            For Each dllPath As String In _dependencyValues.DLLsToReload
                Dim assemblyBytes As Byte() = Nothing
                Try
                    assemblyBytes = System.IO.File.ReadAllBytes(dllPath)
                Catch ex As Exception
                    doc.Editor.WriteMessage(vbLf + "Error reading assembly to byte array: " + dllPath)
                    doc.Editor.WriteMessage(vbLf + "Exception: " + ex.Message)
                End Try
                Try
                    If dllPath.Contains(_dependencyValues.IExtensionApplicationClassName) Then
                        'Update Reloader values
                        _dependencyValues.ReloadCount += 1
                        _dependencyValues.Terminated = False
                        WriteDependecyValuesToIni()
                        ' Upon loading the assemblyBytes from the IExtensionApplication class, the App.Initialize() method will be called
                        assemblyWithIExtensionApp = AppDomain.CurrentDomain.Load(assemblyBytes)
                        SetReloadedValues(assemblyWithIExtensionApp)
                        WriteDependecyValuesToIni()
                    Else
                        Dim reloadedAssembly As Assembly = AppDomain.CurrentDomain.Load(assemblyBytes)
                    End If
                Catch ex As Exception
                    doc.Editor.WriteMessage(vbLf + "Error loading assembly: " + dllPath)
                    doc.Editor.WriteMessage(vbLf + "Exception: " + ex.Message)
                End Try
            Next
            Dim currentTypes As Type() = cadwiki.NetUtils.AssemblyUtils.GetTypesSafely(assemblyWithIExtensionApp)
            ' Create reference to the IExtensionApplication object
            Dim currentAppObject As Object = AcadAssemblyUtils.GetAppObjectSafely(currentTypes)
            'currentAppObject.Initialize()
        End Sub

        Private Shared Function GetNewTempFolder() As String
            Dim folder As String = Path.Combine(Path.GetTempPath, Path.GetRandomFileName)
            Do While Directory.Exists(folder) Or File.Exists(folder)
                folder = Path.Combine(Path.GetTempPath, Path.GetRandomFileName)
            Loop
            Return folder
        End Function

    End Class

End Namespace


