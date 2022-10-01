Option Strict On
Option Infer Off
Option Explicit On

Imports System.Windows
Imports Application = System.Windows.Forms.Application
Imports cadwiki.NUnitTestRunner.Results
Imports cadwiki.NetUtils
Imports System.Windows.Media.Imaging
Imports System.Drawing

Namespace UI
    Public Class WindowTestRunner
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
            _commonUiObject.WpfAddTreeViewItemForTestResult(mostRecentlyAddedTestResult, TreeViewResults)
        End Sub



        Public Sub New()
            InitializeComponent()
            Init()
        End Sub

        Public Sub Init()
            RichTextBoxConsole.AppendText(vbLf & "NunitTestRunner started")
            Dim bitMap As Bitmap = FileStore.My.Resources.ResourceIcons._500x500_cadwiki_v1
            Dim bitMapImage As BitmapImage = Bitmaps.BitMapToBitmapImage(bitMap)
            Me.Icon = bitMapImage
        End Sub

        Public Sub New(ByRef suiteResults As ObservableTestSuiteResults)
            ObservableResults = suiteResults
            InitializeComponent()
            Init()
        End Sub

        Private Sub ButtonOk_Click(sender As Object, e As RoutedEventArgs)
            Close()
        End Sub

        Private Sub ButtonTestEvidence_Click(sender As Object, e As RoutedEventArgs)
            Close()
        End Sub

        Private Sub ButtonCancel_Click(sender As Object, e As RoutedEventArgs)
            Close()
        End Sub
    End Class
End Namespace

