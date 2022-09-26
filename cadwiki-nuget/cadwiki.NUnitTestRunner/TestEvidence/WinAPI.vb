Imports System.Runtime.InteropServices

Namespace TestEvidence
    Public Class WinAPI
        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
        Public Shared Function GetWindowRect(ByVal hWnd As IntPtr, <Out> ByRef lpRect As RECT) As Boolean
        End Function

        <DllImport("user32.dll")>
        Public Shared Function PrintWindow(ByVal hWnd As IntPtr, ByVal hdcBlt As IntPtr, ByVal nFlags As Integer) As Boolean
        End Function

    End Class
End Namespace

