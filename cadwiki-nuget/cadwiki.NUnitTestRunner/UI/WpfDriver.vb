Option Strict On
Option Infer Off
Option Explicit On

Imports cadwiki.NUnitTestRunner.Results
Namespace Ui
    Public Class WpfDriver

        Private _regressionTestTypes As Type()
        Private _window As WindowTestRunner = Nothing
        Private _commonUiObject As New CommonUiObject()

        Public Function GetWindow() As WindowTestRunner
            Return _window
        End Function

        Public Sub New()
            _window = New WindowTestRunner()
        End Sub

        Public Sub New(ByRef suiteResult As ObservableTestSuiteResults,
                       regressionTestTypes As Type())
            Try
                If suiteResult Is Nothing Then
                    suiteResult = New ObservableTestSuiteResults()
                End If
                If regressionTestTypes Is Nothing Then
                    Console.WriteLine("RegressionTestTypes argument was null.")
                    Return
                End If
                _regressionTestTypes = regressionTestTypes
                _window = New WindowTestRunner(suiteResult)
                _commonUiObject.WpfAddResultsToTreeView(suiteResult, _window.TreeViewResults)
            Catch ex As Exception
                Dim window As cadwiki.WpfUi.Templates.WindowAutoCADException =
                    New WpfUi.Templates.WindowAutoCADException(ex)
                window.ShowDialog()
            End Try
        End Sub

        Public Async Function ExecuteTestsAsync() As Task
            Try
                Dim stopWatch As Stopwatch = New Stopwatch()
                stopWatch.Start()
                Await Engine.RunTestsFromType(_window.ObservableResults, stopWatch, _regressionTestTypes)
                _commonUiObject.WpfAddResultsToTreeView(_window.ObservableResults, _window.TreeViewResults)
            Catch ex As Exception
                Dim window As cadwiki.WpfUi.Templates.WindowAutoCADException =
                    New WpfUi.Templates.WindowAutoCADException(ex)
                window.ShowDialog()
            End Try

        End Function
    End Class
End Namespace
