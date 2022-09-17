Imports System.Reflection
Imports Autodesk.Windows
Imports cadwiki.DllReloader.AutoCAD
Imports cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons

<TestClass()> Public Class TestAutoCADNetReloader

    <TestMethod()> Public Sub Test_Configure_FromAnEmptyIniFile_ShouldReturnNonEmptyString()
        Dim actual As String

        Dim appAssembly As Assembly = Assembly.GetExecutingAssembly
        Dim reloader As AutoCADAppDomainDllReloader = New AutoCADAppDomainDllReloader()
        reloader.ClearIni()
        reloader.Configure(appAssembly, False)
        actual = reloader.GetIExtensionApplicationClassName()

        Assert.AreNotEqual("", actual)
        Assert.AreNotEqual(Nothing, actual)

    End Sub
    <TestMethod()> Public Sub Test_GenericCommandHandler_WithNoArguments_ShouldPass()

        ExecuteRibbonButtonGenericCommandHandler(
                "cadwiki.NetUtils",
                "cadwiki.NetUtils.AssemblyUtils",
                "GetCurrentlyExecutingAssembly",
                Nothing)

    End Sub

    <TestMethod()> Public Sub Test_GenericCommandHandler_WithOneArgument_ShouldPass()
        Dim appAssembly As Assembly = Assembly.GetExecutingAssembly
        Dim parameters As Object() = {appAssembly}

        ExecuteRibbonButtonGenericCommandHandler(
                "cadwiki.NetUtils",
                "cadwiki.NetUtils.AssemblyUtils",
                "GetTypesSafely",
                parameters)
    End Sub

    Private Shared Sub ExecuteRibbonButtonGenericCommandHandler(
            assemblyName As String, fullClassName As String,
            methodName As String, parameters() As Object)

        Dim appAssembly As Assembly = Assembly.GetExecutingAssembly
        Dim reloader As AutoCADAppDomainDllReloader = New AutoCADAppDomainDllReloader()
        reloader.ClearIni()
        reloader.Configure(appAssembly, False)
        Dim uiRouter As UiRouter = New UiRouter(
                assemblyName,
                fullClassName,
                methodName,
                parameters,
                reloader,
                Assembly.GetExecutingAssembly())
        Dim ribbonButton As RibbonButton = New RibbonButton()
        ribbonButton.CommandParameter = uiRouter
        ribbonButton.CommandHandler = New GenericClickCommandHandler()
        ribbonButton.CommandHandler.Execute(ribbonButton)
    End Sub


    <TestMethod()> Public Sub Test_Configure_WithTrueLoadFlag_ShouldFindMoreThan0AssembliesAndLoad0Assemblies()

        Dim appAssembly As Assembly = Assembly.GetExecutingAssembly
        Dim reloader As AutoCADAppDomainDllReloader = New AutoCADAppDomainDllReloader()
        reloader.ClearIni()
        reloader.Configure(appAssembly, True)

        Dim dllFound As List(Of String) = reloader.GetDllsToReload()
        Dim reloadedDlls As List(Of String) = reloader.GetDllsThatWereSuccessfullyReloaded()

        Assert.AreNotEqual(dllFound.Count, 0)
        Assert.AreEqual(reloadedDlls.Count, 0)

    End Sub

End Class