Imports System.Runtime.InteropServices
Imports HWND = System.IntPtr

Namespace WinAPI
    Public Class Delegates
        Public Delegate Function EnumWindowsProc(ByVal hWnd As HWND, ByVal lParam As Integer) As Boolean

    End Class
End Namespace
