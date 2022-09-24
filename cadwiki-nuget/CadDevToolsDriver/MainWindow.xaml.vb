
Class MainWindow
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        Me.Hide()
        Dim window As Window = New cadwiki.CadDevTools.MainWindow()
        window.Show()
    End Sub

End Class
