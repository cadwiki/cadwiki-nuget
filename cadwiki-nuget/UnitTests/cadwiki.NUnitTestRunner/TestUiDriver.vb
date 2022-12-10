Imports System.Reflection
Imports System.Windows.Forms
Imports cadwiki.NUnitTestRunner
Imports cadwiki.NUnitTestRunner.TestEvidence
Imports cadwiki.NUnitTestRunner.Creators

<TestClass()> Public Class TestUiDriver


    <TestMethod()> Public Sub Test_RunTestsWithWpUi_ShouldPass()

        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}

        Dim results As New Results.ObservableTestSuiteResults()
        results.TestSuiteName = "Test_RunTestsWithWpUi_ShouldPass"
        Dim driver As New UI.WpfDriver(results, allTypes)
        Dim window As UI.WindowTestRunner = driver.GetWindow()
        window.Show()
        driver.ExecuteTestsAsync()
        Threading.Thread.Sleep(3000)
        window.Close()
    End Sub

    <TestMethod()> Public Sub Test_RunTestsWithWinFormsUi_ShouldPass()

        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}

        Dim results As New Results.ObservableTestSuiteResults()
        results.TestSuiteName = "Test_RunTestsWithWinFormsUi_ShouldPass"
        Dim driver As New UI.WinformsDriver(results, allTypes)
        Dim form As UI.FormTestRunner = driver.GetForm()

        form.Show()
        driver.ExecuteTestsAsync()
        Threading.Thread.Sleep(3000)
        form.Close()
    End Sub


    <TestMethod()> Public Sub Test_WpfDriver_SuiteResultToJson_ShouldPass()
        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}
        Dim results As New Results.ObservableTestSuiteResults()
        results.TestSuiteName = "Test_WpfDriver_SuiteResultToJson_ShouldPass"
        Dim driver As New UI.WpfDriver(results, allTypes)
        Dim window As UI.WindowTestRunner = driver.GetWindow()
        driver.ExecuteTestsAsync()
        Dim jsonString As String = results.ToJson()
        Assert.AreEqual(2, results.TotalTests)
        Assert.IsNotNull(jsonString)
        Assert.AreNotEqual("{}", jsonString)
        Assert.AreNotEqual("", jsonString)
    End Sub

    <TestMethod()> Public Sub Test_WinFormsDriver_SuiteResultToJson_ShouldPass()
        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}
        Dim results As New Results.ObservableTestSuiteResults()
        results.TestSuiteName = "Test_WinFormsDriver_SuiteResultToJson_ShouldPass"
        Dim driver As New UI.WinformsDriver(results, allTypes)
        Dim form As UI.FormTestRunner = driver.GetForm()
        driver.ExecuteTestsAsync()
        Dim jsonString As String = results.ToJson()
        Assert.AreEqual(2, results.TotalTests)
        Assert.IsNotNull(jsonString)
        Assert.AreNotEqual("{}", jsonString)
        Assert.AreNotEqual("", jsonString)
    End Sub

    <TestMethod()> Public Sub Test_WinFormsDriver_ClickEvidenceButton_GetOpenWindowFileExporer_ShouldPass()
        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}
        Dim results As New Results.ObservableTestSuiteResults()
        results.TestSuiteName = "Test_WinFormsDriver_ClickEvidenceButton_GetOpenWindowFileExporer_ShouldPass"
        Dim driver As New UI.WinformsDriver(results, allTypes)
        Dim form As UI.FormTestRunner = driver.GetForm()
        form.Show()
        Dim tce As TestEvidenceCreator = New TestEvidenceCreator()
        Dim hWnd = tce.ProcessesGetHandleFromUiTitle(form.Text)
        form.BringToFront()
        Dim wasButtonPressed As Boolean = tce.MicrosoftTestClickUiControlByName(hWnd, "Evidence")
        Application.DoEvents()
        Threading.Thread.Sleep(3000)
        Dim windowsExplorerTitle As String = "cadwiki.NUnitTestRunner"
        Dim windowsExplorerHandle As IntPtr = WinAPI.ExtensionMethods.GetOpenWindow(windowsExplorerTitle)
        form.Close()
        WinAPI.ExtensionMethods.CloseWindow(windowsExplorerHandle)
        Assert.IsTrue(wasButtonPressed)
        Assert.AreNotEqual(IntPtr.Zero, windowsExplorerHandle)
    End Sub

End Class