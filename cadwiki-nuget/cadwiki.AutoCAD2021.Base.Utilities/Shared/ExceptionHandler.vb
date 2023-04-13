Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.EditorInput

Public Class ExceptionHandler
    Public Shared Sub WriteToEditor(ex As Exception)
        Dim doc As Document = Core.Application.DocumentManager.MdiActiveDocument
        Dim ed As Editor = doc.Editor
        Dim list As List(Of String) = NetUtils.Exceptions.GetPrettyStringList(ex)
        For Each str As String In list
            ed.WriteMessage(Environment.NewLine.ToString() & str)
        Next
    End Sub



End Class
