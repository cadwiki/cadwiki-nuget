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
        testSuiteResults.TestSuiteName = "cadwiki Automation Tests"
        Dim driver As New UI.WpfDriver(testSuiteResults, allTypes)
        driver.ExecuteTestsAsync()

        Dim testEvidenceCreator As New TestEvidenceCreator

        Dim htmlReportFilePath As String = testEvidenceCreator.GetNewHtmlReportFilePath(testSuiteResults)
        Dim htmlCreator As New HtmlCreator
        Dim model As New HTML_Models.TestSuiteReport(testSuiteResults)

        Dim bitMap As Bitmap = FileStore.My.Resources.ResourceIcons._500x500_cadwiki_v1
        Dim bitMapImage As BitmapImage = Bitmaps.BitMapToBitmapImage(bitMap)
        Dim reportFolder As String = System.IO.Path.GetDirectoryName(htmlReportFilePath)
        Dim imageFile As String = reportFolder + "\" + "test.bmp"
        bitMap.Save(imageFile)
        model.BannerImagePath = "./test.bmp"

        testSuiteResults.SetImagePathsToRelative(reportFolder)
        htmlCreator.ParameterizeReportTemplate(model)


        htmlCreator.SaveReportToFile(htmlReportFilePath)

    End Sub

End Class