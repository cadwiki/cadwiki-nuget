Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.EditorInput

Public Class ExceptionHandler
    Public Shared Sub Handle(ex As Exception)
        Dim doc As Document = Core.Application.DocumentManager.MdiActiveDocument
        Dim ed As Editor = doc.Editor
        ed.WriteMessage(Environment.NewLine.ToString() & "Exception: " + ex.Message)
    End Sub

End Class
