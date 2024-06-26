using System;
using System.Diagnostics;
using System.Threading.Tasks;
using cadwiki.NUnitTestRunner.Results;

namespace cadwiki.NUnitTestRunner.Ui
{
    public class WpfDriver
    {

        private Type[] _regressionTestTypes;
        private cadwiki.NUnitTestRunner.UI.WindowTestRunner _window = null;
        private CommonUiObject _commonUiObject = new CommonUiObject();

        public cadwiki.NUnitTestRunner.UI.WindowTestRunner GetWindow()
        {
            return _window;
        }

        public WpfDriver()
        {
            _window = new cadwiki.NUnitTestRunner.UI.WindowTestRunner();
        }

        public WpfDriver(ref ObservableTestSuiteResults suiteResult, Type[] regressionTestTypes)
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
                _window = new cadwiki.NUnitTestRunner.UI.WindowTestRunner(ref suiteResult);
                _commonUiObject.WpfAddResultsToTreeView(suiteResult, _window.TreeViewResults);
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
                await Engine.RunTestsFromType(_window.ObservableResults, stopWatch, _regressionTestTypes);
                _commonUiObject.WpfAddResultsToTreeView(_window.ObservableResults, _window.TreeViewResults);
            }
            catch (Exception ex)
            {
                var window = new WpfUi.Templates.WindowAutoCADException(ex);
                window.Show();
            }

        }
    }
}