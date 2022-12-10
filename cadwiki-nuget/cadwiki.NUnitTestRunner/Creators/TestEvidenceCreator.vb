Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Windows.Automation
Imports System.Windows.Automation.AutomationElement
Imports cadwiki.NUnitTestRunner.Results
Imports cadwiki.NUnitTestRunner.TestEvidence
Imports Microsoft.Test.Input
Imports Image = cadwiki.NUnitTestRunner.TestEvidence.Image

Namespace Creators

    Public Class TestEvidenceCreator

        Private Shared _evidenceForCurrentlyExecutingTest As Evidence
        Private Shared _localFolderCache As String = IO.Path.GetTempPath + "cadwiki.NUnitTestRunner"
        Private Shared _localScreenShotCache As String = _localFolderCache + "\" + "screenshots"
        Private Shared _pdfFileReport As String = "AutomatedTestEvidence.pdf"
        Private Shared _jsonFileResults As String = "AutomatedTestEvidence.json"

        Public Sub New()
            If (_evidenceForCurrentlyExecutingTest Is Nothing) Then
                _evidenceForCurrentlyExecutingTest = New Evidence()
            End If
            If (Not IO.Directory.Exists(_localFolderCache)) Then
                IO.Directory.CreateDirectory(_localFolderCache)
            End If
            If (Not IO.Directory.Exists(_localScreenShotCache)) Then
                IO.Directory.CreateDirectory(_localScreenShotCache)
            End If
        End Sub

        Public Function CreatePdf(suiteResult As ObservableTestSuiteResults) As String
            Dim pdfCreator As New PdfCreator(GetNewPdfReportFilePath(suiteResult))
            pdfCreator.AddTitlePage(suiteResult)
            For Each testResult As TestResult In suiteResult.TestResults
                pdfCreator.AddTestPage(testResult)
            Next
            pdfCreator.Save()
            Return pdfCreator.PdfFilePath
        End Function

        Public Function WriteTestSuiteResultsToFile(suiteResult As ObservableTestSuiteResults, jsonString As String) As String
            Dim jsonFilePath As String
            If (Not String.IsNullOrEmpty(suiteResult.TestSuiteName)) Then
                jsonFilePath = _localFolderCache + "\" + suiteResult.TestSuiteName + "-" + _jsonFileResults
            Else
                jsonFilePath = _localFolderCache + "\" + _jsonFileResults
            End If
            Dim jsonFile As String = NetUtils.Paths.GetUniqueFilePath(jsonFilePath)
            File.WriteAllText(jsonFile, jsonString)
            Return jsonFile
        End Function

        Public Function GetNewPdfReportFilePath(suiteResult As ObservableTestSuiteResults) As String
            Dim reportFilePath As String
            If (Not String.IsNullOrEmpty(suiteResult.TestSuiteName)) Then
                reportFilePath = _localFolderCache + "\" + suiteResult.TestSuiteName + "-" + _pdfFileReport
            Else
                reportFilePath = _localFolderCache + "\" + _pdfFileReport
            End If
            reportFilePath = NetUtils.Paths.GetUniqueFilePath(reportFilePath)
            Return reportFilePath
        End Function

        Public Function GetFolderCache() As String
            Return _localFolderCache
        End Function

        Public Function GetScreenshotCache() As String
            Return _localScreenShotCache
        End Function

        Public Sub SetEvidenceForCurrentTest(testEvidence As Evidence)
            _evidenceForCurrentlyExecutingTest = testEvidence
        End Sub

        Public Function GetEvidenceForCurrentTest() As Evidence
            Return _evidenceForCurrentlyExecutingTest
        End Function

        Public Function ProcessesGetHandleFromUiTitle(ByVal windowTitle As String) As IntPtr
            Dim hWnd As IntPtr = IntPtr.Zero

            For Each pList As Process In Process.GetProcesses()

                If pList.MainWindowTitle.Contains(windowTitle) Then
                    hWnd = pList.MainWindowHandle
                End If
            Next

            Return hWnd
        End Function

        Public Shared Sub PrintWindowToImage(ByVal windowIntPtr As IntPtr, ByVal screenshotPath As String, ByVal format As ImageFormat)
            Dim screenshot As Bitmap = PrintWindowWithWinAPI(windowIntPtr)
            screenshot.Save(screenshotPath, format)
        End Sub


        Public Function MicrosoftTestClickUiControl(windowIntPtr As IntPtr, automationId As String) As Boolean
            Return MicrosoftTestClickUiControlByAutomationId(windowIntPtr, automationId)
        End Function

        Public Function MicrosoftTestClickUiControlByAutomationId(windowIntPtr As IntPtr, automationId As String) As Boolean

            Dim element As AutomationElement = GetElementByAutomationId(windowIntPtr, automationId)
            If (element Is Nothing) Then
                Return False
            End If
            Dim clickableSystemDrawingPoint As System.Drawing.Point =
                GetClickableSystemDrawingPointFromElement(element)

            Return MicrosoftTestClickPoint(clickableSystemDrawingPoint)
        End Function

        Public Function MicrosoftTestClickUiControlByName(windowIntPtr As IntPtr, name As String) As Boolean

            Dim element As AutomationElement = GetElementByControlName(windowIntPtr, name)
            If (element Is Nothing) Then
                Return False
            End If
            Dim clickableSystemDrawingPoint As System.Drawing.Point =
                GetClickableSystemDrawingPointFromElement(element)

            Return MicrosoftTestClickPoint(clickableSystemDrawingPoint)
        End Function

        Public Function GetControlCollection(windowIntPtr As IntPtr) As AutomationElementCollection
            Dim root = AutomationElement.FromHandle(windowIntPtr)
            Dim elementCollection As AutomationElementCollection = root.FindAll(TreeScope.Subtree, System.Windows.Automation.Condition.TrueCondition)
            Return elementCollection
        End Function

        Public Sub TakeJpegScreenshot(windowIntPtr As IntPtr, title As String)
            Dim fileName As String = title + ".jpg"
            fileName = NetUtils.Paths.ReplaceAllillegalCharsForWindowsOSInFileName(fileName, "-")
            Dim screenshotPath As String = GetScreenshotCache() + "\" + fileName
            screenshotPath = NetUtils.Paths.GetUniqueFilePath(screenshotPath)
            Dim format As ImageFormat = ImageFormat.Jpeg
            TestEvidenceCreator.PrintWindowToImage(windowIntPtr, screenshotPath, format)
            Dim image As New Image()
            image.Title = title
            image.FilePath = screenshotPath
            _evidenceForCurrentlyExecutingTest.Images.Add(image)
            Me.SetEvidenceForCurrentTest(_evidenceForCurrentlyExecutingTest)
        End Sub

        Private Shared Function PrintWindowWithWinAPI(ByVal hwnd As IntPtr) As Bitmap
            Dim rc As WinAPI.RECT
            WinAPI.Stubs.GetWindowRect(hwnd, rc)
            Dim bmp As Bitmap = New Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb)
            Dim gfxBmp As Graphics = Graphics.FromImage(bmp)
            Dim hdcBitmap As IntPtr = gfxBmp.GetHdc()
            WinAPI.Stubs.PrintWindow(hwnd, hdcBitmap, 0)
            gfxBmp.ReleaseHdc(hdcBitmap)
            gfxBmp.Dispose()
            Return bmp
        End Function



        Private Function GetElementByAutomationId(ByVal windowIntPtr As IntPtr, ByVal automationIdToFind As String) As AutomationElement
            Dim root = AutomationElement.FromHandle(windowIntPtr)
            Dim elementCollection As AutomationElementCollection = root.FindAll(TreeScope.Subtree, System.Windows.Automation.Condition.TrueCondition)

            For Each element As AutomationElement In elementCollection
                Dim current As AutomationElementInformation = element.Current
                Dim controlType As ControlType = current.ControlType
                Dim name As String = current.Name
                Dim automationId As String = current.AutomationId
                If automationId.Equals(automationIdToFind) Then
                    Return element
                End If
            Next

            Return Nothing
        End Function

        Private Function GetElementByControlName(ByVal windowIntPtr As IntPtr, ByVal controlNameToFind As String) As AutomationElement
            Dim root = AutomationElement.FromHandle(windowIntPtr)
            Dim elementCollection As AutomationElementCollection = root.FindAll(TreeScope.Subtree, System.Windows.Automation.Condition.TrueCondition)

            For Each element As AutomationElement In elementCollection
                Dim current As AutomationElementInformation = element.Current
                Dim controlType As ControlType = current.ControlType
                Dim name As String = current.Name
                Dim automationId As String = current.AutomationId
                If name.Equals(controlNameToFind) Then
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

