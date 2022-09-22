Imports System.Reflection
Imports cadwiki.NUnitTestRunner


<TestClass()> Public Class TestUiDriver

    <TestMethod()> Public Sub Test_RunTests_ShouldReturnNothing()

        Dim testStringsType As Type = GetType(TestStrings)
        Dim allTypes As Type() = {testStringsType}

        Dim results As New Results.ObservableTestSuiteResults()
        Dim driver As New Ui.Driver(results, allTypes)
        Dim window As Ui.WindowTestRunner = driver.GetWindow()
        window.Show()
        driver.ExecuteTests()

    End Sub

End Class