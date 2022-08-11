
Imports System.Windows.Controls

Class WindowCadWiki

    Public WasOkayClicked As Boolean

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
    End Sub

    Public Sub New(inputValueFromBusinessLogic As String)
        InitializeComponent()
        TextBoxDisplay.Text = inputValueFromBusinessLogic
    End Sub

    Private Sub ButtonOk_Click(sender As Object, e As Windows.RoutedEventArgs)
        WasOkayClicked = True
        Close()
    End Sub

    Private Sub ButtonCancel_Click(sender As Object, e As Windows.RoutedEventArgs)
        Close()
    End Sub

End Class
