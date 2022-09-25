Imports System.Reflection
Imports cadwiki.NUnitTestRunner


<TestClass()> Public Class TestUiDriver

    <TestMethod()> Public Sub Test_RunTestsWithWpUi_ShouldPass()

        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}

        Dim results As New Results.ObservableTestSuiteResults()
        Dim driver As New Ui.WpfDriver(results, allTypes)
        Dim window As Ui.WindowTestRunner = driver.GetWindow()
        window.Show()
        driver.ExecuteTests()
    End Sub

    <TestMethod()> Public Sub Test_RunTestsWithWinFormsUi_ShouldPass()

        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}

        Dim results As New Results.ObservableTestSuiteResults()
        Dim driver As New Ui.WinformsDriver(results, allTypes)
        Dim form As UI.FormTestRunner = driver.GetForm()

        driver.ExecuteTests()
        form.Show()
    End Sub


    <TestMethod()> Public Sub Test_WpfDriver_SuiteResultToJson_ShouldPass()
        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}
        Dim results As New Results.ObservableTestSuiteResults()
        Dim driver As New UI.WpfDriver(results, allTypes)
        Dim form As UI.WindowTestRunner = driver.GetWindow()
        driver.ExecuteTests()
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
        Dim driver As New UI.WinformsDriver(results, allTypes)
        Dim form As UI.FormTestRunner = driver.GetForm()
        driver.ExecuteTests()
        Dim jsonString As String = results.ToJson()
        Assert.AreEqual(2, results.TotalTests)
        Assert.IsNotNull(jsonString)
        Assert.AreNotEqual("{}", jsonString)
        Assert.AreNotEqual("", jsonString)
    End Sub

End Class