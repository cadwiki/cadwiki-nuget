using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using cadwiki.NetUtils;
using cadwiki.NUnitTestRunner.Results;
using Microsoft.VisualBasic;
using NUnit.Framework;

namespace cadwiki.NUnitTestRunner
{

    public class Engine
    {

        public static Creators.TestEvidenceCreator TestEvidenceCreator = new Creators.TestEvidenceCreator();

        public static async Task RunTestsFromType(ObservableTestSuiteResults suiteResult, Stopwatch stopwatch, Type[] integrationTestTypes)
        {
            var tuples = Utils.GetTestMethodDictionarySafely(integrationTestTypes);

            await RunTests(suiteResult, tuples);
            stopwatch.Stop();
            TestEvidenceCreator.SetEvidenceForCurrentTest(null);

            var ts = stopwatch.Elapsed;
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10d);
            if (ts.TotalMinutes > 5d)
            {
                suiteResult.TimeElapsed = "Consider removing tests to reduce elapsed time to below 5 minutes " + elapsedTime;
            }
            else
            {
                suiteResult.TimeElapsed = elapsedTime;
            }

            // Output pdf with evidence
            TestEvidenceCreator.CreatePdf(suiteResult);

            string jsonString = suiteResult.ToJson();
            TestEvidenceCreator.WriteTestSuiteResultsToFile(suiteResult, jsonString);
            TestEvidenceCreator.CreateHtmlReport(suiteResult);
        }

        private static async Task RunTests(ObservableTestSuiteResults suiteResult, List<Tuple<Type, MethodInfo>> tuples)
        {
            foreach (var item in tuples)
            {
                var testResult = new TestResult();
                // Clear current evidence
                TestEvidenceCreator.SetEvidenceForCurrentTest(null);
                var type = item.Item1;
                var mi = item.Item2;
                string methodName = mi.Name;

                var setupTuple = Utils.GetSetupMethod(new[] { type });
                object setupObject = null;
                MethodInfo setupMethodInfo = null;
                if (setupTuple is not null)
                {
                    var setupType = setupTuple.Item1;
                    setupObject = Activator.CreateInstance(setupType);
                    setupMethodInfo = setupTuple.Item2;
                }

                var tearDownTuple = Utils.GetTearDownMethod(new[] { type });
                object tearDownObject = null;
                MethodInfo tearDownMethodInfo = null;
                if (tearDownTuple is not null)
                {
                    var tearDownType = tearDownTuple.Item1;
                    tearDownObject = Activator.CreateInstance(tearDownType);
                    tearDownMethodInfo = tearDownTuple.Item2;
                }

                try
                {
                    await ExecuteTest(suiteResult, testResult, type, mi, setupObject, setupMethodInfo, tearDownObject, tearDownMethodInfo).ConfigureAwait(false);
                }
                catch (SuccessException ex)
                {
                    testResult.TestName = mi.Name;
                    testResult.Passed = true;
                    testResult.ExceptionMessage = ex.Message;
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is SuccessException)
                    {
                        testResult.TestName = mi.Name;
                        testResult.Passed = true;
                        testResult.ExceptionMessage = ex.Message;
                    }
                    else if (ex.InnerException is AssertionException)
                    {
                        AssertionException ae = (AssertionException)ex.InnerException;
                        var result = ae.ResultState;
                        testResult.TestName = mi.Name;
                        testResult.Passed = false;
                        testResult.ExceptionMessage = ae.Message;
                        testResult.StackTrace = Exceptions.GetStackTraceLines(ae);
                    }
                    else
                    {
                        testResult.TestName = mi.Name;
                        testResult.Passed = false;
                        AddFullExceptionToTestResult(testResult, ex);
                    }
                }
                catch (Exception ex)
                {
                    testResult.TestName = mi.Name;
                    testResult.Passed = false;
                    if (ex.InnerException is not null)
                    {
                        AddFullExceptionToTestResult(testResult, (TargetInvocationException)ex);
                    }
                    else
                    {
                        testResult.ExceptionMessage = ex.Message;
                        testResult.StackTrace = Exceptions.GetStackTraceLines(ex);
                    }
                }

                // Get any evidence that was collected during the test
                var evidence = TestEvidenceCreator.GetEvidenceForCurrentTest();
                if (evidence is not null)
                {
                    testResult.Evidence = evidence;
                }
                suiteResult.AddResult(testResult);
            }
        }

        private static async Task ExecuteTest(ObservableTestSuiteResults suiteResult, TestResult testResult, 
            Type type, MethodInfo mi, object setupObject, 
            MethodInfo setupMethodInfo, object tearDownObject, 
            MethodInfo tearDownMethodInfo)
        {
            suiteResult.AddMessage(Environment.NewLine + "Running test method: " + mi.Name);
            object o = Activator.CreateInstance(type);
            if (setupMethodInfo is not null && setupObject is not null)
            {
                setupMethodInfo.Invoke(setupObject, null);
            }

            bool isAwaitable = mi.ReturnType.GetMethod(nameof(Task.GetAwaiter)) is not null;
            object result = mi.Invoke(o, null);

            if (isAwaitable && result is Task task)
            {
                await task.ConfigureAwait(false);
                if (task.GetType().IsGenericType && task.GetType().GetGenericTypeDefinition() == typeof(Task<>))
                {
                    result = task.GetType().GetProperty("Result")?.GetValue(task);
                }
            }

            testResult.TestName = mi.Name;
            testResult.Passed = true;

            if (tearDownMethodInfo is not null && tearDownObject is not null)
            {
                tearDownMethodInfo.Invoke(tearDownObject, null);
            }
        }

        private static void AddFullExceptionToTestResult(TestResult testResult, TargetInvocationException ex)
        {
            testResult.ExceptionMessage = ex.Message + " InnerMessage:" + ex.InnerException.Message;
            var stackTrace = Exceptions.GetStackTraceLines(ex);
            stackTrace.Add("InnerException:");
            stackTrace.AddRange(Exceptions.GetStackTraceLines(ex.InnerException));
            testResult.StackTrace = stackTrace;
        }
    }
}