Imports Application = System.Windows.Forms.Application
Imports System.Windows.Controls
Imports System.Windows.Media
Imports cadwiki.NetUtils
Imports cadwiki.NUnitTestRunner.Results

Public Class CommonUiObject
    ReadOnly converter As BrushConverter = New BrushConverter()
    Public ReadOnly Green As Brush = CType(converter.ConvertFromString("#00FF00"), Brush)
    Public ReadOnly Red As Brush = CType(converter.ConvertFromString("#FF0000"), Brush)



    Public Sub AddTreeViewItemForTestResult(testResult As TestResult, treeView As TreeView)
        Dim tvi As TreeViewItem = New TreeViewItem()
        tvi.Header = testResult.TestName
        If testResult.Passed Then
            tvi.Items.Add("Passed: " + testResult.TestName)
            tvi.Background = Green
        Else
            tvi.Items.Add("Failed: " + testResult.TestName)
            tvi.Background = Red
            tvi.Items.Add("Exception: " + testResult.ExceptionMessage)
            Dim stackTraceString As String = Lists.StringListToString(testResult.StackTrace, vbLf)
            tvi.Items.Add("Stack trace: " + stackTraceString)
        End If
        treeView.Items.Add(tvi)
        treeView.Items.Refresh()
        Application.DoEvents()
    End Sub

    Public Sub AddResultsToTreeView(observableResults As ObservableTestSuiteResults,
                                   treeView As TreeView)
        Dim tvi As TreeViewItem = CreateResultsItem(observableResults)
        treeView.Items.Add(tvi)
    End Sub

    Public Sub UpdateResultsToTreeView(observableResults As ObservableTestSuiteResults,
                                      treeView As TreeView)
        Dim tvi As TreeViewItem = CreateResultsItem(observableResults)
        treeView.Items.Item(0) = tvi
    End Sub

    Private Function CreateResultsItem(
            observableResults As ObservableTestSuiteResults) As TreeViewItem
        Dim tvi As TreeViewItem = New TreeViewItem()
        tvi.Header = "Test Run Results: " + observableResults.TimeElapsed
        tvi.Items.Add("Total: " + observableResults.TotalTests.ToString())
        tvi.Items.Add("Passed: " + observableResults.PassedTests.ToString())
        tvi.Items.Add("Failed: " + observableResults.FailedTests.ToString())
        tvi.Items.Add("Time Elapsed: " + observableResults.TimeElapsed)
        tvi.IsExpanded = True
        Return tvi
    End Function

End Class
