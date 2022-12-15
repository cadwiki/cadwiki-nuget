Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices

Public Class ObjectIds
    Public Shared Function GetEntity(doc As Document, objectId As ObjectId) As Entity
        Dim deletedEntities As New List(Of Entity)
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using t As Transaction = db.TransactionManager.StartTransaction()
                Dim entity As Entity = CType(t.GetObject(objectId, OpenMode.ForRead), Entity)
                Return entity
            End Using
        End Using
    End Function
End Class
