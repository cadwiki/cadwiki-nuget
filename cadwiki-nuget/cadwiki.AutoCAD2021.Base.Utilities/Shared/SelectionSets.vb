Option Strict On
Option Infer Off
Option Explicit On

Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry

Public Class SelectionSets
    Public Shared Function Merge(doc As Document, ss As SelectionSet, ssList As List(Of SelectionSet)) As SelectionSet
        Dim tempIds() As ObjectId = New ObjectId() {}
        If ss IsNot Nothing Then
            tempIds = ss.GetObjectIds()
        End If
        Dim mergedIdCollection As ObjectIdCollection = New ObjectIdCollection()
        If tempIds.Count > 0 Then
            mergedIdCollection = New ObjectIdCollection(tempIds)
        End If

        For Each sSet As SelectionSet In ssList
            If sSet IsNot Nothing Then
                tempIds = sSet.GetObjectIds()
            Else
                tempIds = Nothing
            End If
            If tempIds IsNot Nothing Then
                Dim objectIdCollection2 As ObjectIdCollection = New ObjectIdCollection(tempIds)
                Dim unionIds As IEnumerable(Of ObjectId) = Union(mergedIdCollection, objectIdCollection2)
                mergedIdCollection = New ObjectIdCollection(unionIds.ToArray())
            End If
        Next
        Dim objectIds() As ObjectId = mergedIdCollection.Cast (Of ObjectId)().ToArray()
        Dim ssReturn As SelectionSet = SelectionSet.FromObjectIds(objectIds)
        Return ssReturn
    End Function

    Public Shared Function Union(ByVal ids As ObjectIdCollection, ByVal otherIds As ObjectIdCollection) _
        As IEnumerable(Of ObjectId)
        Return ids.Cast (Of ObjectId)().ToArray().Union(otherIds.Cast (Of ObjectId))
    End Function

    Public Shared Function UnionWithIds(ByVal ids As ObjectIdCollection, ByVal otherIds As IEnumerable(Of ObjectId)) _
        As IEnumerable(Of ObjectId)
        Return ids.Cast (Of ObjectId).Union(otherIds)
    End Function

    Public Shared Function DeleteAllEntities(doc As Document, ss As SelectionSet) As List(Of Entity)
        Dim deletedEntities As New List(Of Entity)
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using t As Transaction = db.TransactionManager.StartTransaction()
                Dim currentSpace As BlockTableRecord = CType(t.GetObject(db.CurrentSpaceId, OpenMode.ForWrite),
                    BlockTableRecord)
                For Each objId As ObjectId In ss.GetObjectIds
                    Dim entity As Entity = CType(t.GetObject(objId, OpenMode.ForWrite), Entity)
                    entity.Erase()
                    deletedEntities.Add(entity)
                Next
                t.Commit()
            End Using
        End Using
        Return deletedEntities
    End Function

    Public Shared Function SelectFence(doc As Document, polygon As Point3dCollection, filter As SelectionFilter) _
        As SelectionSet
        Dim selectionResult As PromptSelectionResult = doc.Editor.SelectFence(polygon, filter)
        Dim selectionSet As SelectionSet = selectionResult.Value
        If selectionResult.Status = PromptStatus.Cancel Then
            doc.Editor.WriteMessage(vbLf + "Nothing selected")
            Return Nothing
        End If
        Return selectionSet
    End Function

    Public Shared Function SelectFence(doc As Document, pt1 As Point3d, pt2 As Point3d, filter As SelectionFilter) _
        As SelectionSet
        Dim polygon As New Point3dCollection
        polygon.Add(pt1)
        polygon.Add(pt2)
        Dim selectionResult As PromptSelectionResult = doc.Editor.SelectFence(polygon, filter)
        Dim selectionSet As SelectionSet = selectionResult.Value
        If selectionResult.Status = PromptStatus.Cancel Then
            doc.Editor.WriteMessage(vbLf + "Nothing selected")
            Return Nothing
        End If
        Return selectionSet
    End Function

    Public Shared Function SelectAll(doc As Document, filter As SelectionFilter) As SelectionSet
        Dim selectionResult As PromptSelectionResult = doc.Editor.SelectAll(filter)
        Dim selectionSet As SelectionSet = selectionResult.Value
        If selectionResult.Status = PromptStatus.Cancel Then
            doc.Editor.WriteMessage(vbLf + "Nothing selected")
            Return Nothing
        End If
        Return selectionSet
    End Function

    Public Shared Function CrossingWindow(doc As Document,
        point1 As Point3d,
        point2 As Point3d,
        filter As SelectionFilter) As SelectionSet
        Dim selectionResult As PromptSelectionResult = doc.Editor.SelectCrossingWindow(point1, point2, filter)
        Dim selectionSet As SelectionSet = selectionResult.Value
        If selectionResult.Status = PromptStatus.Cancel Then
            doc.Editor.WriteMessage(vbLf + "Nothing selected")
            Return Nothing
        End If
        Return selectionSet
    End Function

    Public Shared Function ObjectIdListToSs(objectIdList As List(Of ObjectId)) As SelectionSet
        Dim objectIdArray() As ObjectId = objectIdList.Cast (Of ObjectId)().ToArray()
        Dim ssReturn As SelectionSet = SelectionSet.FromObjectIds(objectIdArray)
        Return ssReturn
    End Function

    Public Shared Sub HighlightAll(doc As Document, ss As SelectionSet, highlight As Boolean)
        Dim deletedEntities As New List(Of Entity)
        Dim db As Database = doc.Database

        Using lock As DocumentLock = doc.LockDocument()
            Using t As Transaction = db.TransactionManager.StartTransaction()
                Dim currentSpace As BlockTableRecord = CType(t.GetObject(db.CurrentSpaceId, OpenMode.ForWrite),
                    BlockTableRecord)
                For Each objId As ObjectId In ss.GetObjectIds
                    Dim entity As Entity = CType(t.GetObject(objId, OpenMode.ForWrite), Entity)
                    If highlight Then
                        entity.Highlight()
                    Else
                        entity.Unhighlight()
                    End If
                Next
                t.Commit()
            End Using
        End Using
    End Sub

    Public Shared Function GetEntityByHandle(doc As Document, ss As SelectionSet, handle As String) As Entity
        Dim deletedEntities As New List(Of Entity)
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using t As Transaction = db.TransactionManager.StartTransaction()
                Dim currentSpace As BlockTableRecord = CType(t.GetObject(db.CurrentSpaceId, OpenMode.ForWrite),
                    BlockTableRecord)
                For Each objId As ObjectId In ss.GetObjectIds
                    Dim entity As Entity = CType(t.GetObject(objId, OpenMode.ForWrite), Entity)
                    If entity.Handle.ToString().Equals(handle) Then
                        Return entity
                    End If
                Next
                t.Commit()
            End Using
        End Using
        Return Nothing
    End Function
End Class

