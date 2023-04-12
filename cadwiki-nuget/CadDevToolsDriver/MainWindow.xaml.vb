
Imports System.Reflection

Class MainWindow
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        Me.Hide()
        Dim exePath As String = Assembly.GetExecutingAssembly().Location
        Dim exeDir As String = System.IO.Path.GetDirectoryName(exePath)
        Dim dependencies As New cadwiki.CadDevTools.MainWindow.Dependencies()
        dependencies.AutoCADExePath = "E:\Program Files\Autodesk\AutoCAD 2021\acad.exe"
        dependencies.AutoCADStartupSwitches = "/p VANILLA"
        dependencies.DllFilePathToNetload = exeDir + "\" + "cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.dll"
        Dim window As Window = New cadwiki.CadDevTools.MainWindow(dependencies)
        window.Show()
    End Sub

End Class
