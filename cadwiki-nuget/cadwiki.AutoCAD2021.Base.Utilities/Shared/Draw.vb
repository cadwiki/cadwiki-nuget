Option Explicit On
Option Strict On
Option Infer Off

Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry

Public Class Draw
    Public Shared Function Rectangle(doc As Document, layerName As String, pt1 As Point2d, pt2 As Point2d) As ObjectId
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using t As Transaction = db.TransactionManager.StartTransaction()
                Dim x1 As Double = pt1.X
                Dim y1 As Double = pt1.Y
                Dim x2 As Double = pt2.X
                Dim y2 As Double = pt2.Y
                Dim pline As Polyline = New Polyline()
                pline.AddVertexAt(0, New Point2d(x1, y1), 0.0, 0.0, 0.0)
                pline.AddVertexAt(0, New Point2d(x2, y1), 0.0, 0.0, 0.0)
                pline.AddVertexAt(0, New Point2d(x2, y2), 0.0, 0.0, 0.0)
                pline.AddVertexAt(0, New Point2d(x1, y2), 0.0, 0.0, 0.0)
                pline.Layer = layerName
                pline.Closed = True
                pline.TransformBy(doc.Editor.CurrentUserCoordinateSystem)
                Dim curSpace As BlockTableRecord = CType(t.GetObject(db.CurrentSpaceId, OpenMode.ForWrite),
                    BlockTableRecord)
                Dim objectId As ObjectId = curSpace.AppendEntity(pline)
                t.AddNewlyCreatedDBObject(pline, True)
                t.Commit()
                Return objectId
            End Using
        End Using

        Return Nothing
    End Function

    Public Shared Function LineInCurrentSpace(doc As Document, line As Line) As ObjectId
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using t As Transaction = db.TransactionManager.StartTransaction()
                Dim curSpace As BlockTableRecord = CType(t.GetObject(db.CurrentSpaceId, OpenMode.ForWrite),
                    BlockTableRecord)
                Dim objectId As ObjectId = curSpace.AppendEntity(line)
                t.AddNewlyCreatedDBObject(line, True)
                t.Commit()
                Return objectId
            End Using
        End Using
        Return Nothing
    End Function

    'https://forums.autodesk.com/t5/net/how-to-draw-line-with-angle-vb-net-2005/td-p/1776262?attachment-id=382
    Public Shared Function LineByRadians(doc As Document,
        ByVal startPoint As Point3d,
        ByVal radians As Double,
        ByVal length As Double) As Line
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Dim endpoint As Point3d = Points.PolarPoint(startPoint, radians, length)
            startPoint = Points.TransformByUCS(startPoint, db)
            endpoint = Points.TransformByUCS(endpoint, db)
            Using t As Transaction = db.TransactionManager.StartTransaction
                Try
                    Dim curSpace As BlockTableRecord = CType(t.GetObject(db.CurrentSpaceId, OpenMode.ForWrite),
                        BlockTableRecord)
                    Dim line As Line = New Line(startPoint, endpoint)
                    Dim id As ObjectId = curSpace.AppendEntity(line)
                    db.TransactionManager.AddNewlyCreatedDBObject(line, True)
                    t.Commit()
                    Return line
                Catch ex As Exception
                    t.Abort()
                End Try
            End Using
        End Using
        Return Nothing
    End Function
End Class

