Option Strict On
Option Infer Off
Option Explicit On

Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry

Public Class UserInput
    Public Class PointFromUser
        Public Point As Point3d
        Public Success As Boolean
    End Class

    Public Shared Function GetEntityFromUser(doc As Document, promptEntityOptions As PromptEntityOptions) As ObjectId
        Dim entityResult As PromptEntityResult = doc.Editor.GetEntity(promptEntityOptions)
        Dim id As ObjectId = entityResult.ObjectId
        If entityResult.Status = PromptStatus.Cancel Then
            Return Nothing
        End If
        Return id
    End Function

    Public Shared Function GetPointFromUser(doc As Document, promptPointOptions As PromptPointOptions) As PointFromUser
        Dim promptPointResult As PromptPointResult = doc.Editor.GetPoint(promptPointOptions)
        Dim point3d As Point3d = promptPointResult.Value
        Dim pointFromUser As PointFromUser = New PointFromUser
        pointFromUser.Point = point3d
        pointFromUser.Success = True
        If promptPointResult.Status = PromptStatus.Cancel Then
            pointFromUser.Success = False
        End If
        Return pointFromUser
    End Function

    Public Shared Function GetSelectionFromUser(doc As Document, message As String) As SelectionSet
        doc.Editor.WriteMessage(vbLf + message + vbLf)
        Dim promptSelectionResult As PromptSelectionResult = doc.Editor.GetSelection()
        Dim selectionSet As SelectionSet = promptSelectionResult.Value
        If promptSelectionResult.Status = PromptStatus.Cancel Then
            Return Nothing
        End If
        Return selectionSet
    End Function

    Public Shared Function GetSelectionFromUser(doc As Document, filter As SelectionFilter, message As String) _
        As SelectionSet
        doc.Editor.WriteMessage(vbLf + message + vbLf)
        Dim promptSelectionResult As PromptSelectionResult = doc.Editor.GetSelection(filter)
        Dim selectionSet As SelectionSet = promptSelectionResult.Value
        If promptSelectionResult.Status = PromptStatus.Cancel Then
            Return Nothing
        End If
        Return selectionSet
    End Function


    Public Shared Function GetIntegerFromUser(doc As Document, options As PromptDoubleOptions) As Integer
        Dim promptDoubleResult As PromptDoubleResult = doc.Editor.GetDouble(options)
        If promptDoubleResult.Status = PromptStatus.OK Then
            Dim value As Double = promptDoubleResult.Value
            Dim int As Integer = CType(value, Integer)
            Return int
        End If
        Return Nothing
    End Function
End Class
