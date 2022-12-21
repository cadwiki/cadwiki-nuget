Imports System.IO
Imports System.Reflection
Imports cadwiki.NUnitTestRunner.Results
Imports HandlebarsDotNet

Public Class HtmlCreator

    Private _testSuiteReportTemplateToObject As HandlebarsDotNet.HandlebarsTemplate(Of Object, Object)
    Private _parametizedReport As String
    Public Sub New()
        Dim assembly As Assembly = System.Reflection.Assembly.GetExecutingAssembly()
        CompileAllTemplates(assembly)
    End Sub

    Private Sub CompileAllTemplates(assembly As Assembly)
        Dim testSuiteReportTemplate As String = NetUtils.AssemblyUtils.ReadEmbeddedResourceToString(assembly, "TestSuiteReport.html")
        _testSuiteReportTemplateToObject = Handlebars.Compile(testSuiteReportTemplate)
    End Sub

    Public Function ParameterizeReportTemplate(model As HTML_Models.TestSuiteReport) As String
        If (_testSuiteReportTemplateToObject IsNot Nothing) Then
            _parametizedReport = _testSuiteReportTemplateToObject(model)
            Return _parametizedReport
        End If
        Return Nothing
    End Function

    Public Sub SaveReportToFile(filePath As String)
        filePath = NetUtils.Paths.GetUniqueFilePath(filePath)
        File.WriteAllText(filePath, _parametizedReport)
    End Sub

End Class
