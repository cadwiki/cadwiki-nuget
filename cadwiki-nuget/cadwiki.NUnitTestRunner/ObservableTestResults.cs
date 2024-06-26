using System;
using System.Collections.Generic;
using cadwiki.NUnitTestRunner.TestEvidence;
using Newtonsoft.Json;

namespace cadwiki.NUnitTestRunner.Results
{
    public class TestResult
    {
        public string TestName = "";
        public bool Passed;
        public string ExceptionMessage = "";
        public List<string> StackTrace = new List<string>();
        public Evidence Evidence = new Evidence();

        public List<string> ToStringList()
        {
            var strList = new List<string>();
            strList.Add("TestName: " + TestName);
            strList.Add("Passed: " + Passed.ToString());
            strList.Add("ExceptionMessage: " + ExceptionMessage);
            strList.Add("StackTrace: ");
            foreach (string trace in StackTrace)
                strList.Add(trace);
            strList.Add("Evidence");
            strList.Add("Images: ");
            foreach (Image image in Evidence.Images)
                strList.Add(image.FilePath);
            return strList;
        }

    }

    public class ObservableTestSuiteResults
    {
        public string TimeElapsed = "";
        public int TotalTests;
        public int PassedTests;
        public int FailedTests;
        public List<string> Messages = new List<string>();
        public List<TestResult> TestResults = new List<TestResult>();
        public string TestSuiteName = "";

        public event MessageAddedEventHandler MessageAdded;

        public delegate void MessageAddedEventHandler(object sender, EventArgs e);

        public void AddMessage(string newItem)
        {
            Messages.Add(newItem);
            MessageAdded?.Invoke(this, new EventArgs());
        }

        public void SetImagePathsToRelative(string reportFolder)
        {
            foreach (TestResult tr in TestResults)
            {
                foreach (Image img in tr.Evidence.Images)
                {
                    if (!string.IsNullOrEmpty(img.FilePath))
                    {
                        string relativePath = img.FilePath.Replace(reportFolder, ".");
                        relativePath = relativePath.Replace(@"\", "/");
                        img.RelativeFilePath = relativePath;
                    }
                }
            }
        }

        public event ResultAddedEventHandler ResultAdded;

        public delegate void ResultAddedEventHandler(object sender, EventArgs e);

        public void AddResult(TestResult newItem)
        {
            if (newItem.Passed)
            {
                PassedTests += 1;
            }
            else
            {
                FailedTests += 1;
            }
            TotalTests += 1;
            TestResults.Add(newItem);
            ResultAdded?.Invoke(this, new EventArgs());
        }

        public string ToJson()
        {
            string testSuiteResult = JsonConvert.SerializeObject(this, Formatting.Indented);
            return testSuiteResult;
        }

        public List<string> ToStringList()
        {
            var strList = new List<string>();
            strList.Add(TestSuiteName);
            strList.Add("Summary");
            strList.Add("Elapsed Time: " + TimeElapsed);
            strList.Add(string.Format("{0}      Total Tests:     {1}%", TotalTests.ToString(), (TotalTests / (double)TotalTests * 100.0d).ToString("0.##")));
            strList.Add(string.Format("{0}/{1}  Passed Tests:    {2}%", PassedTests.ToString(), TotalTests.ToString(), (PassedTests / (double)TotalTests * 100.0d).ToString("0.##")));
            strList.Add(string.Format("{0}/{1}  Failed Tests:    {2}%", FailedTests.ToString(), TotalTests.ToString(), (FailedTests / (double)TotalTests * 100.0d).ToString("0.##")));
            return strList;
        }

    }

}