﻿Option Strict On
Option Infer Off
Option Explicit On

Imports cadwiki.NUnitTestRunner.Results
Namespace Ui
    Public Class WinformsDriver

        Private _regressionTestTypes As Type()
        Private _form As FormTestRunner = Nothing
        Private _commonUiObject As New CommonUiObject()

        Public Function GetForm() As FormTestRunner
            Return _form
        End Function

        Public Sub New()
            _form = New FormTestRunner()
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
                _form = New FormTestRunner(suiteResult)
                _commonUiObject.WinFormsAddResultsToTreeView(suiteResult, _form.TreeViewResults)
            Catch ex As Exception
                Dim window As cadwiki.WpfUi.Templates.WindowAutoCADException =
                    New WpfUi.Templates.WindowAutoCADException(ex)
                window.Show()
            End Try
        End Sub

        Public Async Function ExecuteTestsAsync() As Task
            Try
                Dim stopWatch As Stopwatch = New Stopwatch()
                stopWatch.Start()
                Await Engine.RunTestsFromType(_form.ObservableResults, stopWatch, _regressionTestTypes)
                _commonUiObject.WinFormsUpdateResultsToTreeView(_form.ObservableResults, _form.TreeViewResults)
            Catch ex As Exception
                Dim window As cadwiki.WpfUi.Templates.WindowAutoCADException =
                    New WpfUi.Templates.WindowAutoCADException(ex)
                window.Show()
            End Try

        End Function
    End Class
End Namespace
