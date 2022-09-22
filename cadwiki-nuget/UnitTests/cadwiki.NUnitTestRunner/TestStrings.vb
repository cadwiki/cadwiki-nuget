Imports NUnit.Framework

<TestFixture>
Partial Public Class TestStrings

    <Test>
    Public Sub Test_DoStringsMatch_ShouldPass()
        Dim expected As String = "Hello"
        Dim actual As String = "Hello"
        Assert.AreEqual(expected, actual, "Input strings don't match")
    End Sub

    <Test>
    Public Sub Test_DoStringsMatch_ShouldFail()
        Dim expected As String = "Hello"
        Dim actual As String = "World"
        Assert.AreEqual(expected, actual, "Input strings don't match")
    End Sub


End Class