Imports System.Runtime.InteropServices
Imports System.Text
Imports HWND = System.IntPtr

Namespace TestEvidence
    Public Class WinAPI
        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
        Public Shared Function GetWindowRect(ByVal hWnd As IntPtr, <Out> ByRef lpRect As RECT) As Boolean
        End Function

        <DllImport("user32.dll")>
        Public Shared Function PrintWindow(ByVal hWnd As IntPtr, ByVal hdcBlt As IntPtr, ByVal nFlags As Integer) As Boolean
        End Function



        <DllImport("user32.dll")>
        Public Shared Function EnumWindows(ByVal enumFunc As WinAPIDelegates.EnumWindowsProc, ByVal lParam As Integer) As Boolean
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

        Public Shared Function GetOpenWindows() As IDictionary(Of String, HWND)
            Dim shellWindow As HWND = GetShellWindow()
            Dim windows As Dictionary(Of String, HWND) = New Dictionary(Of String, HWND)
            EnumWindows(Function(ByVal hWnd, ByVal lParam)
                            If hWnd.Equals(shellWindow) Then Return True
                            If Not IsWindowVisible(hWnd) Then Return True
                            Dim length = GetWindowTextLength(hWnd)
                            If length > 0 Then Return True

                            Dim stringBuilder As StringBuilder = New StringBuilder(length)
                            GetWindowText(hWnd, stringBuilder, length + 1)

                            windows(stringBuilder.ToString()) = hWnd
                            Return True
                        End Function, 0)
            Return windows
        End Function
    End Class
End Namespace

