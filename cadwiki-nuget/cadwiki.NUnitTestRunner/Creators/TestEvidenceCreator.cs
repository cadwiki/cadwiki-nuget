using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Automation;
using static System.Windows.Automation.AutomationElement;
using cadwiki.NUnitTestRunner.Results;
using cadwiki.NUnitTestRunner.TestEvidence;
using Image = cadwiki.NUnitTestRunner.TestEvidence.Image;
using Microsoft.Test.Input;

namespace cadwiki.NUnitTestRunner.Creators
{

    public class TestEvidenceCreator
    {

        private static Evidence _evidenceForCurrentlyExecutingTest;
        private static string _localFolderCache = Path.GetTempPath() + "cadwiki.NUnitTestRunner";
        private static string _localScreenShotCache = _localFolderCache + @"\" + "screenshots";
        private static string _pdfFileReport = "AutomatedTestEvidence.pdf";
        private static string _jsonFileResults = "AutomatedTestEvidence.json";
        private static string _htmlFileReport = "AutomatedTestEvidence.html";

        public TestEvidenceCreator()
        {
            if (_evidenceForCurrentlyExecutingTest is null)
            {
                _evidenceForCurrentlyExecutingTest = new Evidence();
            }
            if (!Directory.Exists(_localFolderCache))
            {
                Directory.CreateDirectory(_localFolderCache);
            }
            if (!Directory.Exists(_localScreenShotCache))
            {
                Directory.CreateDirectory(_localScreenShotCache);
            }
        }

        public string CreatePdf(ObservableTestSuiteResults suiteResult)
        {
            var pdfCreator = new PdfCreator(GetNewPdfReportFilePath(suiteResult));
            pdfCreator.AddTitlePage(suiteResult);
            foreach (TestResult testResult in suiteResult.TestResults)
                pdfCreator.AddTestPage(testResult);
            pdfCreator.Save();
            return pdfCreator.PdfFilePath;
        }

        public void CreateHtmlReport(ObservableTestSuiteResults suiteResult)
        {
            var htmlCreator = new HtmlCreator();
            string htmlReportFilePath = GetNewHtmlReportFilePath(suiteResult);
            string reportFolder = Path.GetDirectoryName(htmlReportFilePath);

            var model = new HTML_Models.TestSuiteReport(suiteResult);
            model.TestSuiteResults.SetImagePathsToRelative(reportFolder);
            htmlCreator.ParameterizeReportTemplate(model);
            htmlCreator.SaveReportToFile(htmlReportFilePath);
        }

        public string WriteTestSuiteResultsToFile(ObservableTestSuiteResults suiteResult, string jsonString)
        {
            string jsonFilePath;
            if (!string.IsNullOrEmpty(suiteResult.TestSuiteName))
            {
                jsonFilePath = _localFolderCache + @"\" + suiteResult.TestSuiteName + "-" + _jsonFileResults;
            }
            else
            {
                jsonFilePath = _localFolderCache + @"\" + _jsonFileResults;
            }
            string jsonFile = NetUtils.Paths.GetUniqueFilePath(jsonFilePath);
            File.WriteAllText(jsonFile, jsonString);
            return jsonFile;
        }

        public string GetNewPdfReportFilePath(ObservableTestSuiteResults suiteResult)
        {
            string reportFilePath;
            if (!string.IsNullOrEmpty(suiteResult.TestSuiteName))
            {
                reportFilePath = _localFolderCache + @"\" + suiteResult.TestSuiteName + "-" + _pdfFileReport;
            }
            else
            {
                reportFilePath = _localFolderCache + @"\" + _pdfFileReport;
            }
            reportFilePath = NetUtils.Paths.GetUniqueFilePath(reportFilePath);
            return reportFilePath;
        }

        public string GetNewHtmlReportFilePath(ObservableTestSuiteResults suiteResult)
        {
            string reportFilePath;
            if (!string.IsNullOrEmpty(suiteResult.TestSuiteName))
            {
                reportFilePath = _localFolderCache + @"\" + suiteResult.TestSuiteName + "-" + _htmlFileReport;
            }
            else
            {
                reportFilePath = _localFolderCache + @"\" + _htmlFileReport;
            }
            reportFilePath = NetUtils.Paths.GetUniqueFilePath(reportFilePath);
            return reportFilePath;
        }

        public string GetFolderCache()
        {
            return _localFolderCache;
        }

        public string GetScreenshotCache()
        {
            return _localScreenShotCache;
        }

        public void SetEvidenceForCurrentTest(Evidence testEvidence)
        {
            _evidenceForCurrentlyExecutingTest = testEvidence;
        }

        public Evidence GetEvidenceForCurrentTest()
        {
            return _evidenceForCurrentlyExecutingTest;
        }

