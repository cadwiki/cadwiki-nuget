
Class MainWindow
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        Me.Hide()
        Dim dependencies As New cadwiki.CadDevTools.MainWindow.Dependencies()
        dependencies.AutoCADExePath = "E:\Program Files\Autodesk\AutoCAD 2021\acad.exe"
        dependencies.AutoCADStartupSwitches = "/p VANILLA"
        dependencies.DllFilePathToNetload = "E:\GitHub\cadwiki\cadwiki-nuget\cadwiki-nuget\cadwiki.CadDevTools\bin\Debug\cadwiki.CadDevTools.dll"
        Dim window As Window = New cadwiki.CadDevTools.MainWindow(dependencies)
        window.Show()
    End Sub

End Class
