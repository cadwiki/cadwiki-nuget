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
        Dim driver As New Ui.WpfDriver(results, allTypes)
        Dim window As Ui.WindowTestRunner = driver.GetWindow()
        window.Show()
        driver.ExecuteTests()
    End Sub

End Class