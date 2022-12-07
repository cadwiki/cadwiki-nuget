
Imports System.Text
Imports HWND = System.IntPtr

Namespace WinAPI
    Public Class ExtensionMethods
        Public Shared Function GetOpenWindows() As IDictionary(Of String, HWND)
            Dim shellWindow As HWND = Stubs.GetShellWindow()
            Dim windows As Dictionary(Of String, HWND) = New Dictionary(Of String, HWND)
            Stubs.EnumWindows(Function(ByVal hWnd, ByVal lParam)
                                  If hWnd.Equals(shellWindow) Then Return True
                                  If Not Stubs.IsWindowVisible(hWnd) Then Return True
                                  Dim length = Stubs.GetWindowTextLength(hWnd)
                                  If length > 0 Then Return True

                                  Dim stringBuilder As StringBuilder = New StringBuilder(length)
                                  Stubs.GetWindowText(hWnd, stringBuilder, length + 1)

                                  windows(stringBuilder.ToString()) = hWnd
                                  Return True
                              End Function, 0)
            Return windows
        End Function
    End Class
End Namespace
