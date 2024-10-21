using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Microsoft.VisualBasic;
using cadwiki.AC.Utilities;

namespace cadwiki.AC.NodeGraph
{
    public class NodeGraph
    {
        public List<Node> Nodes = new List<Node>();
        public List<Edge> Edges = new List<Edge>();
        public Document Document;
        public int SourceNodeId = -1;
        public int DestNodeId = -2;
        public Point3d Source = default;
        public Point3d Dest;
        public List<Point3d> PointList = new List<Point3d>();
        public string LayerName;

        public NodeGraph(Document document, List<Point3d> pointList, Point3d destination, Point3d source, string layerName)
        {
            Nodes = Nodes;
            Edges = Edges;
            Document = document;
            Source = source;
            Dest = destination;
            PointList = pointList;
            LayerName = layerName;
            BuildGraph(pointList);
        }

        public NodeGraph(Document document, List<Point3d> pointList, string layerName)
        {
            Nodes = Nodes;
            Edges = Edges;
            Document = document;
            PointList = pointList;
            LayerName = layerName;
            BuildGraph(pointList);
        }

        public void ModifyWithSourceAndDest(Document doc, string layerName, Point3d destination, Point3d source)
        {
            Nodes = new List<Node>();
            Edges = new List<Edge>();

            Source = source;
            Dest = destination;
            PointList.Add(source);
            PointList.Add(destination);

            var filter = SelectionFilters.GetAllLineBasedEntitiesOnLayer(layerName);
            var graphSS = SelectionSets.SelectAll(doc, filter);

            var closestPointOnGraphToSource = SelectionSets.GetClosestPointOnAnyLineFromSelectionToAGivenPoint(doc, graphSS, source);
            var line = Draw.DrawLineByPoints(doc, source, closestPointOnGraphToSource, layerName);

            var closestPointOnGraphToDest = SelectionSets.GetClosestPointOnAnyLineFromSelectionToAGivenPoint(doc, graphSS, destination);
            var line2 = Draw.DrawLineByPoints(doc, destination, closestPointOnGraphToDest, layerName);

            var modifiedGraph = SelectionSets.SelectAll(doc, filter);
            var inputs = new Workflows.BreakSs.BreakSsInputs();
            inputs.SelectionToBreak = modifiedGraph;
            inputs.SelectionToBreakWith = modifiedGraph;
            inputs.Self = true;
            inputs.NewLayer = layerName;
            inputs.DeleteOriginal = true;
            var newLines = Workflows.BreakSs.BreakSsWithSs(doc, inputs);

            if (!PointList.Contains(closestPointOnGraphToSource))
            {
                PointList.Add(closestPointOnGraphToSource);
            }

            if (!PointList.Contains(closestPointOnGraphToDest))
            {
                PointList.Add(closestPointOnGraphToDest);
            }


            BuildGraph(PointList);
        }

        private void BuildGraph(List<Point3d> pointList)
        {
            int counter = 0;
            int nodeId = counter;
            foreach (Point3d point in pointList)
            {
                Node parentNode = null;
                var entitiesListAtNode = new List<Entity>();
                var neighborList = new List<Node>();
                Point3d nodeCoordinates = default;
                bool hasNodeBeenDiscovered = false;

                nodeId = counter;

                if (!Source.Equals(null))
                {
                    double distanceFromSource = Source.DistanceTo(point);
                    if (distanceFromSource == 0.0d)
                    {
                        nodeId = SourceNodeId;
                    }
                }

                double distanceFromDestination = double.MaxValue;
                if (!Dest.Equals(null))
                {
                    distanceFromDestination = Dest.DistanceTo(point);
                    if (distanceFromDestination == 0.0d)
                    {
                        nodeId = DestNodeId;
                    }
                }

                var node = new Node(nodeId, parentNode, entitiesListAtNode, neighborList, nodeCoordinates, hasNodeBeenDiscovered, point, distanceFromDestination);
                Nodes.Add(node);
                counter += 1;
            }
            AddNeighborsToNodes(LayerName);
        }

        public void AddNeighborsToNodes(string layerNameToSelectFrom)
        {
            int index = 0;
            while (index < Nodes.Count - 1)
            {
                var node = Nodes[index];
                var newNode = AddNeighborsToNode(node, layerNameToSelectFrom);
                ReplaceNode(index, newNode);
                index += 1;
            }
        }

        private Node AddNeighborsToNode(Node argumentNode, string layerNameToSelectFrom)
        {
            var entityList = GetEntitiesAtNodePoint(argumentNode, layerNameToSelectFrom);
            int index = 0;
            while (index < entityList.Count)
            {
                var entity = entityList[index];
                Line line = (Line)entity;
                var startPoint = line.StartPoint;
                var endPoint = line.EndPoint;
                var startNode = GetNodeByPoint(startPoint);
                var endNode = GetNodeByPoint(endPoint);
                if (startNode is not null)
                {
                    AddNeighborIfNotExists(argumentNode, startNode);
                }
                if (endNode is not null)
                {
                    AddNeighborIfNotExists(argumentNode, endNode);
                }
                index += 1;
            }
            return argumentNode;
        }

