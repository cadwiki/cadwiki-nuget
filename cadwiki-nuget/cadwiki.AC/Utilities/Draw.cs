using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace cadwiki.AC.Utilities
{

    public class Draw
    {
        public static ObjectId Rectangle(Document doc, string layerName, Point2d pt1, Point2d pt2)
        {
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    double x1 = pt1.X;
                    double y1 = pt1.Y;
                    double x2 = pt2.X;
                    double y2 = pt2.Y;
                    var pline = new Polyline();
                    pline.AddVertexAt(0, new Point2d(x1, y1), 0.0d, 0.0d, 0.0d);
                    pline.AddVertexAt(0, new Point2d(x2, y1), 0.0d, 0.0d, 0.0d);
                    pline.AddVertexAt(0, new Point2d(x2, y2), 0.0d, 0.0d, 0.0d);
                    pline.AddVertexAt(0, new Point2d(x1, y2), 0.0d, 0.0d, 0.0d);
                    pline.Layer = layerName;
                    pline.Closed = true;
                    pline.TransformBy(doc.Editor.CurrentUserCoordinateSystem);
                    BlockTableRecord curSpace = (BlockTableRecord)t.GetObject(db.CurrentSpaceId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    var objectId = curSpace.AppendEntity(pline);
                    t.AddNewlyCreatedDBObject(pline, true);
                    t.Commit();
                    return objectId;
                }
            }

            return default;
        }

        public static ObjectId LineInCurrentSpace(Document doc, Line line)
        {
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord curSpace = (BlockTableRecord)t.GetObject(db.CurrentSpaceId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    var objectId = curSpace.AppendEntity(line);
                    t.AddNewlyCreatedDBObject(line, true);
                    t.Commit();
                    return objectId;
                }
            }
            return default;
        }

        public static Line DrawLineByPoints(Document doc, Point3d pt1, Point3d pt2, string layerName)
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

        // https://forums.autodesk.com/t5/net/how-to-draw-line-with-angle-vb-net-2005/td-p/1776262?attachment-id=382
        public static Line LineByRadians(Document doc, Point3d startPoint, double radians, double length)
        {
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                var endpoint = Points.PolarPoint(startPoint, radians, length);
                startPoint = Points.TransformByUCS(startPoint, db);
                endpoint = Points.TransformByUCS(endpoint, db);
                using (var t = db.TransactionManager.StartTransaction())
                {
                    try
                    {
                        BlockTableRecord curSpace = (BlockTableRecord)t.GetObject(db.CurrentSpaceId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        var line = new Line(startPoint, endpoint);
                        var id = curSpace.AppendEntity(line);
                        db.TransactionManager.AddNewlyCreatedDBObject(line, true);
                        t.Commit();
                        return line;
                    }
                    catch (Exception ex)
                    {
                        t.Abort();
                    }
                }
            }
            return null;
        }

        public static void DrawCircleAtLocation(Point3d center, double radius)
        {
            // Get the current document and database
            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            // Start a transaction
            using (var trans = db.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);

                // Open the Model Space block table record for write
                BlockTableRecord ms = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);

                // Create a new Circle entity
                var circle = new Circle(center, Vector3d.ZAxis, radius);

                // Add the Circle entity to the Model Space block table record
                ms.AppendEntity(circle);
                trans.AddNewlyCreatedDBObject(circle, true);

                // Commit the transaction
                trans.Commit();
            }
        }
    }
}