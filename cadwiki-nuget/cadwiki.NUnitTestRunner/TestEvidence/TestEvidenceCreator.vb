Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Windows.Automation
Imports System.Windows.Automation.AutomationElement
Imports Microsoft.Test.Input

Namespace TestEvidence

    Public Class TestEvidenceCreator

        Private Shared _evidenceForCurrentlyExecutingTest As Evidence
        Private Shared _localFolderCache As String = IO.Path.GetTempPath + "cadwiki.NUnitTestRunner"
        Private Shared _localScreenShotCache As String = _localFolderCache + "\" + "screenshots"
        Private Shared _pdfFileReport As String = "AutomatedTestEvidence.pdf"

        Public Sub New()
            If (Not IO.Directory.Exists(_localFolderCache)) Then
                IO.Directory.CreateDirectory(_localFolderCache)
            End If
            If (Not IO.Directory.Exists(_localScreenShotCache)) Then
                IO.Directory.CreateDirectory(_localScreenShotCache)
            End If
        End Sub

        Public Function GetNewPdfReportFilePath() As String
            Dim reportFilePath As String = _localFolderCache + "\" + _pdfFileReport
            reportFilePath = NetUtils.Paths.GetUniqueFilePath(reportFilePath)
            Return reportFilePath
        End Function

        Public Function GetFolderCache() As String
            Return _localFolderCache
        End Function

        Public Function GetScreenshotCache() As String
            Return _localScreenShotCache
        End Function

        Public Function SetEvidenceForCurrentTest(testEvidence As Evidence)
            _evidenceForCurrentlyExecutingTest = testEvidence
        End Function

        Public Function GetEvidenceForCurrentTest() As Evidence
            Return _evidenceForCurrentlyExecutingTest
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

        Public Shared Sub PrintWindowToImage(ByVal windowIntPtr As IntPtr, ByVal screenshotPath As String, ByVal format As ImageFormat)
            Dim screenshot As Bitmap = PrintWindowWithWinAPI(windowIntPtr)
            screenshot.Save(screenshotPath, format)
        End Sub


        Public Function MicrosoftTestClickUiControl(windowIntPtr As IntPtr, controlName As String) As Boolean

            Dim element As AutomationElement = GetElementByControlName(windowIntPtr, controlName)
            If (element Is Nothing) Then
                Return False
            End If
            Dim clickableSystemDrawingPoint As System.Drawing.Point =
                GetClickableSystemDrawingPointFromElement(element)

            Return MicrosoftTestClickPoint(clickableSystemDrawingPoint)
        End Function



        Private Shared Function PrintWindowWithWinAPI(ByVal hwnd As IntPtr) As Bitmap
            Dim rc As RECT
            WinAPI.GetWindowRect(hwnd, rc)
            Dim bmp As Bitmap = New Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb)
            Dim gfxBmp As Graphics = Graphics.FromImage(bmp)
            Dim hdcBitmap As IntPtr = gfxBmp.GetHdc()
            WinAPI.PrintWindow(hwnd, hdcBitmap, 0)
            gfxBmp.ReleaseHdc(hdcBitmap)
            gfxBmp.Dispose()
            Return bmp
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

