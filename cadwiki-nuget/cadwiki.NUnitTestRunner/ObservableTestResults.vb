Option Strict On
Option Infer Off
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports cadwiki.NUnitTestRunner.TestEvidence
Imports Newtonsoft.Json

Namespace Results
    Public Class TestResult
        Public TestName As String = ""
        Public Passed As Boolean
        Public ExceptionMessage As String = ""
        Public StackTrace As New List(Of String)
        Public Evidence As New TestEvidence.Evidence

        Public Function ToStringList() As List(Of String)
            Dim strList As New List(Of String)
            strList.Add("TestName: " + TestName)
            strList.Add("Passed: " + Passed.ToString())
            strList.Add("ExceptionMessage: " + ExceptionMessage)
            strList.Add("StackTrace: ")
            For Each trace As String In StackTrace
                strList.Add(trace)
            Next
            strList.Add("Evidence")
            strList.Add("Images: ")
            For Each image As Image In Evidence.Images
                strList.Add(image.FilePath)
            Next
            Return strList
        End Function

    End Class

    Public Class ObservableTestSuiteResults
        Public TimeElapsed As String = ""
        Public TotalTests As Integer
        Public PassedTests As Integer
        Public FailedTests As Integer
        Public Messages As New List(Of String)
        Public TestResults As New List(Of TestResult)
        Public TestSuiteName As String = ""

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

        Public Function ToJson() As String
            Dim testSuiteResult As String = JsonConvert.SerializeObject(Me, Formatting.Indented)
            Return testSuiteResult
        End Function

        Public Function ToStringList() As List(Of String)
            Dim strList As New List(Of String)
            strList.Add(TestSuiteName)
            strList.Add("Summary")
            strList.Add("Elapsed Time: " + TimeElapsed)
            strList.Add(String.Format("Total Tests:    {0}    {1}%",
                        TotalTests.ToString(),
                        (TotalTests / TotalTests * 100.0).ToString
                        ))
            strList.Add(String.Format("Passed Tests:    {0}/{1}    {2}%",
                        PassedTests.ToString(),
                        TotalTests.ToString(),
                        (PassedTests / TotalTests * 100.0).ToString
                        ))
            strList.Add(String.Format("Failed Tests:    {0}/{1}    {2}%",
                        FailedTests.ToString(),
                        TotalTests.ToString(),
                        (FailedTests / TotalTests * 100.0).ToString
                        ))
            Return strList
        End Function

    End Class

End Namespace
