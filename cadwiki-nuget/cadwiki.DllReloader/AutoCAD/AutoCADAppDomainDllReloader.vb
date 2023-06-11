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
        Inherits AutodeskAppDomainReloader

        Private _document As Document
        Private _tempFolder As String

        Public Shadows Function GetLogMode() As LogMode
            Return DependencyValues.LogMode
        End Function

        Public Sub Configure(currentIExtensionAppAssembly As Assembly)
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












        Public Sub Reload(currentIExtensionAppAssembly As Assembly)
            Try
                Log("---------------------------------------------")
                Log("---------------------------------------------")
                Log("Reload started.")
                ' If Terminated = True
                ' And Versions don't match
                ' this is the first Initalize call from any IExtensionApplication in this AutoCAD session
                Dim wasLastReloaderStateTerminated As Boolean = GetTerminatedFlag()
                If wasLastReloaderStateTerminated = True Then
                    'Not NetReloader.GetVersion().Equals(iExtensionAppVersion) Then
                    Dim dllPath As String = NetUtils.AssemblyUtils.GetFileLocationFromCodeBase(currentIExtensionAppAssembly)
                    ReloadAllDllsFoundInSameFolder(dllPath)
                Else
                    SetReloadedValues(currentIExtensionAppAssembly)
                End If

                If String.IsNullOrEmpty(GetIExtensionApplicationClassName()) Then
                    SetIExtensionApplicationClassNameFromAssembly(currentIExtensionAppAssembly)
                End If
                Log("Reload complete.")
                Log("---------------------------------------------")
                Log("---------------------------------------------")
            Catch ex As Exception
                Dim window As WpfUi.Templates.WindowAutoCADException = New WpfUi.Templates.WindowAutoCADException(ex)
                window.Show()
            End Try
        End Sub

        'Called from DllReloadClickCommandHandler
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

                    If DependencyValues.OriginalAppDirectory Is Nothing Then
                        DependencyValues.OriginalAppDirectory = IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly.Location)
                    End If
                    Dim originalDirectory As String = DependencyValues.OriginalAppDirectory
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






        Private Function ReloadAllDllsFoundInSameFolder(dllPath As String) As Tuple(Of Assembly, String)
            Try
                Log(String.Format("Reload count {0}.", DependencyValues.ReloadCount))
                Dim newCount As Integer = DependencyValues.ReloadCount + 1
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

        Private Function ReloadAll(tempDlls As List(Of String), reloadCount As Integer) As Tuple(Of Assembly, String)
            Dim assemblies As Assembly() = AppDomain.CurrentDomain.GetAssemblies()
            Dim appAssembly As Assembly = Nothing
            Dim appAssemblyPath As String = ""
            Log("Looking for dlls to reload.")
            Log("Skipping these dlls: ")
            For Each dllToSkip As String In DependencyValues.DllsToSkip
                Log(dllToSkip)
            Next
            appAssemblyPath = AddDllsToReloadToList(tempDlls, assemblies, appAssemblyPath)
            If String.IsNullOrEmpty(appAssemblyPath) Then
                Dim errorMessage As String = "Unable to locate the Assembly whose name contains: " + DependencyValues.IExtensionApplicationClassName
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


        Private Sub RemoveAllCommandsFromAnyAssemblyThatWillBeReloaded()
            Dim assemblies As Assembly() = AppDomain.CurrentDomain.GetAssemblies()
            For Each dllFilePath As String In DependencyValues.DLLsToReload
                Dim dllName As String = IO.Path.GetFileName(dllFilePath)
                Dim assemblyName As String = IO.Path.GetFileNameWithoutExtension(dllName)
                Dim newestAssemblyWithNameInAppDomain As Assembly =
                        AcadAssemblyUtils.GetNewestAssembly(assemblies, assemblyName, Nothing)
                'Remove any commands that need to be overwritten latter
                If Not newestAssemblyWithNameInAppDomain Is Nothing Then
                    CommandRemover.RemoveAllCommandsFromiExtensionAppAssembly(_document, newestAssemblyWithNameInAppDomain, DependencyValues.OriginalAppDirectory)
                End If

            Next
        End Sub

        Private Function AddDllsToReloadToList(tempDlls As List(Of String), assemblies() As Assembly, appAssemblyPath As String) As String
            DependencyValues.DLLsToReload.Clear()
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
                            If assemblyName.Contains(DependencyValues.IExtensionApplicationClassName) Then
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

        Private Sub ReloadDllsIntoAppDomain()
            Dim assemblyWithIExtensionApp As Assembly = Nothing
            For Each dllPath As String In DependencyValues.DLLsToReload
                Dim assemblyBytes As Byte() = Nothing
                Try
                    assemblyBytes = System.IO.File.ReadAllBytes(dllPath)
                Catch ex As Exception
                    Log("Error reading assembly to byte array: " + dllPath)
                    Log("Exception: " + ex.Message)
                End Try
                Try
                    If dllPath.Contains(DependencyValues.IExtensionApplicationClassName) Then
                        'Update Reloader values
                        DependencyValues.ReloadCount += 1
                        DependencyValues.Terminated = False
                        WriteDependecyValuesToIni(DependencyValues)
                        ' Upon loading the assemblyBytes from the IExtensionApplication class, the App.Initialize() method will be called
                        assemblyWithIExtensionApp = AppDomain.CurrentDomain.Load(assemblyBytes)
                        Log("Reloaded iExtensionAppAssembly dll: " + dllPath)
                        SetReloadedValues(assemblyWithIExtensionApp)
                        WriteDependecyValuesToIni(DependencyValues)
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



















        Public Shadows Sub Log(message As String)
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
            If (TextFileLog Is Nothing) Then
                TextFileLog = New NetUtils.TextFileLog()
                TextFileLog.Write(message)
            Else
                TextFileLog.Write(message)
            End If
        End Sub

        Private Sub LogToEditor(message As String)
            If (_document IsNot Nothing) Then
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
            If (TextFileLog Is Nothing) Then
                TextFileLog = New NetUtils.TextFileLog()
                TextFileLog.Exception(ex)
            Else
                TextFileLog.Exception(ex)
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


