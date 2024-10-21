using System;
using System.Threading.Tasks;
using cadwiki.NUnitTestRunner.Results;

namespace cadwiki.AC.Workflows
{
    public class NunitTestRunner
    {
        public async Task Run(Type[] regressionTestTypes)
        {
            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            try
            {
                var results = new ObservableTestSuiteResults();
                var driver = new NUnitTestRunner.Ui.WpfDriver(ref results, regressionTestTypes);
                var window = driver.GetWindow();
                // https://forums.autodesk.com/t5/net/how-to-set-a-focus-to-autocad-main-window-from-my-form-of-c-net/td-p/4680059
                global::Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowModelessWindow(window);
                await driver.ExecuteTestsAsync();
            }
            catch (Exception ex)
            {
                ExceptionHandler.WriteToEditor(ex);
            }
        }
    }
}