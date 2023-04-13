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
    End Class

End Namespace
