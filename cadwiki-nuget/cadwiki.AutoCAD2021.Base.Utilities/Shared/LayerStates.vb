Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices

Public Class LayerStates
    'CType(LayerStateMasks.Color + LayerStateMasks.LineType, LayerStateMasks)
    Public Shared Function Create(doc As Document, layerStateName As String, layerStateMasks As LayerStateMasks) _
        As Boolean
        Try
            Using lock As DocumentLock = doc.LockDocument
                Dim layerStateManager As LayerStateManager = doc.Database.LayerStateManager
                Dim tempLayerStateName As String = ""
                Dim leaveMissingLayersUnchanged As Integer = 0
                If layerStateManager.HasLayerState(layerStateName) = True Then
                    tempLayerStateName = CreateFirstAvailableLayerStateName(doc, "tempLayerState", layerStateMasks)
                    layerStateManager.RestoreLayerState(tempLayerStateName,
                        ObjectId.Null,
                        leaveMissingLayersUnchanged,
                        layerStateMasks)
                    layerStateManager.DeleteLayerState(layerStateName)
                End If
                layerStateManager.SaveLayerState(layerStateName, layerStateMasks, ObjectId.Null)
                layerStateManager.RestoreLayerState(layerStateName,
                    ObjectId.Null,
                    leaveMissingLayersUnchanged,
                    layerStateMasks)
                If Not String.IsNullOrEmpty(tempLayerStateName) Then
                    layerStateManager.DeleteLayerState(tempLayerStateName)
                End If
            End Using
        Catch ex As Exception
            Throw (ex)
        End Try
        Return True
    End Function

    Public Shared Function CreateFirstAvailableLayerStateName(doc As Document,
        layerStateName As String,
        layerStateMasks As LayerStateMasks) As String
        Dim i As Integer = 0
        Dim currentLayerStateName As String = layerStateName
        Try
            Using lock As DocumentLock = doc.LockDocument
                Dim layerStateManager As LayerStateManager = doc.Database.LayerStateManager
                Dim layerStateExists As Boolean = layerStateManager.HasLayerState(layerStateName)
                While layerStateExists
                    i = i + 1
                    currentLayerStateName = layerStateName + "(" + i.ToString + ")"
                    layerStateExists = layerStateManager.HasLayerState(currentLayerStateName)
                End While
                layerStateManager.SaveLayerState(layerStateName, layerStateMasks, ObjectId.Null)
                Return currentLayerStateName
            End Using
        Catch ex As Exception
            Throw (ex)
        End Try
        Return Nothing
    End Function


    Public Shared Function GetLastRestoredLayerState(doc As Document) As String
        Dim i As Integer = 0
        Try
            Using lock As DocumentLock = doc.LockDocument
                Dim layerStateManager As LayerStateManager = doc.Database.LayerStateManager
                Return layerStateManager.LastRestoredLayerState
            End Using
        Catch ex As Exception
            Throw (ex)
        End Try
        Return Nothing
    End Function

    Public Shared Function Restore(doc As Document, layerState As String, layerStateMasks As LayerStateMasks) As Boolean
        Dim i As Integer = 0
        Try
            Using lock As DocumentLock = doc.LockDocument
                Dim layerStateManager As LayerStateManager = doc.Database.LayerStateManager
                If layerStateManager.HasLayerState(layerState) Then
                    Dim leaveMissingLayersUnchanged As Integer = 0
                    layerStateManager.RestoreLayerState(layerState,
                        ObjectId.Null,
                        leaveMissingLayersUnchanged,
                        layerStateMasks)
                    Return True
                End If
            End Using
        Catch ex As Exception
            Throw (ex)
        End Try
        Return False
    End Function
End Class
