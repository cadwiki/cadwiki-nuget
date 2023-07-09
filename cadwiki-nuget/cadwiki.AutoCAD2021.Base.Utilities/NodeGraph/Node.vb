Option Strict On
Option Infer Off
Option Explicit On
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry

Namespace NodeGraph
    Public Class Node
        Public NodeId As Integer
        Public ParentNode As Node
        Public EntitiesListAtNode As New List(Of Entity)
        Public NeighborList As New List(Of Node)
        Public NodeCoordinates As Point3d
        Public HasNodeBeenDiscovered As Boolean
        Public DistanceFromDestination As Double
        Public AutoCADPoint As Point3d

        Public Sub New(nodeId As Integer, parentNode As Node, entitiesListAtNode As List(Of Entity), neighborIdList As List(Of Node), nodeCoordinates As Point3d, hasNodeBeenDiscovered As Boolean, location As Point3d, distanceFromDestination As Double)
            Me.NodeId = nodeId
            Me.ParentNode = parentNode
            Me.EntitiesListAtNode = entitiesListAtNode
            Me.NeighborList = neighborIdList
            Me.NodeCoordinates = nodeCoordinates
            Me.HasNodeBeenDiscovered = hasNodeBeenDiscovered
            Me.AutoCADPoint = location
            Me.DistanceFromDestination = distanceFromDestination
        End Sub

        Public Function GetNeighborIds() As List(Of Integer)
            If NeighborList IsNot Nothing Then
                Dim neighborIds As List(Of Integer) = NeighborList.Select(Function(p) p.NodeId).ToList()
                Return neighborIds
            End If
            Return New List(Of Integer)
        End Function

        Public Function GetNeighborIdsToStringList() As List(Of String)
            Dim neighborIds As List(Of Integer) = GetNeighborIds()
            Dim neighborStrings As New List(Of String)

            For Each id As Integer In neighborIds
                neighborStrings.Add(id.ToString)
            Next
            Return neighborStrings
        End Function
    End Class

End Namespace
