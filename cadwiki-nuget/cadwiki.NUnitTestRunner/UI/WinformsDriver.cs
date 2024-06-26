using System;
using System.Diagnostics;
using System.Threading.Tasks;
using cadwiki.NUnitTestRunner.Results;

namespace cadwiki.NUnitTestRunner.Ui
{
    public class WinformsDriver
    {

        private Type[] _regressionTestTypes;
        private UI.FormTestRunner _form = null;
        private CommonUiObject _commonUiObject = new CommonUiObject();

        public UI.FormTestRunner GetForm()
        {
            return _form;
        }

        public WinformsDriver()
        {
            _form = new UI.FormTestRunner();
        }

        public WinformsDriver(ref ObservableTestSuiteResults suiteResult, Type[] regressionTestTypes)
        {
            try
            {
                if (suiteResult is null)
                {
                    suiteResult = new ObservableTestSuiteResults();
                }
                if (regressionTestTypes is null)
                {
                    Console.WriteLine("RegressionTestTypes argument was null.");
                    return;
                }
                _regressionTestTypes = regressionTestTypes;
                _form = new UI.FormTestRunner(ref suiteResult);
                _commonUiObject.WinFormsAddResultsToTreeView(suiteResult, _form.TreeViewResults);
            }
            catch (Exception ex)
            {
                var window = new WpfUi.Templates.WindowAutoCADException(ex);
                window.Show();
            }
        }

        public async Task ExecuteTestsAsync()
        {
            try
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                await Engine.RunTestsFromType(_form.ObservableResults, stopWatch, _regressionTestTypes);
                _commonUiObject.WinFormsUpdateResultsToTreeView(_form.ObservableResults, _form.TreeViewResults);
            }
            catch (Exception ex)
            {
                var window = new WpfUi.Templates.WindowAutoCADException(ex);
                window.Show();
            }

        }
    }
}