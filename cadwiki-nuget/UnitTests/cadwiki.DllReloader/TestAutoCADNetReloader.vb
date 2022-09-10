Imports System.Reflection
Imports cadwiki.DllReloader.AutoCAD
Imports cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons

<TestClass()> Public Class TestAutoCADNetReloader

    <TestMethod()> Public Sub Test_Configure_FromAnEmptyIniFile_ShouldReturnNonEmptyString()
        Dim actual As String

        Dim appAssembly As Assembly = Assembly.GetExecutingAssembly
        Dim reloader As AutoCADAppDomainDllReloader = New AutoCADAppDomainDllReloader()
        reloader.ClearIni()
        reloader.Configure(appAssembly)
        actual = reloader.GetIExtensionApplicationClassName()

        Assert.AreNotEqual("", actual)
        Assert.AreNotEqual(Nothing, actual)

    End Sub
    <TestMethod()> Public Sub Test_GenericCommandHandler_WithNoArguments_ShouldPass()
        Dim actual As String
        Dim reloader As AutoCADAppDomainDllReloader = New AutoCADAppDomainDllReloader()
        Dim uiRouter As UiRouter = New UiRouter(
                "cadwiki.NetUtils", "GetCurrentlyExecutingAssembly", Nothing,
                reloader,
                Assembly.GetExecutingAssembly())

        Dim commandHandler = New GenericClickCommandHandler()
        commandHandler.Execute(uiRouter)
    End Sub

End Class