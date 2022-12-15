Option Strict On
Option Infer Off
Option Explicit On

Imports Autodesk.AutoCAD.BoundaryRepresentation
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry

Public Class Polylines
    Public Shared Function IsPointInside(ByVal pline As Polyline, ByVal point As Point3d) As Boolean
        Dim dbCollection As DBObjectCollection = New DBObjectCollection()
        dbCollection.Add(pline)

        Dim regionCollection As DBObjectCollection = Region.CreateFromCurves(dbCollection)
        Dim acRegion As Region = CType(regionCollection(0), Region)

        Dim pointCont As PointContainment = New PointContainment

        Dim brep As Brep = New Brep(acRegion)

        brep.GetPointContainment(point, pointCont)
        If Not pointCont = PointContainment.Outside Then
            Return True
        End If
        Return False
    End Function
End Class
