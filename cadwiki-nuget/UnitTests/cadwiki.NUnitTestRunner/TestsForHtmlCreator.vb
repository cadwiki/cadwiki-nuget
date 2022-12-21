Imports System.Drawing
Imports System.Windows.Media.Imaging
Imports cadwiki
Imports cadwiki.NetUtils
Imports cadwiki.NUnitTestRunner
Imports cadwiki.NUnitTestRunner.Creators
Imports cadwiki.NUnitTestRunner.TestEvidence

<TestClass()> Public Class TestsForHtmlCreator



    <TestMethod()> Public Sub Test_CreateReport_ShouldPass()

        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}

        Dim testSuiteResults As New Results.ObservableTestSuiteResults()
        testSuiteResults.TestSuiteName = "Test_CreateReport_ShouldPass"
        Dim driver As New UI.WpfDriver(testSuiteResults, allTypes)
        driver.ExecuteTestsAsync()

        Dim testEvidence As New TestEvidenceCreator
        Dim htmlReportFilePath As String = testEvidence.GetNewHtmlReportFilePath(testSuiteResults)

        testSuiteResults.TestSuiteName = "cadwiki Automation Tests"

        Dim htmlCreator As New HtmlCreator
        Dim model As New HTML_Models.TestSuiteReport()
        model.GradeHighlight = "HighlightTest"
        model.TestSuiteResults = testSuiteResults
        model.Title = "cadwiki test suite"

        Dim bitMap As Bitmap = FileStore.My.Resources.ResourceIcons._500x500_cadwiki_v1
        Dim bitMapImage As BitmapImage = Bitmaps.BitMapToBitmapImage(bitMap)
        Dim imageFile As String = System.IO.Path.GetDirectoryName(htmlReportFilePath) + "test.bmp"
        bitMap.Save(imageFile)
        model.BannerImagePath = imageFile

        htmlCreator.ParameterizeReportTemplate(model)


        htmlCreator.SaveReportToFile(htmlReportFilePath)

    End Sub

End Class