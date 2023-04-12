Option Strict On
Option Infer Off
Option Explicit On

Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.EditorInput
Imports cadwiki.NUnitTestRunner.Results
Imports cadwiki.NUnitTestRunner.UI

Namespace Workflows
    Public Class NunitTestRunner
        Public Async Function Run(ByVal regressionTestTypes As Type()) As Task
            Dim doc As Document = Core.Application.DocumentManager.MdiActiveDocument
            Dim ed As Editor = doc.Editor
            Try
                Dim results As ObservableTestSuiteResults = New cadwiki.NUnitTestRunner.Results.ObservableTestSuiteResults()
                Dim driver As WpfDriver = New cadwiki.NUnitTestRunner.UI.WpfDriver(results, regressionTestTypes)
                Dim window As WindowTestRunner = driver.GetWindow()
                ' https://forums.autodesk.com/t5/net/how-to-set-a-focus-to-autocad-main-window-from-my-form-of-c-net/td-p/4680059
                Core.Application.ShowModelessWindow(window)
                Await driver.ExecuteTestsAsync()
            Catch ex As Exception
                ExceptionHandler.Handle(ex)
            End Try
        End Function
    End Class
End Namespace
