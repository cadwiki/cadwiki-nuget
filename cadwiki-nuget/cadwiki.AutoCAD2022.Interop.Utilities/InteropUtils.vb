Option Explicit On
Option Strict On
Option Infer Off


Imports System.Runtime.InteropServices
Imports Autodesk.AutoCAD.Interop
Imports Autodesk.AutoCAD.Interop.Common

Public Class InteropUtils
    Public Declare Auto Function GetWindowThreadProcessId Lib "user32"(ByVal hwnd As IntPtr,
        ByRef lpdwProcessId As IntPtr) As IntPtr

    Private ReadOnly AutoCADProgId As String = "AutoCAD.Application.R24.1"
    Private App As AcadApplication


    Public Function IsAutoCADRunning() As Boolean
        Dim appAcad As AcadApplication = GetRunningAutoCADInstance()
        If appAcad Is Nothing Then
            Return False
        End If
        Return True
    End Function

    Public Function ConfigureRunningAutoCADForUsage() As Boolean
        If App Is Nothing Then
            Return False
        End If
        cadwiki.NetUtils.MessageFilter.Register()
        SetAutoCADWindowToNormal()
        Return True
    End Function

    Private autocad2021ProgId As String = "AutoCAD.Application.24"

    Public Function StartAutoCADApp(autocadProcessInfo As ProcessStartInfo) As Boolean
        Dim pr As Process = Process.Start(autocadProcessInfo)
        pr.WaitForInputIdle()
        Dim appAcad As AcadApplication = Nothing
        While appAcad Is Nothing
            Try
                appAcad = CType(Marshal.GetActiveObject(autocad2021ProgId), AcadApplication)
            Catch ex As Exception
                Debug.WriteLine("Error: " + ex.Message)
                Threading.Thread.Sleep(1000)
            End Try
        End While
        App = appAcad
        cadwiki.NetUtils.MessageFilter.Register()
        SetAutoCADWindowToNormal()
        Return True
    End Function

    Public Function NetloadDll(dllPath As String) As Boolean
        If Not IO.File.Exists(dllPath) Then
            Throw New Exception("Dll does not exist: " + dllPath)
        End If
        App.ActiveDocument.SendCommand("(setvar ""secureload"" 0)" + vbLf)
        dllPath = dllPath.Replace("\", "\\")
        App.ActiveDocument.SendCommand("(command ""_netload"" """ + dllPath + """)" + vbLf)
        Return True
    End Function

    Public Function OpenDrawingTemplate(dwtFilePath As String, readOnlyMode As Boolean) As Boolean
        If (App Is Nothing) Then
            Return False
        End If
        Dim doc As AcadDocument = App.Documents.Open(dwtFilePath, readOnlyMode)
        If doc IsNot Nothing Then
            Return True
        End If
        Return False
    End Function

    Public Function LoadCuiFile(cuiMenuFileName As String, menuGroupName As String) As Boolean
        If (App Is Nothing) Then
            Return False
        End If

        Dim mainCui As Object = App.ActiveDocument.GetVariable("MENUNAME")
        Dim cuiFile As String = CType(mainCui, String) + ".cui"

        Dim menuGroups As AcadMenuGroups = App.MenuGroups
        Dim menuGroup As AcadMenuGroup = GetExistingMenuGroup(menuGroups, menuGroupName)

        If (menuGroup Is Nothing) Then
            menuGroup = App.MenuGroups.Load(cuiMenuFileName)
            App.ActiveDocument.SendCommand("(command ""+RIBBON"" ""LOADER.RBNU_230_F2E64"")" + vbLf)
            Return True
        Else
            Return False
        End If
        Return Nothing
    End Function

    Public Function DoesMenuGroupExist(groupName As String) As Boolean
        If App Is Nothing Then
            Return False
        End If
        Dim menuGroups As AcadMenuGroups = App.MenuGroups
        Dim i As Integer = 0
        Dim currentGroup As AcadMenuGroup
        While i < menuGroups.Count
            currentGroup = menuGroups.Item(i)
            If (currentGroup.Name.Equals(groupName)) Then
                Return True
            End If
            i += 1
        End While
        Return False
    End Function

    Public Function CreateProfile(profileName As String) As Boolean
        If App Is Nothing Then
            Return False
        End If
        Dim doesThisProfileExist As Boolean = DoesProfileExist(App, profileName)
        If doesThisProfileExist Then
            SetProfileActive(App, profileName)
            AddTempToPreferencePaths(App)
        Else
            CreateProfile(App, profileName)
            AddTempToPreferencePaths(App)
        End If
        SetProfileActive(App, profileName)
        Return True
    End Function


    Public Function SetAutoCADWindowToNormal() As Boolean
        If App Is Nothing Then
            Return False
        End If
        App.WindowState = AcWindowState.acNorm
        Return True
    End Function


    Private Function GetRunningAutoCADInstance() As AcadApplication
        Dim appAcad As AcadApplication = Nothing
        Try
            appAcad = CType(Marshal.GetActiveObject(autocad2021ProgId), AcadApplication)
        Catch ex As Exception
            Debug.WriteLine("Error: " + ex.Message)
        End Try
        App = appAcad
        Return appAcad
    End Function

    Private Function GetExistingMenuGroup(menuGroups As AcadMenuGroups, groupName As String) As AcadMenuGroup

        Dim i As Integer = 0
        Dim currentGroup As AcadMenuGroup
        While i < menuGroups.Count
            currentGroup = menuGroups.Item(i)
            If (currentGroup.Name.Equals(groupName)) Then
                Return currentGroup
            End If
            i += 1
        End While
        Return Nothing
    End Function

    Private Function GetExistingPopupMenu(menuGroup As AcadPopupMenus, menuName As String) As AcadPopupMenu
        Dim i As Integer = 0
        Dim currentMenu As AcadPopupMenu
        While i < menuGroup.Count
            currentMenu = menuGroup.Item(i)
            If (currentMenu.Name.Equals(menuName)) Then
                Return currentMenu
            End If
            i += 1
        End While
        Return Nothing
    End Function


    Private Sub SetProfileActive(appAcad As AcadApplication, profileName As String)
        Dim profiles As AcadPreferencesProfiles = appAcad.Preferences.Profiles
        profiles.ActiveProfile = profileName
    End Sub

    Private Sub CreateProfile(appAcad As AcadApplication, profileName As String)
        Dim profiles As AcadPreferencesProfiles = appAcad.Preferences.Profiles
        profiles.CopyProfile(profiles.ActiveProfile, profileName)
        profiles.ActiveProfile = profileName
    End Sub

    Private Function DoesProfileExist(appAcad As AcadApplication, profileName As String) As Boolean
        Dim profiles As AcadPreferencesProfiles = appAcad.Preferences.Profiles
        Dim pNames As Object = Nothing
        profiles.GetAllProfileNames(pNames)
        Dim profileNames As String() = CType(pNames, String())
        For Each name As String In profileNames
            If name.Equals(profileName) Then
                Return True
            End If
        Next
        Return False
    End Function

    Private Sub AddTempToPreferencePaths(appAcad As AcadApplication)
        Dim trustedPathsString As String = CStr(appAcad.ActiveDocument.GetVariable("TRUSTEDPATHS"))
        Dim tempDirectory As String = System.IO.Path.GetTempPath()
        Dim Directory As String = tempDirectory + ""
        Dim newPaths As List(Of String) = New List(Of String) From {Directory}

        If Not trustedPathsString.Contains(Directory) Then
            AddTrustedPaths(appAcad, newPaths)
        End If
    End Sub

    Private Sub AddTrustedPaths(appAcad As AcadApplication, newPaths As List(Of String))
        Dim trustedPathsString As String = CStr(appAcad.ActiveDocument.GetVariable("TRUSTEDPATHS"))
        Dim oldPaths As List(Of String) = New List(Of String)
        oldPaths = trustedPathsString.Split(CChar(";")).ToList
        Dim newTrustedPathsString As String = trustedPathsString
        For Each newPath As String In newPaths
            Dim pathAlreadyExists As Boolean = trustedPathsString.Contains(newPath)
            If Not pathAlreadyExists Then
                newTrustedPathsString = newPath + ";" + newTrustedPathsString
            End If
        Next
        appAcad.ActiveDocument.SetVariable("TRUSTEDPATHS", newTrustedPathsString)
    End Sub
End Class
