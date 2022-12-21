Imports System.IO
Imports System.Reflection
Imports HandlebarsDotNet

Public Class HtmlCreator
    Public Sub ParameterizeReportTemplate()
        Dim assembly As Assembly = System.Reflection.Assembly.GetExecutingAssembly()
        Dim templateString As String = NetUtils.AssemblyUtils.ReadEmbeddedResourceToString(assembly, "HtmlTemplateTestSuiteReport.cshtml")
        If (templateString IsNot Nothing) Then
            Dim template As HandlebarsDotNet.HandlebarsTemplate(Of Object, Object) = Handlebars.Compile(templateString)
            Dim reportModel As New HtmlModelTestSuiteReport
            reportModel.BannerImagePath = "test"
            Dim result As Object = template(reportModel)
        End If
    End Sub
End Class
