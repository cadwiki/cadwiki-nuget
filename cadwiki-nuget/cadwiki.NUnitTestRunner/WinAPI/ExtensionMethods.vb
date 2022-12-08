
Imports System.Text
Imports HWND = System.IntPtr

Namespace WinAPI
    Public Class ExtensionMethods

        Public Shared Function GetOpenWindow(title As String) As HWND
            Dim titleToHandle As Dictionary(Of String, HWND) = GetOpenWindows()
            Dim windowHandle As HWND
            titleToHandle.TryGetValue(title, windowHandle)
            Return windowHandle
        End Function

        Public Shared Function GetOpenWindows() As IDictionary(Of String, HWND)
            Dim shellWindow As HWND = Stubs.GetShellWindow()
            Dim windows As Dictionary(Of String, HWND) = New Dictionary(Of String, HWND)
            Stubs.EnumWindows(Function(ByVal hWnd, ByVal lParam)
                                  If hWnd.Equals(shellWindow) Then Return True
                                  If Not Stubs.IsWindowVisible(hWnd) Then Return True
                                  Dim length = Stubs.GetWindowTextLength(hWnd)
                                  If length = 0 Then Return True

                                  Dim stringBuilder As StringBuilder = New StringBuilder(length)
                                  Stubs.GetWindowText(hWnd, stringBuilder, length + 1)

                                  windows(stringBuilder.ToString()) = hWnd
                                  Return True
                              End Function, 0)
            Return windows
        End Function

        Public Shared Function FindSubControl(parentHwnd As HWND, controlClass As String, conrolDisplayText As String)
            Return Stubs.FindWindowEx(parentHwnd, HWND.Zero, controlClass, conrolDisplayText)
        End Function

        Public Shared Sub CloseWindow(hWnd As HWND)
            Stubs.SendMessage(hWnd, Constants.WM_SYSCOMMAND, Constants.SC_CLOSE, 0)
        End Sub
    End Class
End Namespace
