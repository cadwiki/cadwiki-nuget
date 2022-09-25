Imports cadwiki.NUnitTestRunner.Results
Imports Application = System.Windows.Forms.Application

Namespace UI
    Public Class FormTestRunner
        Public WithEvents ObservableResults As New ObservableTestSuiteResults

        Private _commonUiObject As New CommonUiObject()

        Private Sub TestMessages_OnChanged(sender As Object, e As EventArgs) Handles ObservableResults.MessageAdded
            Dim suiteResults As Results.ObservableTestSuiteResults = CType(sender, ObservableTestSuiteResults)
            Dim messages As List(Of String) = suiteResults.Messages
            Dim lastItem As String = messages(messages.Count - 1)
            RichTextBoxConsole.AppendText(lastItem)
            Application.DoEvents()
        End Sub


        Private Sub TestResults_OnChanged(sender As Object, e As EventArgs) Handles ObservableResults.ResultAdded
            Dim suiteResults As ObservableTestSuiteResults = CType(sender, ObservableTestSuiteResults)
            Dim testResults As List(Of TestResult) = suiteResults.TestResults
            Dim mostRecentlyAddedTestResult As TestResult = testResults(testResults.Count - 1)
            _commonUiObject.WinFormsAddTreeViewItemForTestResult(mostRecentlyAddedTestResult, TreeViewResults)
        End Sub



        Public Sub New()
            InitializeComponent()
            Init()
        End Sub

        Public Sub Init()
            RichTextBoxConsole.AppendText(vbLf & "NunitTestRunner started")
        End Sub

        Public Sub New(ByRef suiteResults As ObservableTestSuiteResults)
            ObservableResults = suiteResults
            InitializeComponent()
            Init()
        End Sub

        Private Sub ButtonOk_Click(sender As Object, e As EventArgs) Handles ButtonOk.Click
            Me.Close()
        End Sub

        Private Sub Cancel_Click(sender As Object, e As EventArgs) Handles Cancel.Click
            Me.Close()
        End Sub
    End Class

End Namespace
