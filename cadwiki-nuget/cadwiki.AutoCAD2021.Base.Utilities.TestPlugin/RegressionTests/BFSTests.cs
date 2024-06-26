using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using NUnit.Framework;

namespace cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.Tests
{
    [TestFixture]
    public partial class RegressionTests
    {

        // <SetUp>
        // Public Sub Init()
        // Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("(vla-startundomark (vla-get-ActiveDocument (vlax-get-acad-object)))" + vbLf, True, False, False)
        // End Sub

        // <TearDown>
        // Public Sub TearDown()
        // Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("(vla-endundomark (vla-get-ActiveDocument (vlax-get-acad-object)))" + vbLf, True, False, False)
        // Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("(command-s ""._undo"" ""back"" ""yes"")" + vbLf, True, False, False)
        // End Sub

        private static string tempLayer = "Temp";


        public Line DrawLine(Document doc, Point3d pt1, Point3d pt2, string layerName)
        {
            Line line = null;
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord ms;
                    ms = (BlockTableRecord)t.GetObject(db.CurrentSpaceId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    line = new Line(pt1, pt2);
                    line.SetDatabaseDefaults();
                    var id = ms.AppendEntity(line);
                    t.AddNewlyCreatedDBObject(line, true);
                    Line newLine = (Line)t.GetObject(id, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    newLine.Layer = layerName;

                    t.Commit();
                }
            }

            return line;
        }

        // <Test>
        // Public Sub Break_2_overlapping_lines_Should_return_2_new_lines()
        // Dim doc As Document = Application.DocumentManager.MdiActiveDocument
        // Dim layer As LayerTableRecord = Layers.CreateFirstAvailableLayerName(doc, tempLayer)
        // Dim pt1 As Point3d = New Point3d(0, -2, 0)
        // Dim pt2 As Point3d = New Point3d(0, 2, 0)
        // Dim line1 As Line = DrawLine(doc, pt1, pt2)
        // pt1 = New Point3d(-2, 0, 0)
        // pt2 = New Point3d(2, 0, 0)
        // Dim line2 As Line = DrawLine(doc, pt1, pt2)
        // Dim lines As List(Of ObjectId) = New List(Of ObjectId)
        // lines.Add(line1.Id)
        // Dim lines2 As List(Of ObjectId) = New List(Of ObjectId)
        // lines2.Add(line2.Id)
        // Dim selection As SelectionSet = SelectionSet.FromObjectIds(lines.ToArray)
        // Dim selection2 As SelectionSet = SelectionSet.FromObjectIds(lines2.ToArray)
        // Dim inputs As New Inputs
        // inputs.SelectionToBreak = selection
        // inputs.SelectionToBreakWith = selection2
        // Dim newLines As List(Of ObjectId) = BreakSsWithSs(doc, inputs)
        // Assert.AreEqual(newLines.Count, 2, "Expected 2 new lines, instead was: " + newLines.Count.ToString)
        // End Sub

        [Test]
        public void Break_2_overlapping_lines_with_self_Should_return_4_new_lines()
        {
            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var layer = Layers.CreateFirstAvailableLayerName(doc, tempLayer);
            var pt1 = new Point3d(0d, -2, 0d);
            var pt2 = new Point3d(0d, 2d, 0d);
            var line1 = DrawLine(doc, pt1, pt2, layer.Name);
            pt1 = new Point3d(-2, 0d, 0d);
            pt2 = new Point3d(2d, 0d, 0d);
            var line2 = DrawLine(doc, pt1, pt2, layer.Name);
            var lines = new List<ObjectId>();
            lines.Add(line1.Id);
            lines.Add(line2.Id);
            var lines2 = new List<ObjectId>();
            lines2.Add(line1.Id);
            lines2.Add(line2.Id);
            var selection = SelectionSet.FromObjectIds(lines.ToArray());
            var selection2 = SelectionSet.FromObjectIds(lines2.ToArray());
            var inputs = new Workflows.BreakSs.BreakSsInputs();
            inputs.SelectionToBreak = selection;
            inputs.SelectionToBreakWith = selection2;
            inputs.Self = true;
            var newLines = Workflows.BreakSs.BreakSsWithSs(doc, inputs);
            Assert.AreEqual(newLines.Count, 4, "Expected 4 new lines, instead was: " + newLines.Count.ToString());
        }

