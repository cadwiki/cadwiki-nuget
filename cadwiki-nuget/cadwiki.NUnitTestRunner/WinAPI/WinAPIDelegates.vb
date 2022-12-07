Imports System.Runtime.InteropServices
Imports HWND = System.IntPtr

Namespace TestEvidence
    Public Class WinAPIDelegates
        Public Delegate Function EnumWindowsProc(ByVal hWnd As HWND, ByVal lParam As Integer) As Boolean

    End Class
End Namespace
