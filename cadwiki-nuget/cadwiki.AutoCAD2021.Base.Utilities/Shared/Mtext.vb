Option Strict On
Option Infer Off
Option Explicit On

Imports System.Runtime.CompilerServices
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports Autodesk.AutoCAD.Runtime

Public Class Mtexts
    Public Class Settings
        Public Location As Point3d
        Public Height As Double
        Public Content As String
    End Class

    Public Shared Function Add(settings As Settings) As MText
        ' Get the current document and database
        Dim doc As Document = Application.DocumentManager.MdiActiveDocument
        Dim db As Database = doc.Database

        ' Start a transaction
        Using trans As Transaction = db.TransactionManager.StartTransaction()
            ' Open the Block table for read
            Dim bt As BlockTable = CType(trans.GetObject(db.BlockTableId, OpenMode.ForRead), BlockTable)

            ' Open the Model Space block table record for write
            Dim ms As BlockTableRecord = CType(trans.GetObject(bt(BlockTableRecord.ModelSpace), OpenMode.ForWrite), BlockTableRecord)

            ' Create a new text entity
            Dim mtext As New MText()

            mtext.Location = settings.Location
            mtext.Height = settings.Height
            mtext.Contents = settings.Content

            ' Add the text entity to the Model Space block table record
            ms.AppendEntity(mtext)
            trans.AddNewlyCreatedDBObject(mtext, True)

            ' Commit the transaction
            trans.Commit()

            Return mtext
        End Using
        Return Nothing

    End Function
End Class