        // Test make node graph
        [Test]
        public void Make_Simple_4x4_Node_Graph()
        {
            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var layer = Layers.CreateFirstAvailableLayerName(doc, tempLayer);
            var pt1 = new Point3d(0d, 0d, 0d);
            var pt2 = new Point3d(2d, 0d, 0d);
            var pt3 = new Point3d(2d, 2d, 0d);
            var pt4 = new Point3d(0d, 2d, 0d);
            var linePointTuples = new List<LinePoints>();
            linePointTuples.Add(new LinePoints(pt1, pt2));
            linePointTuples.Add(new LinePoints(pt2, pt3));
            linePointTuples.Add(new LinePoints(pt3, pt4));
            linePointTuples.Add(new LinePoints(pt4, pt1));
            var linePoints = new List<Point3d>() { pt1, pt2, pt3, pt4 };
            var lineIds = DrawLines(doc, linePointTuples, tempLayer);
            var nodeGraph = new NodeGraph.NodeGraph(doc, linePoints, pt1, pt2, layer.Name);
            Assert.AreEqual(nodeGraph.Nodes.Count, 4, "Expected 4 nodes on graph, instead was: " + nodeGraph.Nodes.Count.ToString());
        }

        public class LinePoints
        {
            public Point3d StartPoint;
            public Point3d EndPoint;

            public LinePoints(Point3d pt1, Point3d pt2)
            {
                StartPoint = pt1;
                EndPoint = pt2;
            }
        }


        public List<ObjectId> DrawLines(Document doc, List<LinePoints> linePointTuples, string layerName)
        {
            var lineIds = new List<ObjectId>();
            foreach (LinePoints points in linePointTuples)
            {
                var line = DrawLine(doc, points.StartPoint, points.EndPoint, layerName);
                lineIds.Add(line.ObjectId);
            }
            return lineIds;
        }

        // Test add neighbors to node graph
        [Test]
        public void Add_Neighbors_To_Simple_4x4_Node_Graph()
        {
            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var layer = Layers.CreateFirstAvailableLayerName(doc, tempLayer);
            var pt1 = new Point3d(0d, 0d, 0d);
            var pt2 = new Point3d(2d, 0d, 0d);
            var pt3 = new Point3d(2d, 2d, 0d);
            var pt4 = new Point3d(0d, 2d, 0d);
            var linePointTuples = new List<LinePoints>();
            linePointTuples.Add(new LinePoints(pt1, pt2));
            linePointTuples.Add(new LinePoints(pt2, pt3));
            linePointTuples.Add(new LinePoints(pt3, pt4));
            linePointTuples.Add(new LinePoints(pt4, pt1));
            var linePoints = new List<Point3d>() { pt1, pt2, pt3, pt4 };
            var lineIds = DrawLines(doc, linePointTuples, layer.Name);
            var nodeGraph = new NodeGraph.NodeGraph(doc, linePoints, pt1, pt2, layer.Name);

            string layerNameToSelectFrom = layer.Name;
            Zoom.Extents(doc);
            nodeGraph.AddNeighborsToNodes(layerNameToSelectFrom);
            nodeGraph.LabelNodes();
            Assert.AreEqual(nodeGraph.Nodes.Count, 4, "Expected 4 nodes on graph, instead was: " + nodeGraph.Nodes.Count.ToString());
        }

