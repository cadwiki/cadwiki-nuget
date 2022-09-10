Option Strict On
Option Infer Off
Option Explicit On
Imports System
Imports System.Collections.Generic

Namespace Results
    Public Class TestResult
        Public TestName As String
        Public Passed As Boolean
        Public ExceptionMessage As String
        Public StackTrace As List(Of String)
    End Class

    Public Class ObservableTestSuiteResults
        Public TimeElapsed As String
        Public TotalTests As Integer
        Public PassedTests As Integer
        Public FailedTests As Integer
        Public Messages As New List(Of String)
        Public TestResults As New List(Of TestResult)

        Public Event MessageAdded(sender As Object, e As EventArgs)

        Public Sub AddMessage(newItem As String)
            Messages.Add(newItem)
            RaiseEvent MessageAdded(Me, New EventArgs())
        End Sub


        Public Event ResultAdded(sender As Object, e As EventArgs)

        Public Sub AddResult(newItem As TestResult)
            If newItem.Passed Then
                PassedTests += 1
            Else
                FailedTests += 1
            End If
            TotalTests += 1
            TestResults.Add(newItem)
            RaiseEvent ResultAdded(Me, New EventArgs())
        End Sub
    End Class

End Namespace
