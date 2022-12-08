Imports System.Runtime.InteropServices
Imports System.Text
Imports HWND = System.IntPtr

Namespace WinAPI
    Public Class Stubs
        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
        Public Shared Function GetWindowRect(ByVal hWnd As IntPtr, <Out> ByRef lpRect As RECT) As Boolean
        End Function

        <DllImport("user32.dll")>
        Public Shared Function PrintWindow(ByVal hWnd As IntPtr, ByVal hdcBlt As IntPtr, ByVal nFlags As Integer) As Boolean
        End Function



        <DllImport("user32.dll")>
        Public Shared Function EnumWindows(ByVal enumFunc As Delegates.EnumWindowsProc, ByVal lParam As Integer) As Boolean
        End Function

        <DllImport("user32.dll")>
        Public Shared Function GetWindowText(ByVal hWnd As HWND, ByVal title As StringBuilder, ByVal size As Integer) As Integer
        End Function

        <DllImport("user32.dll")>
        Public Shared Function GetWindowTextLength(ByVal hWnd As HWND) As Integer
        End Function

        <DllImport("user32.dll")>
        Public Shared Function IsWindowVisible(ByVal hWnd As HWND) As Boolean
        End Function


        <DllImport("user32.dll")>
        Public Shared Function GetShellWindow() As HWND
        End Function

        <DllImport("user32.dll", EntryPoint:="FindWindowEx", CharSet:=CharSet.Auto)>
        Public Shared Function FindWindowEx(ByVal hwndParent As HWND, ByVal hwndChildAfter As HWND, ByVal lpszClass As String, ByVal lpszWindow As String) As HWND
        End Function

        <DllImport("user32.dll")>
        Public Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As Integer, ByVal lParam As IntPtr) As Integer
        End Function

    End Class
End Namespace

