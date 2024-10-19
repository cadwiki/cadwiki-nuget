using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;

namespace cadwiki.AutoCAD2021.Base.Utilities.Shared
{
    public class RotatedBlockReferenceBoundingBoxes
    {
        public static List<Point3d[]> GetBtrTransformedBoundingBoxes(Document doc, SelectionSet ss, string substr)
        {
            Point3d minPoint = new Point3d(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3d maxPoint = new Point3d(double.MinValue, double.MinValue, double.MinValue);
            Extents3d boundingBox = new Extents3d();
            bool foundBoundingBox = false;
            var bbs = new List<Point3d[]>();
            using (var lk = doc.LockDocument())
            {
                using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                {
                    foreach (SelectedObject selectedObject in ss)
                    {
                        if (selectedObject != null)
                        {
                            Entity entity = tr.GetObject(selectedObject.ObjectId, OpenMode.ForWrite) as Entity;
                            if (entity != null)
                            {
                                Extents3d? extents = entity.Bounds;
                                if (extents.HasValue)
                                {
                                    var bref = (BlockReference)entity;
                                    var btrAccumulatedPoints = GetBoundingBoxOfBtrEntitiesNotOnLayer((BlockReference)bref, substr);
                                    Extents3d? ext2 = new Extents3d(btrAccumulatedPoints[0], btrAccumulatedPoints[1]);
                                    if (ext2.HasValue)
                                    {
                                        // Apply the block reference's transform to the extents
                                        Matrix3d blockTransform = bref.BlockTransform;

                                        // Transform the corners of the bounding box to match the rotation and scaling
                                        Point3d minPt = btrAccumulatedPoints[0].TransformBy(blockTransform);
                                        Point3d maxPt = btrAccumulatedPoints[1].TransformBy(blockTransform);
                                        var transformedBb = new Point3d[] { new Point3d(minPt.X, minPt.Y, 0), new Point3d(maxPt.X, maxPt.Y, 0) };

                                        bbs.Add(transformedBb);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return bbs;
        }

        public static List<Point3d> GetBoundingBoxFromBtrOfEntitiesNotOnLayer(Document doc, BlockReference blockRef, string substr)
        {
            if (blockRef == null)
                throw new ArgumentNullException(nameof(blockRef));

            Database db = doc.Database;
            Extents3d boundingBox = new Extents3d();
            bool foundBoundingBox = false;
            Matrix3d blockTransform = blockRef.BlockTransform;
            using (var docLock = doc.LockDocument())
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord blockTableRecord = trans.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                    foreach (ObjectId entityId in blockTableRecord)
                    {
                        Entity entity = trans.GetObject(entityId, OpenMode.ForRead) as Entity;
                        if (entity == null)
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(substr) || !entity.Layer.Contains(substr))
                        {
                            Extents3d? entityExtents = entity.Bounds;
                            if (entityExtents.HasValue)
                            {
                                boundingBox.AddExtents(entityExtents.Value);
                                foundBoundingBox = true;
                            }
                        }
                    }
                    trans.Dispose();
                }
            }

            if (foundBoundingBox)
            {
                return new List<Point3d>() { boundingBox.MinPoint, boundingBox.MaxPoint };
            }
            else
            {
                return new List<Point3d>() { new Point3d(0, 0, 0), new Point3d(0, 0, 0) };
            }
        }

        public static List<Point3d> GetBoundingBoxOfBtrEntitiesNotOnLayer(BlockReference blockRef, string substr)
        {
            if (blockRef == null)
                throw new ArgumentNullException(nameof(blockRef));

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Extents3d boundingBox = new Extents3d();
            bool foundBoundingBox = false;
            Matrix3d blockTransform = blockRef.BlockTransform;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord blockTableRecord = trans.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;

                foreach (ObjectId entityId in blockTableRecord)
                {
                    Entity entity = trans.GetObject(entityId, OpenMode.ForRead) as Entity;

                    if (entity != null && !entity.Layer.Contains(substr))
                    {
                        Extents3d? entityExtents = entity.Bounds;

                        if (entityExtents.HasValue)
                        {
                            boundingBox.AddExtents(entityExtents.Value);
                            foundBoundingBox = true;
                        }
                    }
                }
            }
            if (foundBoundingBox)
            {
                return new List<Point3d>() { boundingBox.MinPoint, boundingBox.MaxPoint };
            }
            else
            {
                return new List<Point3d>() { new Point3d(0, 0, 0), new Point3d(0, 0, 0) };
            }
        }
    }
}
