
Imports System.Reflection
Imports NUnit.Framework
Imports NUnit.Framework.Interfaces
Imports cadwiki.NetUtils
Imports cadwiki.NUnitTestRunner.Results
Imports cadwiki.NUnitTestRunner.TestEvidence
Imports System.Runtime.CompilerServices

Public Class Engine

    Public Shared TestEvidenceCreator As New Creators.TestEvidenceCreator()

    Public Shared Async Function RunTestsFromType(suiteResult As ObservableTestSuiteResults,
            stopwatch As Stopwatch,
            integrationTestTypes As Type()) As Task
        Dim tuples As List(Of Tuple(Of Type, MethodInfo)) = Utils.GetTestMethodDictionarySafely(integrationTestTypes)

        Await RunTests(suiteResult, tuples)
        stopwatch.Stop()
        TestEvidenceCreator.SetEvidenceForCurrentTest(Nothing)

        Dim ts As TimeSpan = stopwatch.Elapsed
        Dim elapsedTime As String = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours,
                ts.Minutes,
                ts.Seconds,
                ts.Milliseconds / 10)
        If ts.TotalMinutes > 5 Then
            suiteResult.TimeElapsed = "Consider removing tests to reduce elapsed time to below 5 minutes " +
                    elapsedTime
        Else
            suiteResult.TimeElapsed = elapsedTime
        End If

        'Output pdf with evidence
        TestEvidenceCreator.CreatePdf(suiteResult)

        Dim jsonString As String = suiteResult.ToJson()
        TestEvidenceCreator.WriteTestSuiteResultsToFile(jsonString)
    End Function

    Private Shared Async Function RunTests(suiteResult As ObservableTestSuiteResults, tuples As List(Of Tuple(Of Type, MethodInfo))) As Task
        For Each item As Tuple(Of Type, MethodInfo) In tuples
            Dim testResult As New TestResult
            'Clear current evidence
            TestEvidenceCreator.SetEvidenceForCurrentTest(Nothing)
            Dim type As Type = item.Item1
            Dim mi As MethodInfo = item.Item2
            Dim methodName As String = mi.Name

            Dim setupTuple As Tuple(Of Type, MethodInfo) = Utils.GetSetupMethod({type})
            Dim setupObject As Object = Nothing
            Dim setupMethodInfo As MethodInfo = Nothing
            If setupTuple IsNot Nothing Then
                Dim setupType As Type = setupTuple.Item1
                setupObject = Activator.CreateInstance(setupType)
                setupMethodInfo = setupTuple.Item2
            End If

            Dim tearDownTuple As Tuple(Of Type, MethodInfo) = Utils.GetTearDownMethod({type})
            Dim tearDownObject As Object = Nothing
            Dim tearDownMethodInfo As MethodInfo = Nothing
            If tearDownTuple IsNot Nothing Then
                Dim tearDownType As Type = tearDownTuple.Item1
                tearDownObject = Activator.CreateInstance(tearDownType)
                tearDownMethodInfo = tearDownTuple.Item2
            End If

            Try
                suiteResult.AddMessage(vbLf & "Running test method: " + mi.Name)
                Dim o As Object
                o = Activator.CreateInstance(type)
                If setupMethodInfo IsNot Nothing And setupObject IsNot Nothing Then
                    setupMethodInfo.Invoke(setupObject, Nothing)
                End If

                Dim isAwaitable As Boolean = mi.ReturnType.GetMethod(NameOf(Task.GetAwaiter)) IsNot Nothing
                Dim result As Object = Nothing

                If isAwaitable Then
                    result = Await mi.Invoke(o, Nothing)
                    Dim test As String = ""
                Else
                    mi.Invoke(o, Nothing)
                End If

                testResult.TestName = mi.Name
                testResult.Passed = True
                If tearDownMethodInfo IsNot Nothing And tearDownObject IsNot Nothing Then
                    tearDownMethodInfo.Invoke(tearDownObject, Nothing)
                End If
            Catch ex As SuccessException
                testResult.TestName = mi.Name
                testResult.Passed = True
                testResult.ExceptionMessage = ex.Message
            Catch ex As TargetInvocationException
                If (TypeOf ex.InnerException Is SuccessException) Then
                    testResult.TestName = mi.Name
                    testResult.Passed = True
                    testResult.ExceptionMessage = ex.Message
                ElseIf (TypeOf ex.InnerException Is AssertionException) Then
                    Dim ae As AssertionException = CType(ex.InnerException, AssertionException)
                    Dim result As ResultState = ae.ResultState
                    testResult.TestName = mi.Name
                    testResult.Passed = False
                    testResult.ExceptionMessage = ae.Message
                    testResult.StackTrace = Exceptions.GetStackTraceLines(ae)
                Else
                    testResult.TestName = mi.Name
                    testResult.Passed = False
                    AddFullExceptionToTestResult(testResult, ex)
                End If
            Catch ex As Exception
                testResult.TestName = mi.Name
                testResult.Passed = False
                If (ex.InnerException IsNot Nothing) Then
                    AddFullExceptionToTestResult(testResult, ex)
                Else
                    testResult.ExceptionMessage = ex.Message
                    testResult.StackTrace = Exceptions.GetStackTraceLines(ex)
                End If
            End Try
            'Get any evidence that was collected during the test
            Dim evidence As Evidence = TestEvidenceCreator.GetEvidenceForCurrentTest()
            If (evidence IsNot Nothing) Then
                testResult.Evidence = evidence
            End If
            suiteResult.AddResult(testResult)

        Next
    End Function

    Private Shared Sub AddFullExceptionToTestResult(testResult As TestResult, ex As TargetInvocationException)
        testResult.ExceptionMessage = ex.Message + " InnerMessage:" + ex.InnerException.Message
        Dim stackTrace As List(Of String) = Exceptions.GetStackTraceLines(ex)
        stackTrace.Add("InnerException:")
        stackTrace.AddRange(Exceptions.GetStackTraceLines(ex.InnerException))
        testResult.StackTrace = stackTrace
    End Sub
End Class

