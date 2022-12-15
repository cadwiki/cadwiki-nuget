Option Strict On
Option Infer Off
Option Explicit On

Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput

Public Class Layers
    Public Shared Function CopyVisibleEntitiesToNewLayer(doc As Document,
        ss As SelectionSet,
        newLayer As LayerTableRecord) As List(Of Entity)
        Dim copiedEntities As New List(Of Entity)
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using t As Transaction = db.TransactionManager.StartTransaction()
                Dim currentSpace As BlockTableRecord = CType(t.GetObject(db.CurrentSpaceId, OpenMode.ForWrite),
                    BlockTableRecord)
                For Each objId As ObjectId In ss.GetObjectIds
                    Dim entity As Entity = CType(t.GetObject(objId, OpenMode.ForRead), Entity)
                    If entity IsNot Nothing AndAlso entity.Visible Then
                        Dim newEntity As Entity = TryCast(entity.Clone(), Entity)
                        newEntity.LayerId = newLayer.Id
                        Dim objectId As ObjectId = currentSpace.AppendEntity(newEntity)
                        t.AddNewlyCreatedDBObject(newEntity, True)
                        copiedEntities.Add(newEntity)
                    End If
                Next
                t.Commit()
            End Using
        End Using
        Return copiedEntities
    End Function

    Public Shared Sub ThrowErrorIfLayerDoesNotExist(doc As Document, layerName As String)
        Dim layer As LayerTableRecord = Layers.GetLayer(doc, layerName)
        If layer Is Nothing Then
            Throw New Exception("Layer " + layerName + " does not exist in dwg.")
        End If
    End Sub

    Public Shared Function CreateFirstAvailableLayerName(doc As Document, layerName As String) As LayerTableRecord
        Dim i As Integer = 0
        Dim currentLayerName As String = layerName
        Dim layerExists As Boolean = DoesLayerExist(doc, currentLayerName)
        While layerExists
            i = i + 1
            currentLayerName = layerName + "(" + i.ToString + ")"
            layerExists = DoesLayerExist(doc, currentLayerName)
        End While
        Dim layerTableRecord As LayerTableRecord = CreateLayer(doc, currentLayerName)
        Return layerTableRecord
    End Function

    Public Shared Function DoesLayerExist(doc As Document, layerName As String) As Boolean
        Dim layerTableRecord As LayerTableRecord = GetLayer(doc, layerName)
        If layerTableRecord Is Nothing Then
            Return False
        End If
        Return True
    End Function

    Public Shared Function CreateLayer(doc As Document, layerName As String) As LayerTableRecord
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using transaction As Transaction = db.TransactionManager.StartTransaction()
                Dim dbObject As DBObject = transaction.GetObject(db.LayerTableId, OpenMode.ForWrite)
                Dim layerTable As LayerTable = CType(dbObject, LayerTable)

                If layerTable.Has(layerName) Then
                    Dim layerId As ObjectId = layerTable.Item(layerName)
                    Dim layerObject As DBObject = transaction.GetObject(layerId, OpenMode.ForRead)
                    Dim layerTableRecord As LayerTableRecord = CType(layerObject, LayerTableRecord)
                    Return layerTableRecord
                Else
                    Dim newRecord As New LayerTableRecord
                    newRecord.Name = layerName
                    layerTable.Add(newRecord)
                    transaction.AddNewlyCreatedDBObject(newRecord, True)
                    transaction.Commit()
                    Return newRecord
                End If

            End Using
        End Using
    End Function


    Public Shared Function GetLayer(doc As Document, layerName As String) As LayerTableRecord
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using transaction As Transaction = db.TransactionManager.StartTransaction()
                Dim dbObject As DBObject = transaction.GetObject(db.LayerTableId, OpenMode.ForRead)
                Dim layerTable As LayerTable = CType(dbObject, LayerTable)
                If layerTable.Has(layerName) Then
                    Dim layerId As ObjectId = layerTable.Item(layerName)
                    Dim layerObject As DBObject = transaction.GetObject(layerId, OpenMode.ForRead)
                    Dim layerTableRecord As LayerTableRecord = CType(layerObject, LayerTableRecord)
                    transaction.Commit()
                    Return layerTableRecord
                End If

            End Using
        End Using
        Return Nothing
    End Function

    Public Shared Function Delete(doc As Document, layerName As String) As Boolean
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using transaction As Transaction = db.TransactionManager.StartTransaction()
                Dim dbObject As DBObject = transaction.GetObject(db.LayerTableId, OpenMode.ForRead)
                Dim layerTable As LayerTable = CType(dbObject, LayerTable)
                If layerTable.Has(layerName) Then
                    Dim layerId As ObjectId = layerTable.Item(layerName)
                    Dim layerObject As DBObject = transaction.GetObject(layerId, OpenMode.ForWrite)
                    Dim layerTableRecord As LayerTableRecord = CType(layerObject, LayerTableRecord)
                    layerTableRecord.Erase(True)
                    transaction.Commit()
                    Return True
                End If

            End Using
        End Using
        Return False
    End Function

    Public Shared Sub SetLayerCurrent(doc As Document, layerName As String)
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using transaction As Transaction = db.TransactionManager.StartTransaction()
                Dim dbObject As DBObject = transaction.GetObject(db.LayerTableId, OpenMode.ForRead)
                Dim layerTable As LayerTable = CType(dbObject, LayerTable)
                If layerTable.Has(layerName) = True Then
                    db.Clayer = layerTable(layerName)
                    transaction.Commit()
                End If
            End Using
        End Using
    End Sub

    Public Shared Sub TurnAllLayersOfExcept(doc As Document, layerNames As List(Of String))
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using transaction As Transaction = db.TransactionManager.StartTransaction()
                Dim dbObject As DBObject = transaction.GetObject(db.LayerTableId, OpenMode.ForWrite)
                Dim layerTable As LayerTable = CType(dbObject, LayerTable)
                For Each id As ObjectId In layerTable
                    Dim layerTableRecord As LayerTableRecord = CType(transaction.GetObject(id, OpenMode.ForWrite),
                        LayerTableRecord)
                    Dim layerName As String = layerTableRecord.Name
                    If Not layerNames.Contains(layerName) Then
                        layerTableRecord.IsOff = True
                    End If
                Next
                transaction.Commit()
            End Using
        End Using
    End Sub
End Class
