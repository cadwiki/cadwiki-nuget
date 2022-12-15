Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry

Public Class Lines
    Public Shared Function TruncateEndpointsToTwoDecimalPlaces(line As Line) As Line
        Dim truncatedStart As Point3d = Points.ToTwoDecimalPlaces(line.StartPoint)
        Dim truncatedEnd As Point3d = Points.ToTwoDecimalPlaces(line.EndPoint)
        Dim truncatedLine As Line = New Line(truncatedStart, truncatedEnd)
        Return truncatedLine
    End Function
End Class