        private void AddNeighborIfNotExists(Node argumentNode, Node neighbor)
        {
            int areNodesTheSame = argumentNode.NodeId.CompareTo(neighbor.NodeId);
            // update argumentNode with neighbor
            if (!argumentNode.NeighborList.Contains(neighbor) & !argumentNode.NodeId.Equals(neighbor.NodeId))
            {
                argumentNode.NeighborList.Add(neighbor);
            }
            // update neighbor with argumentNode
            if (!neighbor.NeighborList.Contains(argumentNode) & !argumentNode.NodeId.Equals(neighbor.NodeId))
            {
                neighbor.NeighborList.Add(argumentNode);
            }
        }

        private Node GetNodeByPoint(Point3d point)
        {
            foreach (Node node in Nodes)
            {
                if (node.AutoCADPoint.Equals(point))
                {
                    return node;
                }
            }
            return null;
        }
        private List<Entity> GetEntitiesAtNodePoint(Node node, string layerNameToSelectFrom)
        {
            double fuzz = 0.001d;
            var point = node.AutoCADPoint;
            var pt1 = new Point3d(point.X + fuzz, point.Y + fuzz, point.Z);
            var pt2 = new Point3d(point.X - fuzz, point.Y - fuzz, point.Z);
            var filter = SelectionFilters.GetAllLineEntitiesOnLayer(layerNameToSelectFrom);
            var ss = SelectionSets.CrossingWindow(Document, pt2, pt2, filter);
            var entityListAtNode = SelectionSets.GetEntityList(Document, ss);
            return entityListAtNode;
        }

        private void ReplaceNode(int index, Node newNode)
        {
            Nodes[index] = newNode;
        }

        public void LabelNodes()
        {
            string newLine = Constants.vbCrLf;
            foreach (Node node in Nodes)
            {
                var neighborIds = node.GetNeighborIdsToStringList();
                string formattedNeighborString = string.Join(", ", neighborIds);

                string label = string.Format("{0}ID:{1}{2}Nbors:{3}", newLine, node.NodeId, newLine, formattedNeighborString);

                var settings = new Mtexts.Settings();
                settings.Height = 2.5d;
                settings.Content = label;
                settings.Location = node.AutoCADPoint;
                settings.Location = new Point3d(settings.Location.X + 0.1d, settings.Location.Y, settings.Location.Z);

                var mtext = Mtexts.Add(settings);

                Draw.DrawCircleAtLocation(node.AutoCADPoint, 0.1d);
            }
        }


        public Node FindNodeById(int nodeId)
        {
            return Nodes.Find(n => n.NodeId == nodeId);
        }

        public List<Node> BFS(int startNodeId, int destinationNodeId)
        {
            var visitedNodes = new HashSet<Node>();
            var queue = new Queue<Node>();

            var startNode = FindNodeById(startNodeId);
            var destinationNode = FindNodeById(destinationNodeId);

            queue.Enqueue(startNode);
            visitedNodes.Add(startNode);

            while (queue.Count > 0)
            {
                var currentNode = queue.Dequeue();

                // Check if the destination node has been reached
                if (currentNode.NodeId == destinationNodeId)
                {
                    // Build and return the path from start to destination
                    return BuildPath(currentNode);
                }

                // Explore neighbors of the current node
                foreach (Node neighborNode in currentNode.NeighborList)
                {
                    if (!visitedNodes.Contains(neighborNode))
                    {
                        queue.Enqueue(neighborNode);
                        visitedNodes.Add(neighborNode);
                        neighborNode.ParentNode = currentNode; // Set the parent node for backtracking the path
                    }
                }
            }

            // No path found
            return null;
        }

        private List<Node> BuildPath(Node destinationNode)
        {
            var path = new List<Node>();
            var currentNode = destinationNode;

            // Backtrack from destination to start using parent pointers
            while (currentNode is not null)
            {
                path.Insert(0, currentNode); // Insert at the beginning to maintain the correct order
                currentNode = currentNode.ParentNode;
            }

            return path;
        }


        public void DrawLinesAlongPath(Document doc, List<Node> path, string layerName)
        {
            if (path is not null)
            {
                try
                {
                    var startPoint = path[0].AutoCADPoint;
                    for (int i = 1, loopTo = path.Count - 1; i <= loopTo; i++)
                    {
                        var endPoint = path[i].AutoCADPoint;
                        Draw.DrawLineByPoints(doc, startPoint, endPoint, layerName);
                        startPoint = endPoint;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            else
            {

            }
        }

    }
}