Option Strict On
Option Infer Off
Option Explicit On

Imports NUnit.Framework
Imports System.Threading

Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Colors
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry

Namespace Tests
    <TestFixture>
    Partial Public Class RegressionTests

        '<SetUp>
        'Public Sub Init()
        '    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("(vla-startundomark (vla-get-ActiveDocument (vlax-get-acad-object)))" + vbLf, True, False, False)
        'End Sub

        '<TearDown>
        'Public Sub TearDown()
        '    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("(vla-endundomark (vla-get-ActiveDocument (vlax-get-acad-object)))" + vbLf, True, False, False)
        '    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("(command-s ""._undo"" ""back"" ""yes"")" + vbLf, True, False, False)
        'End Sub

        Private Shared tempLayer As String = "Temp"


        Public Function DrawLine(doc As Document, pt1 As Point3d, pt2 As Point3d, layerName As String) As Line
            Dim line As Line = Nothing
            Dim db As Database = doc.Database
            Using lock As DocumentLock = doc.LockDocument
                Using t As Transaction = db.TransactionManager.StartTransaction()
                    Dim ms As BlockTableRecord
                    ms = CType(t.GetObject(db.CurrentSpaceId,
                                          OpenMode.ForWrite), BlockTableRecord)
                    line = New Line(pt1, pt2)
                    line.SetDatabaseDefaults()
                    Dim id As ObjectId = ms.AppendEntity(line)
                    t.AddNewlyCreatedDBObject(line, True)
                    Dim newLine As Line = CType(t.GetObject(id, OpenMode.ForWrite), Line)
                    newLine.Layer = layerName

                    t.Commit()
                End Using
            End Using

            Return line
        End Function

        '<Test>
        'Public Sub Break_2_overlapping_lines_Should_return_2_new_lines()
        '    Dim doc As Document = Application.DocumentManager.MdiActiveDocument
        '    Dim layer As LayerTableRecord = Layers.CreateFirstAvailableLayerName(doc, tempLayer)
        '    Dim pt1 As Point3d = New Point3d(0, -2, 0)
        '    Dim pt2 As Point3d = New Point3d(0, 2, 0)
        '    Dim line1 As Line = DrawLine(doc, pt1, pt2)
        '    pt1 = New Point3d(-2, 0, 0)
        '    pt2 = New Point3d(2, 0, 0)
        '    Dim line2 As Line = DrawLine(doc, pt1, pt2)
        '    Dim lines As List(Of ObjectId) = New List(Of ObjectId)
        '    lines.Add(line1.Id)
        '    Dim lines2 As List(Of ObjectId) = New List(Of ObjectId)
        '    lines2.Add(line2.Id)
        '    Dim selection As SelectionSet = SelectionSet.FromObjectIds(lines.ToArray)
        '    Dim selection2 As SelectionSet = SelectionSet.FromObjectIds(lines2.ToArray)
        '    Dim inputs As New Inputs
        '    inputs.SelectionToBreak = selection
        '    inputs.SelectionToBreakWith = selection2
        '    Dim newLines As List(Of ObjectId) = BreakSsWithSs(doc, inputs)
        '    Assert.AreEqual(newLines.Count, 2, "Expected 2 new lines, instead was: " + newLines.Count.ToString)
        'End Sub

        <Test>
        Public Sub Break_2_overlapping_lines_with_self_Should_return_4_new_lines()
            Dim doc As Document = Application.DocumentManager.MdiActiveDocument
            Dim layer As LayerTableRecord = Layers.CreateFirstAvailableLayerName(doc, tempLayer)
            Dim pt1 As Point3d = New Point3d(0, -2, 0)
            Dim pt2 As Point3d = New Point3d(0, 2, 0)
            Dim line1 As Line = DrawLine(doc, pt1, pt2, layer.Name)
            pt1 = New Point3d(-2, 0, 0)
            pt2 = New Point3d(2, 0, 0)
            Dim line2 As Line = DrawLine(doc, pt1, pt2, layer.Name)
            Dim lines As List(Of ObjectId) = New List(Of ObjectId)
            lines.Add(line1.Id)
            lines.Add(line2.Id)
            Dim lines2 As List(Of ObjectId) = New List(Of ObjectId)
            lines2.Add(line1.Id)
            lines2.Add(line2.Id)
            Dim selection As SelectionSet = SelectionSet.FromObjectIds(lines.ToArray)
            Dim selection2 As SelectionSet = SelectionSet.FromObjectIds(lines2.ToArray)
            Dim inputs As New Workflows.BreakSs.BreakSsInputs
            inputs.SelectionToBreak = selection
            inputs.SelectionToBreakWith = selection2
            inputs.Self = True
            Dim newLines As List(Of ObjectId) = Workflows.BreakSs.BreakSsWithSs(doc, inputs)
            Assert.AreEqual(newLines.Count, 4, "Expected 4 new lines, instead was: " + newLines.Count.ToString)
        End Sub

        'Test make node graph
        <Test>
        Public Sub Make_Simple_4x4_Node_Graph()
            Dim doc As Document = Application.DocumentManager.MdiActiveDocument
            Dim layer As LayerTableRecord = Layers.CreateFirstAvailableLayerName(doc, tempLayer)
            Dim pt1 As Point3d = New Point3d(0, 0, 0)
            Dim pt2 As Point3d = New Point3d(2, 0, 0)
            Dim pt3 As Point3d = New Point3d(2, 2, 0)
            Dim pt4 As Point3d = New Point3d(0, 2, 0)
            Dim linePointTuples As New List(Of LinePoints)
            linePointTuples.Add(New LinePoints(pt1, pt2))
            linePointTuples.Add(New LinePoints(pt2, pt3))
            linePointTuples.Add(New LinePoints(pt3, pt4))
            linePointTuples.Add(New LinePoints(pt4, pt1))
            Dim linePoints As New List(Of Point3d) From {pt1, pt2, pt3, pt4}
            Dim lineIds As List(Of ObjectId) = DrawLines(doc, linePointTuples, tempLayer)
            Dim nodeGraph As New NodeGraph.NodeGraph(doc, linePoints, pt1, pt2)
            Assert.AreEqual(nodeGraph.Nodes.Count, 4, "Expected 4 nodes on graph, instead was: " + nodeGraph.Nodes.Count.ToString)
        End Sub

        Public Class LinePoints
            Public StartPoint As Point3d
            Public EndPoint As Point3d

            Public Sub New(pt1 As Point3d, pt2 As Point3d)
                StartPoint = pt1
                EndPoint = pt2
            End Sub
        End Class


        Public Function DrawLines(doc As Document, linePointTuples As List(Of LinePoints), layerName As String) As List(Of ObjectId)
            Dim lineIds As List(Of ObjectId) = New List(Of ObjectId)
            For Each points As LinePoints In linePointTuples
                Dim line As Line = DrawLine(doc, points.StartPoint, points.EndPoint, layerName)
                lineIds.Add(line.ObjectId)
            Next
            Return lineIds
        End Function

        'Test add neighbors to node graph
        <Test>
        Public Sub Add_Neighbors_To_Simple_4x4_Node_Graph()
            Dim doc As Document = Application.DocumentManager.MdiActiveDocument
            Dim layer As LayerTableRecord = Layers.CreateFirstAvailableLayerName(doc, tempLayer)
            Dim pt1 As Point3d = New Point3d(0, 0, 0)
            Dim pt2 As Point3d = New Point3d(2, 0, 0)
            Dim pt3 As Point3d = New Point3d(2, 2, 0)
            Dim pt4 As Point3d = New Point3d(0, 2, 0)
            Dim linePointTuples As New List(Of LinePoints)
            linePointTuples.Add(New LinePoints(pt1, pt2))
            linePointTuples.Add(New LinePoints(pt2, pt3))
            linePointTuples.Add(New LinePoints(pt3, pt4))
            linePointTuples.Add(New LinePoints(pt4, pt1))
            Dim linePoints As New List(Of Point3d) From {pt1, pt2, pt3, pt4}
            Dim lineIds As List(Of ObjectId) = DrawLines(doc, linePointTuples, layer.Name)
            Dim nodeGraph As New NodeGraph.NodeGraph(doc, linePoints, pt1, pt2)

            Dim layerNameToSelectFrom As String = layer.Name
            Zoom.Extents(doc)
            nodeGraph.AddNeighborsToNodes(layerNameToSelectFrom)
            nodeGraph.LabelNodes()
            Assert.AreEqual(nodeGraph.Nodes.Count, 4, "Expected 4 nodes on graph, instead was: " + nodeGraph.Nodes.Count.ToString)
        End Sub

        'Test connect start and end points to node graph

        'Test calculate ideal path

        'Test calculate ideal diverse path

    End Class
End Namespace
