using System;

namespace cadwiki.NUnitTestRunner.HTML_Models
{
    public class TestSuiteReport
    {
        public string Title;
        public string BannerImagePath;
        public string TabName;
        public string PassPercentage;
        public string DateString;
        public Results.ObservableTestSuiteResults TestSuiteResults;

        public string HeaderBackgroundColor = "";


        public TestSuiteReport(Results.ObservableTestSuiteResults results)
        {
            Title = results.TestSuiteName;
            TabName = Title;
            PassPercentage = (results.PassedTests / (double)results.TotalTests * 1.0d * 100d).ToString("0.00") + "%";
            DateString = DateTimeOffset.Now.ToString();
            TestSuiteResults = results;
        }

    }

    public class HexColors
    {
        public string DarkBlue = "#002554";
        public string White = "#ffffff";
        public string LightBlue = "#0093db";
    }

}