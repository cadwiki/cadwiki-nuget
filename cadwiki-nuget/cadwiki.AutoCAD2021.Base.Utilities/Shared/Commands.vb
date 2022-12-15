Option Strict On
Option Infer Off
Option Explicit On

Imports System.Runtime.CompilerServices
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports Autodesk.AutoCAD.Runtime

Public Class Commands

    Public Shared Sub ExecuteInApplicationContext(parameters As List(Of Object))
        Dim dm As DocumentCollection = Core.Application.DocumentManager
        Dim doc As Document = dm.CurrentDocument
        dm.ExecuteInApplicationContext(
                Sub(obj)
                    doc.Editor.Command(parameters.ToArray())
                End Sub, Nothing)
    End Sub

    Public Shared Sub SendLispCommandStartUndoMark()
        Dim str As String = "(vla-startundomark (vla-get-ActiveDocument (vlax-get-acad-object)))"
        Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute(
            str + Constants.vbLf, True, False, False)
    End Sub

    Public Shared Sub SendLispCommandEndUndoMark()
        Dim str As String = "(vla-endundomark (vla-get-ActiveDocument (vlax-get-acad-object)))"
        Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute(
            str + Constants.vbLf, True, False, False)
    End Sub

    Public Shared Sub SendLispCommandUndoBack()
        Dim str As String = "(command-s ""._undo"" ""back"" ""yes"")"
        Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute(
            str + Constants.vbLf, True, False, False)
    End Sub
End Class
