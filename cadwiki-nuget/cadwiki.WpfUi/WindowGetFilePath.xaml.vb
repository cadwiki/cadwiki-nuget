
Imports System.Windows.Controls

Class WindowGetFilePath

    Public WasOkayClicked As Boolean
    Public SelectedFolder As String = ""

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
    End Sub

    Public Shared Sub LoadXaml(ByVal obj As Object)
        Dim type As Type = obj.GetType()
        Dim assemblyName As Reflection.AssemblyName = type.Assembly.GetName()
        Dim uristring As String = String.Format("/{0};v{1};component/{2}.xaml", assemblyName.Name, assemblyName.Version, type.Name)
        Dim uri As Uri = New Uri(uristring, UriKind.Relative)
        System.Windows.Application.LoadComponent(obj, uri)
    End Sub

    Public Sub New(filePaths As List(Of String))
        'Might work?
        'https://stackoverflow.com/questions/1453107/how-to-force-wpf-to-use-resource-uris-that-use-assembly-strong-name-argh
        _contentLoaded = True
        LoadXaml(Me)
        InitializeComponent()
        For Each filePath As String In filePaths
            ListBoxFolderPaths.Items.Add(filePath)
        Next
    End Sub

    Private Sub ButtonBrowseFolder_Click(sender As Object, e As Windows.RoutedEventArgs)
        Dim dialog As System.Windows.Forms.OpenFileDialog = New System.Windows.Forms.OpenFileDialog()
        dialog.Title = "Select acad.exe to launch: "
        dialog.ShowDialog()
        Dim newPath As String = dialog.FileName
        If System.IO.File.Exists(newPath) Then
            Dim index As Integer = ListBoxFolderPaths.Items.Add(newPath)
            ListBoxFolderPaths.SelectedIndex = index
            TextBlockStatus.Text = "File added to list: " + newPath
        Else
            TextBlockStatus.Text = "File does not exist: " + newPath
        End If
        SelectedFolder = newPath
    End Sub

    Private Sub ListBoxFolderPaths_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
        UpdateSelectedFolderFromListBox()
    End Sub

    Private Sub UpdateSelectedFolderFromListBox()
        Dim selectedPath As String = CType(ListBoxFolderPaths.SelectedItem, String)

        If selectedPath IsNot Nothing Then
            If System.IO.File.Exists(selectedPath) Then
                TextBlockStatus.Text = "File does exist: " + selectedPath
                SelectedFolder = selectedPath
            Else
                TextBlockStatus.Text = "File does not exist: " + selectedPath
                SelectedFolder = selectedPath
            End If
        End If
    End Sub

    Private Sub ButtonOk_Click(sender As Object, e As Windows.RoutedEventArgs)
        WasOkayClicked = True
        UpdateSelectedFolderFromListBox()
        Close()
    End Sub

    Private Sub ButtonCancel_Click(sender As Object, e As Windows.RoutedEventArgs)
        UpdateSelectedFolderFromListBox()
        Close()
    End Sub

End Class
