Option Strict On
Option Infer Off
Option Explicit On

Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry

Public Class Points
    Public Shared Function RoundToNearestWholeNumber(point As Point3d) As Point3d
        Dim x As Double = Math.Round(point.X, 0)
        Dim y As Double = Math.Round(point.Y, 0)
        Dim z As Double = Math.Round(point.Z, 0)
        Dim newPoint As Point3d = New Point3d(x, y, z)

        Return newPoint
    End Function

    Public Shared Function ToTwoDecimalPlaces(point As Point3d) As Point3d
        Dim x As Double = CType(point.X.ToString("N2"), Double)
        Dim y As Double = CType(point.Y.ToString("N2"), Double)
        Dim z As Double = CType(point.Z.ToString("N2"), Double)
        Dim newPoint As Point3d = New Point3d(x, y, z)
        Return newPoint
    End Function

    Public Shared Function ToThreeDecimalPlaces(point As Point3d) As Point3d
        Dim x As Double = CType(point.X.ToString("N3"), Double)
        Dim y As Double = CType(point.Y.ToString("N3"), Double)
        Dim z As Double = CType(point.Z.ToString("N3"), Double)
        Dim newPoint As Point3d = New Point3d(x, y, z)
        Return newPoint
    End Function

    Public Shared Function ToFourDecimalPlaces(point As Point3d) As Point3d
        Dim x As Double = CType(point.X.ToString("N4"), Double)
        Dim y As Double = CType(point.Y.ToString("N4"), Double)
        Dim z As Double = CType(point.Z.ToString("N4"), Double)
        Dim newPoint As Point3d = New Point3d(x, y, z)
        Return newPoint
    End Function

    Public Shared Function PolarPoint(ByVal pPt As Point3d, ByVal radians As Double, ByVal dDist As Double) As Point3d

        Return New Point3d(pPt.X + dDist * Math.Cos(radians), pPt.Y + dDist * Math.Sin(radians), pPt.Z)
    End Function

    Public Shared Function Convert3dTo2d(point3d As Point3d) As Point2d
        Dim point2d As Point2d = New Point2d(point3d.X, point3d.Y)
        Return point2d
    End Function

    Public Shared Function MidPoint3d(pt1 As Point3d, pt2 As Point3d) As Point3d
        Dim midX As Double = (pt1.X + pt2.X) / 2.0
        Dim midY As Double = (pt1.Y + pt2.Y) / 2.0
        Dim midZ As Double = (pt1.Z + pt2.Z) / 2.0

        Dim point3d As Point3d = New Point3d(midX, midY, midZ)
        Return point3d
    End Function

    Public Shared Function GetAllVertices(ByVal polyline As Polyline) As Point2dCollection
        Dim verCollection As Point2dCollection = New Point2dCollection()
        Dim vertex As Point2d
        Dim i As Integer = 0
        For i = 0 To polyline.NumberOfVertices - 1
            vertex = polyline.GetPoint2dAt(i)
            verCollection.Add(vertex)
        Next
        Return verCollection
    End Function

    Shared Function TransformByUCS(ByVal point As Point3d, ByVal db As Database) As Point3d
        Return point.TransformBy(GetUcsMatrix(db))
    End Function

    Shared Function GetUcsMatrix(ByVal db As Database) As Matrix3d
        Dim origin As Point3d = db.Ucsorg
        Dim xAxis As Vector3d = db.Ucsxdir
        Dim yAxis As Vector3d = db.Ucsydir
        Dim zAxis As Vector3d = xAxis.CrossProduct(yAxis)
        Return _
            Matrix3d.AlignCoordinateSystem(Point3d.Origin,
                Vector3d.XAxis,
                Vector3d.YAxis,
                Vector3d.ZAxis,
                origin,
                xAxis,
                yAxis,
                zAxis)
    End Function

    Public Shared Function SortPointsByProximityToPoint(points As List(Of Point3d), point As Point3d) As List(Of Point3d)
        Dim sortedPoints As IEnumerable(Of Point3d) = From o In points
                                                      Order By o.DistanceTo(point)
        Return sortedPoints.ToList()
        Return Nothing
    End Function
End Class
