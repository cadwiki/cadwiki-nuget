Imports System.Drawing.Imaging
Imports System.Reflection
Imports System.Windows.Forms
Imports cadwiki.NUnitTestRunner
Imports cadwiki.NUnitTestRunner.TestEvidence
Imports cadwiki.NUnitTestRunner.TestEvidence.Creator

<TestClass()> Public Class TestEvidenceCreator


    <TestMethod()> Public Sub Test_PrintWindow_ShouldCreateJpeg()

        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}

        Dim results As New Results.ObservableTestSuiteResults()
        Dim driver As New UI.WpfDriver(results, allTypes)
        Dim window As UI.WindowTestRunner = driver.GetWindow()
        window.Show()
        driver.ExecuteTests()

        Dim testEvidenceCreator As New Creator()

        Dim windowIntPtr As IntPtr = testEvidenceCreator.ProcessesGetHandleFromUiTitle("N Unit Test Runner")
        Dim controlName = "ButtonOk"
        Dim screenshotPath = "C:\Temp\test.jpg"
        If (IO.File.Exists(screenshotPath)) Then
            IO.File.Delete(screenshotPath)
        End If
        Dim format As ImageFormat = ImageFormat.Jpeg

        PrintWindowToImage(windowIntPtr, screenshotPath, format)

        window.Close()

        Assert.IsTrue(IO.File.Exists(screenshotPath), "Failed to create screenshot")
    End Sub



End Class