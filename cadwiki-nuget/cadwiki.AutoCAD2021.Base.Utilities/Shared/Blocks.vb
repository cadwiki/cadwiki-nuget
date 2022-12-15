Option Strict On
Option Infer Off
Option Explicit On

Imports System.Runtime.CompilerServices
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports Autodesk.AutoCAD.Runtime

Public Class Blocks
    Public Shared Function SortSelectionSetByProximityToPoint(doc As Document, ss As SelectionSet, point As Point3d) _
        As List(Of BlockReference)
        Dim blockRefs As New List(Of BlockReference)
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using t As Transaction = db.TransactionManager.StartTransaction()
                Dim currentSpace As BlockTableRecord = CType(t.GetObject(db.CurrentSpaceId, OpenMode.ForWrite),
                    BlockTableRecord)
                Dim objectIds As ObjectId() = ss.GetObjectIds
                For Each objId As ObjectId In objectIds
                    Dim blockRef As BlockReference = CType(t.GetObject(objId, OpenMode.ForWrite), BlockReference)
                    blockRefs.Add(blockRef)
                Next

                Dim sortedBlockRefs As IEnumerable(Of BlockReference) = From o In blockRefs
                    Order By o.Position.DistanceTo(point)
                Return sortedBlockRefs.ToList()
            End Using
        End Using
        Return Nothing
    End Function

    Public Shared Function GetAttributeValue(doc As Document, blockRef As BlockReference, attributeName As String) _
        As String
        Dim db As Database = doc.Database

        Using lock As DocumentLock = doc.LockDocument()
            Using t As Transaction = db.TransactionManager.StartTransaction()
                For Each attId As ObjectId In blockRef.AttributeCollection
                    Try
                        Dim attRef As AttributeReference = CType(t.GetObject(attId, OpenMode.ForRead, False),
                            AttributeReference)
                        If (attRef.Tag = attributeName) Then
                            Return attRef.TextString
                        End If
                    Catch ex As Exception
                        Debug.WriteLine("Attribute no longer exists because it was deleted.")
                    End Try

                Next
            End Using
        End Using


        Return Nothing
    End Function

    Public Shared Function SetEntityAttributeValue(entity As Entity, attributeName As String, newValue As String) _
        As Boolean
        Dim wasAttributeUpdated As Boolean = False
        Dim doc As Document = Application.DocumentManager.MdiActiveDocument
        Dim objectIdsToInsertionPoints As Dictionary(Of ObjectId, Point3d) = New Dictionary(Of ObjectId, Point3d)
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using transaction As Transaction = db.TransactionManager.StartTransaction()
                Dim blockRefObj As BlockReference = CType(entity, BlockReference)
                Dim attributeCollection As AttributeCollection = blockRefObj.AttributeCollection
                For Each attId As ObjectId In attributeCollection
                    Try
                        Dim obj As DBObject = transaction.GetObject(attId, OpenMode.ForWrite)
                        Dim attribute As AttributeReference = CType(obj, AttributeReference)
                        If (attribute.Tag = attributeName) Then
                            attribute.TextString = newValue
                            wasAttributeUpdated = True
                        End If
                    Catch ex As Exception
                        Debug.WriteLine("Attribute no longer exists because it was deleted.")
                    End Try

                Next
                transaction.Commit()
            End Using
        End Using
        Return wasAttributeUpdated
    End Function

    Public Shared Sub AddAttributesFromBlockTable(doc As Document,
        acBlkTblRec As BlockTableRecord,
        acBlkRef As BlockReference)
        Dim attDefClass As RXClass = RXObject.GetClass(GetType(AttributeDefinition))
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument
            Using tr As Transaction = doc.TransactionManager.StartTransaction()
                For Each objID As ObjectId In acBlkTblRec
                    If objID.ObjectClass.IsDerivedFrom(attDefClass) Then
                        Dim dbObj As DBObject = tr.GetObject(objID, OpenMode.ForRead)
                        If TypeOf dbObj Is AttributeDefinition Then
                            Dim acAtt As AttributeDefinition = CType(dbObj, AttributeDefinition)
                            If Not acAtt.Constant Then
                                Using acAttRef As New AttributeReference
                                    acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform)
                                    acAttRef.Position = acAtt.Position.TransformBy(acBlkRef.BlockTransform)
                                    acAttRef.TextString = acAtt.TextString
                                    acBlkRef.AttributeCollection.AppendAttribute(acAttRef)
                                    tr.AddNewlyCreatedDBObject(acAttRef, True)
                                End Using
                            End If
                        End If
                    End If
                Next
                tr.Commit()
            End Using
        End Using
    End Sub

    Public Shared Sub DeleteAttributes(doc As Document, acBlkRef As BlockReference)
        Dim attDefClass As RXClass = RXObject.GetClass(GetType(AttributeDefinition))
        Dim db As Database = doc.Database
        Dim purgedIds As ObjectIdCollection = New ObjectIdCollection()

        Using lock As DocumentLock = doc.LockDocument
            Using tr As Transaction = doc.TransactionManager.StartTransaction()
                If acBlkRef.AttributeCollection.Count > 0 Then
                    acBlkRef.UpgradeOpen()
                    For Each attId As ObjectId In acBlkRef.AttributeCollection
                        If attId.IsErased = False Then
                            Dim attRef As AttributeReference = CType(tr.GetObject(attId, OpenMode.ForWrite),
                                AttributeReference)
                            purgedIds.Add(attId)
                            attRef.Erase(True)
                        End If
                    Next
                End If
                db.Purge(purgedIds)
                tr.Commit()
            End Using
        End Using
    End Sub

    Public Shared Function InsertBlockFromFile(doc As Document,
        filePath As String,
        insertionPoint As Point3d,
        rotation As Double,
        layerName As String) As BlockReference
        Dim db As Database = doc.Database
        Using xDb As New Database(False, True)
            xDb.ReadDwgFile(filePath, FileOpenMode.OpenForReadAndReadShare, True, Nothing)
            Using lock As DocumentLock = doc.LockDocument
                Using tr As Transaction = doc.TransactionManager.StartTransaction()
                    Dim name As String = SymbolUtilityServices.GetBlockNameFromInsertPathName(filePath)
                    Dim id As ObjectId = db.Insert(name, xDb, True)
                    If id.IsNull Then
                        doc.Editor.WriteMessage(vbLf & "Failed to insert block")
                        Return Nothing
                    End If
                    Dim currSpace As BlockTableRecord = CType(tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite),
                        BlockTableRecord)
                    Dim coordS As CoordinateSystem3d = New CoordinateSystem3d(insertionPoint, db.Ucsxdir, db.Ucsydir) _
                    'Determine UCS
                    Dim insert As New BlockReference(insertionPoint, id)
                    insert.Normal = coordS.Zaxis 'Align to UCS
                    insert.Rotation = rotation
                    currSpace.AppendEntity(insert)
                    insert.SetDatabaseDefaults()
                    insert.Layer = layerName
                    tr.AddNewlyCreatedDBObject(insert, True)
                    Dim btr As BlockTableRecord = CType(tr.GetObject(insert.BlockTableRecord, OpenMode.ForWrite),
                        BlockTableRecord)
                    AddAttributesFromBlockTable(doc, btr, insert)
                    'Autodesk.AutoCAD.DatabaseServices.SynchronizeAttributes(btr, insert)
                    tr.Commit()
                    Return insert
                End Using
            End Using

        End Using
    End Function
