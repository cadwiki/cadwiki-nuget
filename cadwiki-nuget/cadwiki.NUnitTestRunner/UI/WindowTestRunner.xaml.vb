Option Strict On
Option Infer Off
Option Explicit On

Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports cadwiki.NetUtils
Imports Application = System.Windows.Forms.Application
Imports cadwiki.NUnitTestRunner.Results
Imports System.Reflection

Namespace Ui
    Public Class WindowTestRunner
        Public WithEvents ObservableResults As New ObservableTestSuiteResults


        ReadOnly converter As BrushConverter = New BrushConverter()
        Public ReadOnly Green As Brush = CType(converter.ConvertFromString("#00FF00"), Brush)
        Public ReadOnly Red As Brush = CType(converter.ConvertFromString("#FF0000"), Brush)

        Public Shared Sub LoadXaml(ByVal obj As Object)
            Dim type As Type = obj.GetType()
            Dim assemblyName As AssemblyName = type.Assembly.GetName()
            Dim uristring As String = String.Format("/{0};v{1};component/{2}.xaml", assemblyName.Name, assemblyName.Version, type.Name)
            Dim uri As Uri = New Uri(uristring, UriKind.Relative)
            System.Windows.Application.LoadComponent(obj, uri)
        End Sub


        Private Sub TestMessages_OnChanged(sender As Object, e As EventArgs) Handles ObservableResults.MessageAdded
            Dim suiteResults As Results.ObservableTestSuiteResults = CType(sender, ObservableTestSuiteResults)
            Dim messages As List(Of String) = suiteResults.Messages
            Dim lastItem As String = messages(messages.Count - 1)
            ConsoleRichTextBox.AppendText(lastItem)
            Application.DoEvents()
        End Sub


        Private Sub TestResults_OnChanged(sender As Object, e As EventArgs) Handles ObservableResults.ResultAdded
            Dim suiteResults As ObservableTestSuiteResults = CType(sender, ObservableTestSuiteResults)
            Dim testResults As List(Of TestResult) = suiteResults.TestResults
            Dim lastItem As TestResult = testResults(testResults.Count - 1)
            Dim tvi As TreeViewItem = New TreeViewItem()
            tvi.Header = lastItem.TestName
            If lastItem.Passed Then
                tvi.Items.Add("Passed: " + lastItem.TestName)
                tvi.Background = Green
            Else
                tvi.Items.Add("Failed: " + lastItem.TestName)
                tvi.Background = Red
                tvi.Items.Add("Exception: " + lastItem.ExceptionMessage)
                Dim stackTraceString As String = Lists.StringListToString(lastItem.StackTrace, vbLf)
                tvi.Items.Add("Stack trace: " + stackTraceString)
            End If
            ResultsTree.Items.Add(tvi)
            ResultsTree.Items.Refresh()
            Application.DoEvents()
        End Sub

        Public Sub AddResult()
            Dim tvi As TreeViewItem = CreateResultItem()
            ResultsTree.Items.Add(tvi)
        End Sub

        Public Sub UpdateResult()
            Dim tvi As TreeViewItem = CreateResultItem()
            ResultsTree.Items.Item(0) = tvi
        End Sub

        Private Function CreateResultItem() As TreeViewItem
            Dim tvi As TreeViewItem = New TreeViewItem()
            tvi.Header = "Test Run Results: " + ObservableResults.TimeElapsed
            tvi.Items.Add("Total: " + ObservableResults.TotalTests.ToString())
            tvi.Items.Add("Passed: " + ObservableResults.PassedTests.ToString())
            tvi.Items.Add("Failed: " + ObservableResults.FailedTests.ToString())
            tvi.Items.Add("Time Elapsed: " + ObservableResults.TimeElapsed)
            tvi.IsExpanded = True
            Return tvi
        End Function

        Public Sub New()
            LoadXaml(Me)
            InitializeComponent()
            Init()
        End Sub

        Public Sub Init()
            ConsoleRichTextBox.AppendText(vbLf & "NunitTestRunner started")
        End Sub

        Public Sub New(suiteResults As ObservableTestSuiteResults)
            LoadXaml(Me)
            InitializeComponent()
            Init()
        End Sub

        Private Sub ButtonOk_Click(sender As Object, e As RoutedEventArgs)
            Close()
        End Sub

        Private Sub ButtonCancel_Click(sender As Object, e As RoutedEventArgs)
            Close()
        End Sub
    End Class
End Namespace

