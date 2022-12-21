Imports cadwiki.NUnitTestRunner
Imports cadwiki.NUnitTestRunner.Creators
Imports cadwiki.NUnitTestRunner.TestEvidence

<TestClass()> Public Class TestsForEvidenceCreator



    <TestMethod()> Public Sub Test_CreatePDF_ShouldPass()
        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}
        Dim results As New Results.ObservableTestSuiteResults()
        results.TestSuiteName = "Test_CreatePDF_ShouldPass"
        Dim driver As New UI.WinformsDriver(results, allTypes)
        Dim testEvidenceCreator As New TestEvidenceCreator()
        testEvidenceCreator.CreatePdf(results)
    End Sub

    <TestMethod()> Public Sub Test_PrintWindow_ShouldCreateJpeg()

        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}

        Dim results As New Results.ObservableTestSuiteResults()
        results.TestSuiteName = "Test_PrintWindow_ShouldCreateJpeg"
        Dim driver As New UI.WpfDriver(results, allTypes)
        Dim window As UI.WindowTestRunner = driver.GetWindow()
        window.Show()
        driver.ExecuteTestsAsync()

        Dim testEvidenceCreator As New TestEvidenceCreator()
        Dim windowIntPtr As IntPtr = testEvidenceCreator.ProcessesGetHandleFromUiTitle("N Unit Test Runner")

        Dim evidence As New Evidence()
        testEvidenceCreator.TakeJpegScreenshot(windowIntPtr, "Title")
        evidence = testEvidenceCreator.GetEvidenceForCurrentTest()
        window.Close()
        Assert.IsTrue(IO.File.Exists(evidence.Images.FirstOrDefault.FilePath), "Failed to create screenshot")
    End Sub


    <TestMethod()> Public Sub Test_CreatePdf_ShouldCreatePdf()

        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}

        Dim results As New Results.ObservableTestSuiteResults()
        results.TestSuiteName = "Test_CreatePdf_ShouldCreatePdf"
        Dim driver As New UI.WpfDriver(results, allTypes)
        Dim window As UI.WindowTestRunner = driver.GetWindow()
        window.Show()
        driver.ExecuteTestsAsync()

        Dim testEvidenceCreator As New Creators.TestEvidenceCreator()


        Dim windowIntPtr As IntPtr = testEvidenceCreator.ProcessesGetHandleFromUiTitle("N Unit Test Runner")

        Dim evidence As New Evidence()
        testEvidenceCreator.TakeJpegScreenshot(windowIntPtr, "Title")
        'add evidence to the first test
        evidence = testEvidenceCreator.GetEvidenceForCurrentTest()
        results.TestResults(0).Evidence = evidence

        Dim filePath As String = evidence.Images(0).FilePath
        Dim pdf As String = testEvidenceCreator.CreatePdf(results)
        testEvidenceCreator.CreateHtmlReport(results)
        window.Close()

        Assert.IsTrue(IO.File.Exists(pdf), "Failed to create pdf")
    End Sub

End Class