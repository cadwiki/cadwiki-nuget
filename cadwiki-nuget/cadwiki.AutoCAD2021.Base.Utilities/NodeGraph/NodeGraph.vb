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
        Public SourceNodeId As Integer = -1
        Public DestNodeId As Integer = -2
        Public Source As Point3d = Nothing
        Public Dest As Point3d
        Public PointList As New List(Of Point3d)
        Public LayerName As String

        Public Sub New(document As Document, pointList As List(Of Point3d), destination As Point3d, source As Point3d, layerName As String)
            Me.Nodes = Nodes
            Me.Edges = Edges
            Me.Document = document
            Me.Source = source
            Me.Dest = destination
            Me.PointList = pointList
            Me.LayerName = layerName
            BuildGraph(pointList)
        End Sub

        Public Sub New(document As Document, pointList As List(Of Point3d), layerName As String)
            Me.Nodes = Nodes
            Me.Edges = Edges
            Me.Document = document
            Me.PointList = pointList
            Me.LayerName = layerName
            BuildGraph(pointList)
        End Sub

        Public Sub ModifyWithSourceAndDest(doc As Document, layerName As String, destination As Point3d, source As Point3d)
            Me.Nodes = New List(Of Node)
            Me.Edges = New List(Of Edge)

            Me.Source = source
            Me.Dest = destination
            PointList.Add(source)
            PointList.Add(destination)

            Dim filter As SelectionFilter = SelectionFilters.GetAllLineBasedEntitiesOnLayer(layerName)
            Dim graphSS As SelectionSet = SelectionSets.SelectAll(doc, filter)

            Dim closestPointOnGraphToSource As Point3d = SelectionSets.GetClosestPointOnAnyLineFromSelectionToAGivenPoint(doc, graphSS, source)
            Dim line As Line = Draw.DrawLineByPoints(doc, source, closestPointOnGraphToSource, layerName)

            Dim closestPointOnGraphToDest As Point3d = SelectionSets.GetClosestPointOnAnyLineFromSelectionToAGivenPoint(doc, graphSS, destination)
            Dim line2 As Line = Draw.DrawLineByPoints(doc, destination, closestPointOnGraphToDest, layerName)

            Dim modifiedGraph As SelectionSet = SelectionSets.SelectAll(doc, filter)
            Dim inputs As New Workflows.BreakSs.BreakSsInputs
            inputs.SelectionToBreak = modifiedGraph
            inputs.SelectionToBreakWith = modifiedGraph
            inputs.Self = True
            inputs.NewLayer = layerName
            inputs.DeleteOriginal = True
            Dim newLines As List(Of ObjectId) = Workflows.BreakSs.BreakSsWithSs(doc, inputs)

            If (Not PointList.Contains(closestPointOnGraphToSource)) Then
                PointList.Add(closestPointOnGraphToSource)
            End If

            If (Not PointList.Contains(closestPointOnGraphToDest)) Then
                PointList.Add(closestPointOnGraphToDest)
            End If


            BuildGraph(PointList)
        End Sub

        Private Sub BuildGraph(pointList As List(Of Point3d))
            Dim counter As Integer = 0
            Dim nodeId As Integer = counter
            For Each point As Point3d In pointList
                Dim parentNode As Node = Nothing
                Dim entitiesListAtNode As New List(Of Entity)
                Dim neighborList As New List(Of Node)
                Dim nodeCoordinates As Point3d = Nothing
                Dim hasNodeBeenDiscovered As Boolean = False

                nodeId = counter

                If Not Me.Source.Equals(Nothing) Then
                    Dim distanceFromSource As Double = Me.Source.DistanceTo(point)
                    If distanceFromSource = 0.0 Then
                        nodeId = SourceNodeId
                    End If
                End If

                Dim distanceFromDestination As Double = Double.MaxValue
                If Not Me.Dest.Equals(Nothing) Then
                    distanceFromDestination = Me.Dest.DistanceTo(point)
                    If distanceFromDestination = 0.0 Then
                        nodeId = DestNodeId
                    End If
                End If

                Dim node As New Node(nodeId, parentNode, entitiesListAtNode, neighborList, nodeCoordinates, hasNodeBeenDiscovered, point, distanceFromDestination)
                Nodes.Add(node)
                counter += 1
            Next
            AddNeighborsToNodes(LayerName)
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
            While index < entityList.Count
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
            Dim newLine As String = vbCrLf
            For Each node As Node In Nodes
                Dim neighborIds As List(Of String) = node.GetNeighborIdsToStringList()
                Dim formattedNeighborString As String = String.Join(", ", neighborIds)

                Dim label As String = String.Format("{0}ID:{1}{2}Nbors:{3}", newLine, node.NodeId, newLine, formattedNeighborString)

                Dim settings As Mtexts.Settings = New Mtexts.Settings()
                settings.Height = 2.5
                settings.Content = label
                settings.Location = node.AutoCADPoint
                settings.Location = New Point3d(settings.Location.X + 0.1, settings.Location.Y, settings.Location.Z)

                Dim mtext As MText = Mtexts.Add(settings)

                Draw.DrawCircleAtLocation(node.AutoCADPoint, 0.1)
            Next
        End Sub


        Public Function FindNodeById(nodeId As Integer) As Node
            Return Nodes.Find(Function(n) n.NodeId = nodeId)
        End Function

        Public Function BFS(startNodeId As Integer, destinationNodeId As Integer) As List(Of Node)
            Dim visitedNodes As New HashSet(Of Node)()
            Dim queue As New Queue(Of Node)()

            Dim startNode As Node = FindNodeById(startNodeId)
            Dim destinationNode As Node = FindNodeById(destinationNodeId)

            queue.Enqueue(startNode)
            visitedNodes.Add(startNode)

            While queue.Count > 0
                Dim currentNode As Node = queue.Dequeue()

                ' Check if the destination node has been reached
                If currentNode.NodeId = destinationNodeId Then
                    ' Build and return the path from start to destination
                    Return BuildPath(currentNode)
                End If

                ' Explore neighbors of the current node
                For Each neighborNode As Node In currentNode.NeighborList
                    If Not visitedNodes.Contains(neighborNode) Then
                        queue.Enqueue(neighborNode)
                        visitedNodes.Add(neighborNode)
                        neighborNode.ParentNode = currentNode ' Set the parent node for backtracking the path
                    End If
                Next
            End While

            ' No path found
            Return Nothing
        End Function

        Private Function BuildPath(destinationNode As Node) As List(Of Node)
            Dim path As New List(Of Node)()
            Dim currentNode As Node = destinationNode

            ' Backtrack from destination to start using parent pointers
            While currentNode IsNot Nothing
                path.Insert(0, currentNode) ' Insert at the beginning to maintain the correct order
                currentNode = currentNode.ParentNode
            End While

            Return path
        End Function


        Public Sub DrawLinesAlongPath(doc As Document, path As List(Of Node), layerName As String)
            If path IsNot Nothing Then
                Try
                    Dim startPoint As Point3d = path(0).AutoCADPoint
                    For i As Integer = 1 To path.Count - 1
                        Dim endPoint As Point3d = path(i).AutoCADPoint
                        Draw.DrawLineByPoints(doc, startPoint, endPoint, layerName)
                        startPoint = endPoint
                    Next
                Catch ex As Exception
                End Try
            Else

            End If
        End Sub

    End Class
End Namespace

