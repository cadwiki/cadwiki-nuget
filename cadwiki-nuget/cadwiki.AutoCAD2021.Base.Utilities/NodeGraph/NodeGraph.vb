Option Strict On
Option Infer Off
Option Explicit On
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports System.Linq

Namespace NodeGraph
    Public Class NodeGraph
        Public Nodes As New List(Of Node)
        Public Edges As New List(Of Edge)
        Public Document As Document

        Public Sub New(document As Document, pointList As List(Of Point3d), destination As Point3d, source As Point3d)
            Me.Nodes = Nodes
            Me.Edges = Edges
            Me.Document = document
            Dim counter As Integer = 2
            Dim nodeId As Integer = counter

            For Each point As Point3d In pointList
                Dim parentNode As Node = Nothing
                Dim entitiesListAtNode As New List(Of Entity)
                Dim neighborList As New List(Of Node)
                Dim nodeCoordinates As Point3d = Nothing
                Dim hasNodeBeenDiscovered As Boolean = False

                Dim distanceFromSource As Double = source.DistanceTo(point)
                Dim distanceFromDestination As Double = destination.DistanceTo(point)
                nodeId = counter
                If distanceFromSource = 0.0 Then
                    nodeId = 0
                ElseIf distanceFromDestination = 0.0 Then
                    nodeId = 1
                End If
                Dim node As New Node(nodeId, parentNode, entitiesListAtNode, neighborList, nodeCoordinates, hasNodeBeenDiscovered, point, distanceFromDestination)
                Nodes.Add(node)
                counter += 1
            Next
        End Sub

        Public Sub AddNeighborsToNodes(layerNameToSelectFrom As String)
            Dim index As Integer = 0
            While index < Me.Nodes.Count - 1
                Dim node As Node = Me.Nodes(index)
                Dim newNode As Node = Me.AddNeighborsToNode(node, layerNameToSelectFrom)
                Me.ReplaceNode(index, newNode)
                index += 1
            End While
        End Sub

        Private Function AddNeighborsToNode(argumentNode As Node, layerNameToSelectFrom As String) As Node
            Dim entityList As List(Of Entity) = Me.GetEntitiesAtNodePoint(argumentNode, layerNameToSelectFrom)
            Dim index As Integer = 0
            While index < entityList.Count - 1
                Dim entity As Entity = entityList(index)
                Dim line As Line = CType(entity, Line)
                Dim startPoint As Point3d = line.StartPoint
                Dim endPoint As Point3d = line.EndPoint
                Dim startNode As Node = GetNodeByPoint(startPoint)
                Dim endNode As Node = GetNodeByPoint(endPoint)
                If startNode IsNot Nothing Then
                    AddNeighborIfNotExists(argumentNode, startNode)
                End If
                If endNode IsNot Nothing Then
                    AddNeighborIfNotExists(argumentNode, endNode)
                End If
                index += 1
            End While
            Return argumentNode
        End Function

        Private Sub AddNeighborIfNotExists(argumentNode As Node, neighbor As Node)
            Dim areNodesTheSame As Integer = argumentNode.NodeId.CompareTo(neighbor.NodeId)
            'update argumentNode with neighbor
            If Not argumentNode.NeighborList.Contains(neighbor) And Not argumentNode.NodeId.Equals(neighbor.NodeId) Then
                argumentNode.NeighborList.Add(neighbor)
            End If
            'update neighbor with argumentNode
            If Not neighbor.NeighborList.Contains(argumentNode) And Not argumentNode.NodeId.Equals(neighbor.NodeId) Then
                neighbor.NeighborList.Add(argumentNode)
            End If
        End Sub

        Private Function GetNodeByPoint(point As Point3d) As Node
            For Each node As Node In Me.Nodes
                If node.AutoCADPoint.Equals(point) Then
                    Return node
                End If
            Next
            Return Nothing
        End Function
        Private Function GetEntitiesAtNodePoint(node As Node, layerNameToSelectFrom As String) As List(Of Entity)
            Dim fuzz As Double = 0.001
            Dim point As Point3d = node.AutoCADPoint
            Dim pt1 As Point3d = New Point3d(point.X + fuzz, point.Y + fuzz, point.Z)
            Dim pt2 As Point3d = New Point3d(point.X - fuzz, point.Y - fuzz, point.Z)
            Dim filter As SelectionFilter = SelectionFilters.GetAllLineEntitiesOnLayer(layerNameToSelectFrom)
            Dim ss As SelectionSet = SelectionSets.CrossingWindow(Document, pt2, pt2, filter)
            Dim entityListAtNode As List(Of Entity) = SelectionSets.GetEntityList(Document, ss)
            Return entityListAtNode
        End Function

        Private Sub ReplaceNode(index As Integer, newNode As Node)
            Me.Nodes(index) = newNode
        End Sub

        Public Sub LabelNodes()
            For Each node As Node In Nodes
                Dim neighborIds As List(Of String) = node.GetNeighborIdsToStringList()
                Dim formattedNeighborString As String = String.Join(", ", neighborIds)


                Dim newLine As String = vbCrLf
                Dim label As String = String.Format("{0}ID:{1}{2}Neighbors:{3}", newLine, node.NodeId, newLine, formattedNeighborString)

                Dim settings As Mtexts.Settings = New Mtexts.Settings()
                settings.Height = 5.0
                settings.Content = label
                settings.Location = node.AutoCADPoint

                Dim mtext As MText = Mtexts.Add(settings)

            Next
        End Sub
    End Class
End Namespace

