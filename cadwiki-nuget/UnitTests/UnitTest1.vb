Imports System.Reflection
Imports cadwiki.DllReloader.AutoCAD

<TestClass()> Public Class UnitTest1

    <TestMethod()> Public Sub TestAutoCADNetReloader_Configure_FromAnEmptyIniFile_ShouldReturnNonEmptyString()
        Dim actual As String

        Dim appAssembly As Assembly = Assembly.GetExecutingAssembly
        Dim reloader As AutoCADAppDomainDllReloader = New AutoCADAppDomainDllReloader()
        reloader.ClearIni()
        reloader.Configure(appAssembly)
        actual = reloader.GetIExtensionApplicationClassName()

        Assert.AreNotEqual("", actual)
        Assert.AreNotEqual(Nothing, actual)

    End Sub

End Class