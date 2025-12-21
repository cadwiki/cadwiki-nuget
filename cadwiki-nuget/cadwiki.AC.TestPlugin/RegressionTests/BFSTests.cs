using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using cadwiki.AC.Utilities;
using NUnit.Framework;

namespace cadwiki.AC.TestPlugin.Tests
{
    [TestFixture]
    public partial class RegressionTests
    {
        private static Document _doc;

        [OneTimeSetUp]
        public void Init()
        {
            _doubleComplexCount = 0;

            _doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var wcTest = "*test*";
            var filter = SelectionFilters.GetAllEntitiesOnWildCardLayer(wcTest);
            var ss = SelectionSets.SelectAll(_doc, filter);
            SelectionSets.DeleteAllEntities(_doc, ss);
            Layers.DeleteLayersFromDrawing(_doc, wcTest);

            var wcCadwiki = "*cadwiki*";
            filter = SelectionFilters.GetAllEntitiesOnWildCardLayer(wcCadwiki);
            ss = SelectionSets.SelectAll(_doc, filter);
            SelectionSets.DeleteAllEntities(_doc, ss);

            Layers.DeleteLayersFromDrawing(_doc, wcCadwiki);
        }

        
        [SetUp]
        public void SetUp()
        {
            var testName = TestContext.CurrentContext.Test.Name;
            currentTestLayerName = "Test-" + testName;
            currentTestLayerName = Layers.CreateFirstAvailableLayerName(_doc, currentTestLayerName).Name;
        }

        public void AcMarkUndo()
        {
            _doc.SendStringToExecute("(vla-startundomark (vla-get-ActiveDocument (vlax-get-acad-object)))" + Environment.NewLine, true, false, false);
        }

        public void AcUndo()
        {
            _doc.SendStringToExecute("(vla-endundomark (vla-get-ActiveDocument (vlax-get-acad-object)))" + Environment.NewLine, true, false, false);
            Autodesk.AutoCAD.ApplicationServices.Application
                .DocumentManager
                .MdiActiveDocument
                .SendStringToExecute(
                    "(command-s \"._undo\" \"back\" \"yes\")" + Environment.NewLine,
                    true,
                    false,
                    false
                );
        }

        private static string currentTestLayerName = "Test-";


