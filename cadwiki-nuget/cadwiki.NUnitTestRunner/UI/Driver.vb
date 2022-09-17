﻿Option Strict On
Option Infer Off
Option Explicit On

Imports cadwiki.NUnitTestRunner.Results
Namespace Ui
    Public Class Driver

        Private _regressionTestTypes As Type()
        Private _window As WindowTestRunner

        Public Function GetWindow() As WindowTestRunner
            Return _window
        End Function


        Public Sub New(suiteResult As ObservableTestSuiteResults, regressionTestTypes As Type())
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
                _window.AddResult()
            Catch ex As Exception

            End Try
        End Sub

        Public Sub ExecuteTests()
            Try
                Dim stopWatch As Stopwatch = New Stopwatch()
                stopWatch.Start()
                Engine.RunTestsFromType(_window.ObservableResults, stopWatch, _regressionTestTypes)
                _window.UpdateResult()
            Catch ex As Exception
                Console.WriteLine("Exception: " + ex.Message)
            End Try

        End Sub
    End Class
End Namespace