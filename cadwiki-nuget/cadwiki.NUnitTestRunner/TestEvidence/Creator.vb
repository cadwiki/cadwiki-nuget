Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.Windows.Automation
Imports System.Windows.Automation.AutomationElement
Imports Microsoft.Test.Input

Namespace TestEvidence
    Public Class Creator


        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
        Private Shared Function GetWindowRect(ByVal hWnd As IntPtr, <Out> ByRef lpRect As RECT) As Boolean
        End Function

        <DllImport("user32.dll")>
        Private Shared Function PrintWindow(ByVal hWnd As IntPtr, ByVal hdcBlt As IntPtr, ByVal nFlags As Integer) As Boolean
        End Function


        Public Shared Sub PrintWindowToImage(ByVal windowIntPtr As IntPtr, ByVal screenshotPath As String, ByVal format As ImageFormat)
            Dim screenshot As Bitmap = PrintWindow(windowIntPtr)
            screenshot.Save(screenshotPath, format)
        End Sub

        Public Shared Function PrintWindow(ByVal hwnd As IntPtr) As Bitmap
            Dim rc As RECT
            GetWindowRect(hwnd, rc)
            Dim bmp As Bitmap = New Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb)
            Dim gfxBmp As Graphics = Graphics.FromImage(bmp)
            Dim hdcBitmap As IntPtr = gfxBmp.GetHdc()
            PrintWindow(hwnd, hdcBitmap, 0)
            gfxBmp.ReleaseHdc(hdcBitmap)
            gfxBmp.Dispose()
            Return bmp
        End Function

        Public Function ProcessesGetHandleFromUiTitle(ByVal wName As String) As IntPtr
            Dim hWnd As IntPtr = IntPtr.Zero

            For Each pList As Process In Process.GetProcesses()

                If pList.MainWindowTitle.Contains(wName) Then
                    hWnd = pList.MainWindowHandle
                End If
            Next

            Return hWnd
        End Function


        Public Function MicrosoftTestClickUiControl(windowIntPtr As IntPtr, controlName As String) As Boolean

            Dim element As AutomationElement = GetElementByControlName(windowIntPtr, controlName)
            If (element Is Nothing) Then
                Return False
            End If
            Dim clickableSystemDrawingPoint As System.Drawing.Point =
                GetClickableSystemDrawingPointFromElement(element)

            Return MicrosoftTestClickPoint(clickableSystemDrawingPoint)
        End Function

        Private Function GetElementByControlName(ByVal windowIntPtr As IntPtr, ByVal controlNameToFind As String) As AutomationElement
            Dim root = AutomationElement.FromHandle(windowIntPtr)
            Dim elementCollection As AutomationElementCollection = root.FindAll(TreeScope.Subtree, System.Windows.Automation.Condition.TrueCondition)

            For Each element As AutomationElement In elementCollection
                Dim current As AutomationElementInformation = element.Current
                Dim controlType As ControlType = current.ControlType
                Dim controlText As String = current.Name
                Dim controlName As String = current.AutomationId

                If controlName.Equals(controlNameToFind) Then
                    Return element
                End If
            Next

            Return Nothing
        End Function


        Private Function GetClickableSystemDrawingPointFromElement(ByVal element As AutomationElement) As System.Drawing.Point
            Dim windowsPoint As System.Windows.Point = element.GetClickablePoint()
            Dim drawingPoint As System.Drawing.Point = New System.Drawing.Point(CInt(windowsPoint.X), CInt(windowsPoint.Y))
            Return drawingPoint
        End Function

        Private Function MicrosoftTestClickPoint(ByVal clickableSystemDrawingPoint As System.Drawing.Point) As Boolean
            Microsoft.Test.Input.Mouse.MoveTo(clickableSystemDrawingPoint)
            Microsoft.Test.Input.Mouse.Click(MouseButton.Left)
            Return True
        End Function

    End Class












End Namespace

