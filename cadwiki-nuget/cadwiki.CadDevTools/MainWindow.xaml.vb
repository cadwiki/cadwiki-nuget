Imports System.IO
Imports cadwiki.NetUtils
Imports cadwiki.AutoCAD2021.Interop.Utilities

Class MainWindow
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        Dim window As Window = New cadwiki.CadDevTools2.MainWindow()
        window.Show()
    End Sub

End Class