End Class


Namespace Autodesk.AutoCAD.DatabaseServices
    Module ExtensionMethods
        Public attDefClass As RXClass = RXClass.GetClass(GetType(AttributeDefinition))

        <Extension()>
        Sub SynchronizeAttributes(ByVal target As BlockTableRecord, ByVal instance As BlockReference)
            If target Is Nothing Then Throw New ArgumentNullException("target")
            Dim tr As Transaction = target.Database.TransactionManager.TopTransaction
            If tr Is Nothing Then Throw New Exception(ErrorStatus.NoActiveTransactions)
            Dim attDefs As List(Of AttributeDefinition) = target.GetAttributes(tr)

            For Each id As ObjectId In target.GetBlockReferenceIds(True, False)
                Dim br As BlockReference = CType(tr.GetObject(id, OpenMode.ForWrite), BlockReference)
                br.ResetAttributes(attDefs, tr)
            Next

            If target.IsDynamicBlock = False Then
                target.UpdateAnonymousBlocks()

                For Each id As ObjectId In target.GetAnonymousBlockIds()
                    Dim btr As BlockTableRecord = CType(tr.GetObject(id, OpenMode.ForRead), BlockTableRecord)
                    attDefs = btr.GetAttributes(tr)

                    For Each brId As ObjectId In btr.GetBlockReferenceIds(True, False)
                        Dim br As BlockReference = CType(tr.GetObject(brId, OpenMode.ForWrite), BlockReference)
                        br.ResetAttributes(attDefs, tr)
                    Next
                Next

                attDefs = target.GetAttributes(tr)
                instance.ResetAttributes(attDefs, tr)

            End If
        End Sub

        <Extension()>
        Private Function GetAttributes(ByVal target As BlockTableRecord, ByVal tr As Transaction) _
            As List(Of AttributeDefinition)
            Dim attDefs As List(Of AttributeDefinition) = New List(Of AttributeDefinition)()

            For Each id As ObjectId In target

                If id.ObjectClass = attDefClass Then
                    Dim attDef As AttributeDefinition = CType(tr.GetObject(id, OpenMode.ForRead), AttributeDefinition)
                    attDefs.Add(attDef)
                End If
            Next

            Return attDefs
        End Function

        <Extension()>
        Private Sub ResetAttributes(ByVal br As BlockReference,
            ByVal attDefs As List(Of AttributeDefinition),
            ByVal tr As Transaction)
            Dim attValues As Dictionary(Of String, String) = New Dictionary(Of String, String)()

            For Each id As ObjectId In br.AttributeCollection

                If Not id.IsErased Then
                    Dim attRef As AttributeReference = CType(tr.GetObject(id, OpenMode.ForWrite), AttributeReference)
                    attValues.Add(attRef.Tag,
                        If(attRef.IsMTextAttribute, attRef.MTextAttribute.Contents, attRef.TextString))
                    attRef.[Erase]()
                End If
            Next

            For Each attDef As AttributeDefinition In attDefs
                Dim attRef As AttributeReference = New AttributeReference()
                attRef.SetAttributeFromBlock(attDef, br.BlockTransform)

                If attDef.Constant Then
                    attRef.TextString = If _
                        (attDef.IsMTextAttributeDefinition, attDef.MTextAttributeDefinition.Contents, attDef.TextString)
                ElseIf attValues.ContainsKey(attRef.Tag) Then
                    attRef.TextString = attValues(attRef.Tag)
                End If

                br.AttributeCollection.AppendAttribute(attRef)
                tr.AddNewlyCreatedDBObject(attRef, True)
            Next
        End Sub
    End Module
End Namespace
