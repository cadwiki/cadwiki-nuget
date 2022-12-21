Imports cadwiki.NUnitTestRunner
Imports cadwiki.NUnitTestRunner.Creators
Imports cadwiki.NUnitTestRunner.TestEvidence

<TestClass()> Public Class TestsForHtmlCreator



    <TestMethod()> Public Sub Test_CreateReport_ShouldPass()
        Dim htmlCreator As New HtmlCreator
        htmlCreator.ParameterizeReportTemplate()
    End Sub

End Class