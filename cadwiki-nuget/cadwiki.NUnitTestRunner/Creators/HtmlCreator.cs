using System.IO;
using System.Reflection;
using HandlebarsDotNet;

namespace cadwiki.NUnitTestRunner
{

    public class HtmlCreator
    {

        private HandlebarsTemplate<object, object> _testSuiteReportTemplateToObject;
        private string _parametizedReport;
        public HtmlCreator()
        {
            var @assembly = Assembly.GetExecutingAssembly();
            CompileAllTemplates(assembly);
        }

        private void CompileAllTemplates(Assembly @assembly)
        {
            string testSuiteReportTemplate = NetUtils.AssemblyUtils.ReadEmbeddedResourceToString(assembly, "TestSuiteReport.html");
            _testSuiteReportTemplateToObject = Handlebars.Compile(testSuiteReportTemplate);
        }

        public string ParameterizeReportTemplate(HTML_Models.TestSuiteReport model)
        {
            if (_testSuiteReportTemplateToObject is not null)
            {
                _parametizedReport = _testSuiteReportTemplateToObject(model);
                return _parametizedReport;
            }
            return null;
        }

        public void SaveReportToFile(string filePath)
        {
            filePath = NetUtils.Paths.GetUniqueFilePath(filePath);
            File.WriteAllText(filePath, _parametizedReport);
        }

    }
}