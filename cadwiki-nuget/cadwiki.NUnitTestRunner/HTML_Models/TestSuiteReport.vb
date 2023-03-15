Namespace HTML_Models
    Public Class TestSuiteReport
        Public Title As String
        Public BannerImagePath As String
        Public TabName As String
        Public PassPercentage As String
        Public DateString As String
        Public TestSuiteResults As Results.ObservableTestSuiteResults

        Public HeaderBackgroundColor As String = ""


        Public Sub New(results As Results.ObservableTestSuiteResults)
            Title = results.TestSuiteName
            TabName = Title
            PassPercentage = ((results.PassedTests / results.TotalTests * 1.0) * 100).ToString("0.00") + "%"
            DateString = DateTimeOffset.Now.ToString()
            TestSuiteResults = results
        End Sub

    End Class

    Public Class HexColors
        Public DarkBlue As String = "#002554"
        Public White As String = "#ffffff"
        Public LightBlue As String = "#0093db"
    End Class

End Namespace
