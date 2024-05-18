using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using cadwiki.NetUtils;
using cadwiki.NUnitTestRunner;
using cadwiki.NUnitTestRunner.Creators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{

    [TestClass()]
    public class TestsForHtmlCreator
    {



        [TestMethod()]
        public void Test_CreateReport_ShouldPass()
        {

            var testStringsType = typeof(TestStrings);
            Type[] allTypes = new[] { testStringsType };

            var testSuiteResults = new cadwiki.NUnitTestRunner.Results.ObservableTestSuiteResults();
            testSuiteResults.TestSuiteName = "cadwiki Automation Tests";
            var driver = new cadwiki.NUnitTestRunner.Ui.WpfDriver(ref testSuiteResults, allTypes);
            driver.ExecuteTestsAsync();

            var testEvidenceCreator = new TestEvidenceCreator();

            string htmlReportFilePath = testEvidenceCreator.GetNewHtmlReportFilePath(testSuiteResults);
            var htmlCreator = new HtmlCreator();
            var model = new cadwiki.NUnitTestRunner.HTML_Models.TestSuiteReport(testSuiteResults);

            Bitmap bitMap = cadwiki.FileStore.ResourceIcons._500x500_cadwiki_v1;
            BitmapImage bitMapImage = Bitmaps.BitMapToBitmapImage(bitMap);
            string reportFolder = System.IO.Path.GetDirectoryName(htmlReportFilePath);
            string imageFile = reportFolder + @"\" + "test.bmp";
            bitMap.Save(imageFile);
            model.BannerImagePath = "./test.bmp";

            testSuiteResults.SetImagePathsToRelative(reportFolder);
            htmlCreator.ParameterizeReportTemplate(model);


            htmlCreator.SaveReportToFile(htmlReportFilePath);

        }

    }
}