        [Test]
        public void Add_Neighbors_To_Complex_Node_Graph()
        {
            double xOffset = 10.0d;

            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var layer = Layers.CreateFirstAvailableLayerName(doc, tempLayer);

            var pt1 = new Point3d(0d + xOffset, 0d, 0d);
            var pt2 = new Point3d(2d + xOffset, 0d, 0d);
            var pt3 = new Point3d(2d + xOffset, 2d, 0d);
            var pt4 = new Point3d(0d + xOffset, 2d, 0d);
            var linePointTuples = new List<LinePoints>();
            linePointTuples.Add(new LinePoints(pt1, pt2));
            linePointTuples.Add(new LinePoints(pt2, pt3));
            linePointTuples.Add(new LinePoints(pt3, pt4));
            linePointTuples.Add(new LinePoints(pt4, pt1));
            linePointTuples.Add(new LinePoints(pt1, pt3));

            var linePoints = new List<Point3d>() { pt1, pt2, pt3, pt4 };



            var lineIds = DrawLines(doc, linePointTuples, layer.Name);
            var nodeGraph = new NodeGraph.NodeGraph(doc, linePoints, linePointTuples[0].StartPoint, linePointTuples[0].EndPoint, layer.Name);

            string layerNameToSelectFrom = layer.Name;
            Zoom.Extents(doc);
            nodeGraph.AddNeighborsToNodes(layerNameToSelectFrom);
            nodeGraph.LabelNodes();
            Assert.AreEqual(nodeGraph.Nodes.Count, 4, "Expected 4 nodes on graph, instead was: " + nodeGraph.Nodes.Count.ToString());
        }

        [Test]
        public void Add_Neighbors_To_Double_Complex_Node_Graph()
        {
            double xOffset = 20.0d;

            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var layer = Layers.CreateFirstAvailableLayerName(doc, tempLayer);
            var source = new Point3d(2d + xOffset, 6d, 0d);

            var pt1 = new Point3d(0d + xOffset, 0d, 0d);
            var pt2 = new Point3d(2d + xOffset, 0d, 0d);
            var pt3 = new Point3d(2d + xOffset, 2d, 0d);
            var pt4 = new Point3d(0d + xOffset, 2d, 0d);
            var linePointTuples = new List<LinePoints>();
            linePointTuples.Add(new LinePoints(pt1, pt2));
            linePointTuples.Add(new LinePoints(pt2, pt3));
            linePointTuples.Add(new LinePoints(pt3, pt4));
            linePointTuples.Add(new LinePoints(pt4, pt1));
            linePointTuples.Add(new LinePoints(pt1, pt3));
            var linePoints = new List<Point3d>() { pt1, pt2, pt3, pt4 };

            xOffset = 30.0d;
            double yOffset = 20.0d;
            pt1 = new Point3d(0d + xOffset, 0d + yOffset, 0d);
            linePointTuples.Add(new LinePoints(pt1, pt3));

            pt2 = new Point3d(2d + xOffset, 0d + yOffset, 0d);
            pt3 = new Point3d(2d + xOffset, 2d + yOffset, 0d);
            pt4 = new Point3d(0d + xOffset, 2d + yOffset, 0d);
            linePointTuples.Add(new LinePoints(pt1, pt2));
            linePointTuples.Add(new LinePoints(pt2, pt3));
            linePointTuples.Add(new LinePoints(pt3, pt4));
            linePointTuples.Add(new LinePoints(pt4, pt1));
            linePointTuples.Add(new LinePoints(pt1, pt3));

            linePoints.Add(pt1);
            linePoints.Add(pt2);
            linePoints.Add(pt3);
            linePoints.Add(pt4);

            var lineIds = DrawLines(doc, linePointTuples, layer.Name);
            var nodeGraph = new NodeGraph.NodeGraph(doc, linePoints, layer.Name);


            var dest = new Point3d(5d + xOffset, 5d + yOffset, 0d);
            nodeGraph.ModifyWithSourceAndDest(doc, layer.Name, source, dest);

            nodeGraph.LabelNodes();

            var list = nodeGraph.BFS(nodeGraph.SourceNodeId, nodeGraph.DestNodeId);

            var pathLayer = Layers.CreateFirstAvailableLayerName(doc, tempLayer);
            nodeGraph.DrawLinesAlongPath(doc, list, pathLayer.Name);

            Zoom.Extents(doc);

            Assert.AreEqual(nodeGraph.Nodes.Count, 8, "Expected 8 nodes on graph, instead was: " + nodeGraph.Nodes.Count.ToString());
        }

        // Test connect start and end points to node graph

        // Test calculate ideal path

        // Test calculate ideal diverse path

    }
}