        public Line DrawLine(Document _doc, Point3d pt1, Point3d pt2, string layerName)
        {
            Line line = null;
            var db = _doc.Database;
            using (var @lock = _doc.LockDocument())
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

        [Test]
        public void Break_2_overlapping_lines_with_self_Should_return_4_new_lines()
        {
            var pt1 = new Point3d(0d, -2, 0d);
            var pt2 = new Point3d(0d, 2d, 0d);
            var line1 = DrawLine(_doc, pt1, pt2, currentTestLayerName);
            pt1 = new Point3d(-2, 0d, 0d);
            pt2 = new Point3d(2d, 0d, 0d);
            var line2 = DrawLine(_doc, pt1, pt2, currentTestLayerName);
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
            var newLines = Workflows.BreakSs.BreakSsWithSs(_doc, inputs);
            Assert.AreEqual(newLines.Count, 4, "Expected 4 new lines, instead was: " + newLines.Count.ToString());
        }

        [Test]
        public void Make_Simple_4x4_Node_Graph()
        {
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
            
            var nodeGraph = new NodeGraph.NodeGraph(_doc, linePoints, pt1, pt2);
            var lineIds = DrawLines(_doc, linePointTuples, nodeGraph.LayerNameLines);
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


        public List<ObjectId> DrawLines(Document _doc, List<LinePoints> linePointTuples, string layerName)
        {
            var lineIds = new List<ObjectId>();
            foreach (LinePoints points in linePointTuples)
            {
                var line = DrawLine(_doc, points.StartPoint, points.EndPoint, layerName);
                lineIds.Add(line.ObjectId);
            }
            return lineIds;
        }

        [Test]
        public void Add_Neighbors_To_Simple_4x4_Node_Graph()
        {
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

            var nodeGraph = new NodeGraph.NodeGraph(_doc, linePoints, pt1, pt2);
            var lineIds = DrawLines(_doc, linePointTuples, nodeGraph.LayerNameLines);

            nodeGraph.AddNeighborsToNodes();
            nodeGraph.LabelNodes();
            Assert.AreEqual(nodeGraph.Nodes.Count, 4, "Expected 4 nodes on graph, instead was: " + nodeGraph.Nodes.Count.ToString());
        }

        [Test]
        public void Add_Neighbors_To_Complex_Node_Graph()
        {
            double xOffset = 10.0d;

            

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



            var nodeGraph = new NodeGraph.NodeGraph(_doc, linePoints, linePointTuples[0].StartPoint, linePointTuples[0].EndPoint);
            var lineIds = DrawLines(_doc, linePointTuples, nodeGraph.LayerNameLines);

            nodeGraph.AddNeighborsToNodes();
            nodeGraph.LabelNodes();
            Assert.AreEqual(nodeGraph.Nodes.Count, 4, "Expected 4 nodes on graph, instead was: " + nodeGraph.Nodes.Count.ToString());
        }


        private static int _doubleComplexCount = 0;

        public NodeGraph.NodeGraph DrawDoubleComplexNodeGraph(
            Document doc, 
            bool addNeighbors = false, 
            bool connectSrcAndDest = false, 
            bool bfsPath = false, 
            bool calcDiversePaths = false
            )
        {
            var xStart = (20.0d * _doubleComplexCount);
            var xOffset = 20.0d;

            

            var source = new Point3d(2d + xStart + xOffset, 6d, 0d);

            var pt1 = new Point3d(0d + xStart + xOffset, 0d, 0d);
            var pt2 = new Point3d(2d + xStart + xOffset, 0d, 0d);
            var pt3 = new Point3d(2d + xStart + xOffset, 2d, 0d);
            var pt4 = new Point3d(0d + xStart + xOffset, 2d, 0d);
            var linePointTuples = new List<LinePoints>();
            linePointTuples.Add(new LinePoints(pt1, pt2));
            linePointTuples.Add(new LinePoints(pt2, pt3));
            linePointTuples.Add(new LinePoints(pt3, pt4));
            linePointTuples.Add(new LinePoints(pt4, pt1));
            linePointTuples.Add(new LinePoints(pt1, pt3));
            var linePoints = new List<Point3d>() { pt1, pt2, pt3, pt4 };

            xOffset = 30.0d;

            var yStart = 20.0d;
            var yOffset = 20.0 ;

            pt1 = new Point3d(0d + xStart + xOffset, 0d + yStart + yOffset, 0d);
            linePointTuples.Add(new LinePoints(pt1, pt3));

            pt2 = new Point3d(2d + xStart + xOffset, 0d + yStart + yOffset, 0d);
            pt3 = new Point3d(2d + xStart + xOffset, 2d + yStart + yOffset, 0d);
            pt4 = new Point3d(0d + xStart + xOffset, 2d + yStart + yOffset, 0d);
            linePointTuples.Add(new LinePoints(pt1, pt2));
            linePointTuples.Add(new LinePoints(pt2, pt3));
            linePointTuples.Add(new LinePoints(pt3, pt4));
            linePointTuples.Add(new LinePoints(pt4, pt1));
            linePointTuples.Add(new LinePoints(pt1, pt3));

            linePoints.Add(pt1);
            linePoints.Add(pt2);
            linePoints.Add(pt3);
            linePoints.Add(pt4);

            var nodeGraph = new NodeGraph.NodeGraph(doc, linePoints);
            var lineIds = DrawLines(doc, linePointTuples, nodeGraph.LayerNameLines);

            if (addNeighbors)
            {
                nodeGraph.AddNeighborsToNodes();
            }

            var dest = new Point3d(5d + xStart + xOffset, 5d + yStart + yOffset, 0d);
            if (connectSrcAndDest)
            {
                nodeGraph.ModifyWithSourceAndDest(doc, source, dest);
            }

            nodeGraph.LabelNodes();

            if (bfsPath)
            {
                
                var list = nodeGraph.BFS(nodeGraph.SourceNodeId, nodeGraph.DestNodeId);
                nodeGraph.DrawLinesAlongPath(doc, list, nodeGraph.LayerNameBFSPath);
            }

            _doubleComplexCount = _doubleComplexCount + 1;
            return nodeGraph;
        }


        [Test]
        public void Add_Neighbors_To_Double_Complex_Node_Graph()
        {
            var nodeGraph = DrawDoubleComplexNodeGraph(_doc, true);
            Assert.AreEqual(nodeGraph.Nodes.Count, 8, "Expected 8 nodes on graph, instead was: " + nodeGraph.Nodes.Count.ToString());
        }

        [Test]
        public void Add_Src_And_Dest_To_Double_Complex_Node_Graph()
        {
            var nodeGraph = DrawDoubleComplexNodeGraph(_doc, true, true);
            Assert.AreEqual(nodeGraph.Nodes.Count, 11, "Expected 11 nodes on graph, instead was: " + nodeGraph.Nodes.Count.ToString());
        }

        [Test]
        public void Add_Bfs_Path_To_Double_Complex_Node_Graph()
        {
            var nodeGraph = DrawDoubleComplexNodeGraph(_doc, true, true, true);
            Assert.AreEqual(nodeGraph.BFSPath.Count, 5, "Expected 5 nodes on BFS Path, instead was: " + nodeGraph.BFSPath.Count.ToString());
        }

        // Test larger graph of nodes
        // Test calculate ideal diverse paths

    }
}