        public IntPtr ProcessesGetHandleFromUiTitle(string windowTitle)
        {
            var hWnd = IntPtr.Zero;

            foreach (Process pList in Process.GetProcesses())
            {

                if (pList.MainWindowTitle.Contains(windowTitle))
                {
                    hWnd = pList.MainWindowHandle;
                }
            }

            return hWnd;
        }

        public static void PrintWindowToImage(IntPtr windowIntPtr, string screenshotPath, ImageFormat format)
        {
            var screenshot = PrintWindowWithWinAPI(windowIntPtr);
            screenshot.Save(screenshotPath, format);
        }


        public bool MicrosoftTestClickUiControl(IntPtr windowIntPtr, string automationId)
        {
            return MicrosoftTestClickUiControlByAutomationId(windowIntPtr, automationId);
        }

        public bool MicrosoftTestClickUiControlByAutomationId(IntPtr windowIntPtr, string automationId)
        {

            var element = GetElementByAutomationId(windowIntPtr, automationId);
            if (element is null)
            {
                return false;
            }
            var clickableSystemDrawingPoint = GetClickableSystemDrawingPointFromElement(element);

            return MicrosoftTestClickPoint(clickableSystemDrawingPoint);
        }

        public bool MicrosoftTestClickUiControlByName(IntPtr windowIntPtr, string name)
        {

            var element = GetElementByControlName(windowIntPtr, name);
            if (element is null)
            {
                return false;
            }
            var clickableSystemDrawingPoint = GetClickableSystemDrawingPointFromElement(element);

            return MicrosoftTestClickPoint(clickableSystemDrawingPoint);
        }

        public AutomationElementCollection GetControlCollection(IntPtr windowIntPtr)
        {
            var root = FromHandle(windowIntPtr);
            var elementCollection = root.FindAll(TreeScope.Subtree, Condition.TrueCondition);
            return elementCollection;
        }

        public void TakeJpegScreenshot(IntPtr windowIntPtr, string title)
        {
            string fileName = title + ".jpg";
            fileName = NetUtils.Paths.ReplaceAllillegalCharsForWindowsOSInFileName(fileName, "-");
            string screenshotPath = GetScreenshotCache() + @"\" + fileName;
            screenshotPath = NetUtils.Paths.GetUniqueFilePath(screenshotPath);
            var format = ImageFormat.Jpeg;
            PrintWindowToImage(windowIntPtr, screenshotPath, format);
            var image = new Image();
            image.Title = title;
            image.FilePath = screenshotPath;
            _evidenceForCurrentlyExecutingTest.Images.Add(image);
            SetEvidenceForCurrentTest(_evidenceForCurrentlyExecutingTest);
        }

        private static Bitmap PrintWindowWithWinAPI(IntPtr hwnd)
        {
            WinAPI.RECT rc;
            WinAPI.Stubs.GetWindowRect(hwnd, out rc);
            var bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
            var gfxBmp = Graphics.FromImage(bmp);
            var hdcBitmap = gfxBmp.GetHdc();
            WinAPI.Stubs.PrintWindow(hwnd, hdcBitmap, 0);
            gfxBmp.ReleaseHdc(hdcBitmap);
            gfxBmp.Dispose();
            return bmp;
        }



        private AutomationElement GetElementByAutomationId(IntPtr windowIntPtr, string automationIdToFind)
        {
            var root = FromHandle(windowIntPtr);
            var elementCollection = root.FindAll(TreeScope.Subtree, Condition.TrueCondition);

            foreach (AutomationElement element in elementCollection)
            {
                var current = element.Current;
                var controlType = current.ControlType;
                string name = current.Name;
                string automationId = current.AutomationId;
                if (automationId.Equals(automationIdToFind))
                {
                    return element;
                }
            }

            return null;
        }

        private AutomationElement GetElementByControlName(IntPtr windowIntPtr, string controlNameToFind)
        {
            var root = FromHandle(windowIntPtr);
            var elementCollection = root.FindAll(TreeScope.Subtree, Condition.TrueCondition);

            foreach (AutomationElement element in elementCollection)
            {
                var current = element.Current;
                var controlType = current.ControlType;
                string name = current.Name;
                string automationId = current.AutomationId;
                if (name.Equals(controlNameToFind))
                {
                    return element;
                }
            }

            return null;
        }

        private Point GetClickableSystemDrawingPointFromElement(AutomationElement element)
        {
            var windowsPoint = element.GetClickablePoint();
            var drawingPoint = new Point((int)Math.Round(windowsPoint.X), (int)Math.Round(windowsPoint.Y));
            return drawingPoint;
        }

        private bool MicrosoftTestClickPoint(Point clickableSystemDrawingPoint)
        {
            Mouse.MoveTo(clickableSystemDrawingPoint);
            Mouse.Click(MouseButton.Left);
            return true;
        }

    }












}