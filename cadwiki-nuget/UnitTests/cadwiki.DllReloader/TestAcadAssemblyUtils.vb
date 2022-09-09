Imports System.Reflection
Imports cadwiki.DllReloader.AutoCAD

<TestClass()> Public Class TestAcadAssemblyUtils

    <TestMethod()> Public Sub Test_GetAppAssemblySafeley_WithNothing_ShouldReturnNothing()
        Dim expected As Object = Nothing
        Dim actual As Assembly = AcadAssemblyUtils.GetAppObjectSafely(Nothing)
        Assert.AreEqual(expected, actual)
    End Sub

End Class
