Imports System.Drawing.Imaging
Imports System.Reflection
Imports System.Windows.Forms
Imports cadwiki.NUnitTestRunner
Imports cadwiki.NUnitTestRunner.TestEvidence
Imports cadwiki.NUnitTestRunner.TestEvidence.TestEvidenceCreator

<TestClass()> Public Class TestEvidenceCreator


    <TestMethod()> Public Sub Test_PrintWindow_ShouldCreateJpeg()

        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}

        Dim results As New Results.ObservableTestSuiteResults()
        Dim driver As New UI.WpfDriver(results, allTypes)
        Dim window As UI.WindowTestRunner = driver.GetWindow()
        window.Show()
        driver.ExecuteTests()

        Dim testEvidenceCreator As New TestEvidence.TestEvidenceCreator()
        Dim windowIntPtr As IntPtr = testEvidenceCreator.ProcessesGetHandleFromUiTitle("N Unit Test Runner")

        Dim evidence As New Evidence()
        evidence.TakeJpegScreenshot(windowIntPtr, "Title")

        window.Close()

        Assert.IsTrue(IO.File.Exists(evidence.Images.FirstOrDefault.FilePath), "Failed to create screenshot")
    End Sub


    <TestMethod()> Public Sub Test_CreatePdf_ShouldCreatePdf()

        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}

        Dim results As New Results.ObservableTestSuiteResults()
        Dim driver As New UI.WpfDriver(results, allTypes)
        Dim window As UI.WindowTestRunner = driver.GetWindow()
        window.Show()
        driver.ExecuteTests()

        Dim testEvidenceCreator As New TestEvidence.TestEvidenceCreator()


        Dim windowIntPtr As IntPtr = testEvidenceCreator.ProcessesGetHandleFromUiTitle("N Unit Test Runner")

        Dim evidence As New Evidence()
        evidence.TakeJpegScreenshot(windowIntPtr, "Title")
        results.TestResults(0).Evidence = evidence

        Dim filePath As String = evidence.Images(0).FilePath
        Dim pdf As String = testEvidenceCreator.CreatePdf(results)
        window.Close()

        Assert.IsTrue(IO.File.Exists(pdf), "Failed to create pdf")
    End Sub


End Class