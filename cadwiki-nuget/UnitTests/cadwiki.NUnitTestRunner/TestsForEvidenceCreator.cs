using System;
using System.Linq;
using cadwiki.NUnitTestRunner.Creators;
using cadwiki.NUnitTestRunner.TestEvidence;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{

    [TestClass()]
    public class TestsForEvidenceCreator
    {



        [TestMethod()]
        public void Test_CreatePDF_ShouldPass()
        {
            var testStringsType = typeof(TestStrings);
            Type[] allTypes = new[] { testStringsType };
            var results = new cadwiki.NUnitTestRunner.Results.ObservableTestSuiteResults();
            results.TestSuiteName = "Test_CreatePDF_ShouldPass";
            var driver = new cadwiki.NUnitTestRunner.Ui.WpfDriver(ref results, allTypes);
            var testEvidenceCreator = new TestEvidenceCreator();
            testEvidenceCreator.CreatePdf(results);
        }

        [TestMethod()]
        public void Test_PrintWindow_ShouldCreateJpeg()
        {

            var testStringsType = typeof(TestStrings);
            Type[] allTypes = new[] { testStringsType };

            var results = new cadwiki.NUnitTestRunner.Results.ObservableTestSuiteResults();
            results.TestSuiteName = "Test_PrintWindow_ShouldCreateJpeg";
            var driver = new cadwiki.NUnitTestRunner.Ui.WpfDriver(ref results, allTypes);
            cadwiki.NUnitTestRunner.UI.WindowTestRunner window = driver.GetWindow();
            window.Show();
            driver.ExecuteTestsAsync();

            var testEvidenceCreator = new TestEvidenceCreator();
            var windowIntPtr = testEvidenceCreator.ProcessesGetHandleFromUiTitle("N Unit Test Runner");

            var evidence = new Evidence();
            testEvidenceCreator.TakeJpegScreenshot(windowIntPtr, "Title");
            evidence = testEvidenceCreator.GetEvidenceForCurrentTest();
            window.Close();
            Assert.IsTrue(System.IO.File.Exists(evidence.Images.FirstOrDefault().FilePath), "Failed to create screenshot");
        }


        [TestMethod()]
        public void Test_CreatePdfWithScreenshots_ShouldCreatePdf()
        {

            var testStringsType = typeof(TestStrings);
            Type[] allTypes = new[] { testStringsType };

            var results = new cadwiki.NUnitTestRunner.Results.ObservableTestSuiteResults();
            results.TestSuiteName = "Test_CreatePdf_ShouldCreatePdf";
            var driver = new cadwiki.NUnitTestRunner.Ui.WpfDriver(ref results, allTypes);
            cadwiki.NUnitTestRunner.UI.WindowTestRunner window = driver.GetWindow();
            window.Show();
            driver.ExecuteTestsAsync();

            var testEvidenceCreator = new TestEvidenceCreator();


            var windowIntPtr = testEvidenceCreator.ProcessesGetHandleFromUiTitle("N Unit Test Runner");

            var evidence = new Evidence();
            testEvidenceCreator.TakeJpegScreenshot(windowIntPtr, "Title");
            // add evidence to the first test
            evidence = testEvidenceCreator.GetEvidenceForCurrentTest();
            results.TestResults[0].Evidence = evidence;

            string filePath = evidence.Images[0].FilePath;
            string pdf = testEvidenceCreator.CreatePdf(results);
            testEvidenceCreator.CreateHtmlReport(results);
            window.Close();

            Assert.IsTrue(System.IO.File.Exists(pdf), "Failed to create pdf");
        }

    }
}