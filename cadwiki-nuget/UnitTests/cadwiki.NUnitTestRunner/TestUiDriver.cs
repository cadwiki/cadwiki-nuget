using System;
using System.Windows.Forms;
using cadwiki.NUnitTestRunner.Creators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{

    [TestClass()]
    public class TestUiDriver
    {


        [TestMethod()]
        public void Test_RunTestsWithWpUi_ShouldPass()
        {

            var testStringsType = typeof(TestStrings);
            Type[] allTypes = new[] { testStringsType };

            var results = new cadwiki.NUnitTestRunner.Results.ObservableTestSuiteResults();
            results.TestSuiteName = "Test_RunTestsWithWpUi_ShouldPass";
            var driver = new cadwiki.NUnitTestRunner.Ui.WpfDriver(ref results, allTypes);
            cadwiki.NUnitTestRunner.UI.WindowTestRunner window = driver.GetWindow();
            window.Show();
            driver.ExecuteTestsAsync();
            System.Threading.Thread.Sleep(3000);
            window.Close();
        }

        [TestMethod()]
        public void Test_RunTestsWithWinFormsUi_ShouldPass()
        {

            var testStringsType = typeof(TestStrings);
            Type[] allTypes = new[] { testStringsType };

            var results = new cadwiki.NUnitTestRunner.Results.ObservableTestSuiteResults();
            results.TestSuiteName = "Test_RunTestsWithWinFormsUi_ShouldPass";
            var driver = new cadwiki.NUnitTestRunner.Ui.WinformsDriver(ref results, allTypes);
            cadwiki.NUnitTestRunner.UI.FormTestRunner form = driver.GetForm();

            form.Show();
            driver.ExecuteTestsAsync();
            System.Threading.Thread.Sleep(3000);
            form.Close();
        }


        [TestMethod()]
        public void Test_WpfDriver_SuiteResultToJson_ShouldPass()
        {
            var testStringsType = typeof(TestStrings);
            Type[] allTypes = new[] { testStringsType };
            var results = new cadwiki.NUnitTestRunner.Results.ObservableTestSuiteResults();
            results.TestSuiteName = "Test_WpfDriver_SuiteResultToJson_ShouldPass";
            var driver = new cadwiki.NUnitTestRunner.Ui.WpfDriver(ref results, allTypes);
            cadwiki.NUnitTestRunner.UI.WindowTestRunner window = driver.GetWindow();
            driver.ExecuteTestsAsync();
            string jsonString = results.ToJson();
            Assert.AreEqual(2, results.TotalTests);
            Assert.IsNotNull(jsonString);
            Assert.AreNotEqual("{}", jsonString);
            Assert.AreNotEqual("", jsonString);
        }

        [TestMethod()]
        public void Test_WinFormsDriver_SuiteResultToJson_ShouldPass()
        {
            var testStringsType = typeof(TestStrings);
            Type[] allTypes = new[] { testStringsType };
            var results = new cadwiki.NUnitTestRunner.Results.ObservableTestSuiteResults();
            results.TestSuiteName = "Test_WinFormsDriver_SuiteResultToJson_ShouldPass";
            var driver = new cadwiki.NUnitTestRunner.Ui.WinformsDriver(ref results, allTypes);
            cadwiki.NUnitTestRunner.UI.FormTestRunner form = driver.GetForm();
            driver.ExecuteTestsAsync();
            string jsonString = results.ToJson();
            Assert.AreEqual(2, results.TotalTests);
            Assert.IsNotNull(jsonString);
            Assert.AreNotEqual("{}", jsonString);
            Assert.AreNotEqual("", jsonString);
        }

        [TestMethod()]
        public void Test_WinFormsDriver_ClickEvidenceButton_GetOpenWindowFileExporer_ShouldPass()
        {
            var testStringsType = typeof(TestStrings);
            Type[] allTypes = new[] { testStringsType };
            var results = new cadwiki.NUnitTestRunner.Results.ObservableTestSuiteResults();
            results.TestSuiteName = "Test_WinFormsDriver_ClickEvidenceButton_GetOpenWindowFileExporer_ShouldPass";
            var driver = new cadwiki.NUnitTestRunner.Ui.WinformsDriver(ref results, allTypes);
            cadwiki.NUnitTestRunner.UI.FormTestRunner form = driver.GetForm();
            form.Show();
            var tce = new TestEvidenceCreator();
            var hWnd = tce.ProcessesGetHandleFromUiTitle(form.Text);
            form.BringToFront();
            bool wasButtonPressed = tce.MicrosoftTestClickUiControlByName(hWnd, "Evidence");
            Application.DoEvents();
            System.Threading.Thread.Sleep(3000);
            string windowsExplorerTitle = "cadwiki.NUnitTestRunner";
            var windowsExplorerHandle = cadwiki.NUnitTestRunner.WinAPI.ExtensionMethods.GetOpenWindow(windowsExplorerTitle);
            form.Close();
            cadwiki.NUnitTestRunner.WinAPI.ExtensionMethods.CloseWindow(windowsExplorerHandle);
            Assert.IsTrue(wasButtonPressed);
            Assert.AreNotEqual(IntPtr.Zero, windowsExplorerHandle);
        }

    }
}