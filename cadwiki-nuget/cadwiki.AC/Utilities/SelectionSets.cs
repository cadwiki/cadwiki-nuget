using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Microsoft.VisualBasic;

namespace cadwiki.AC
{

    public class SelectionSets
    {
        public static SelectionSet Merge(Document doc, SelectionSet ss, List<SelectionSet> ssList)
        {
            ObjectId[] tempIds = new ObjectId[] { };
            if (ss is not null)
            {
                tempIds = ss.GetObjectIds();
            }
            var mergedIdCollection = new ObjectIdCollection();
            if (tempIds.Count() > 0)
            {
                mergedIdCollection = new ObjectIdCollection(tempIds);
            }

            foreach (SelectionSet sSet in ssList)
            {
                if (sSet is not null)
                {
                    tempIds = sSet.GetObjectIds();
                }
                else
                {
                    tempIds = null;
                }
                if (tempIds is not null)
                {
                    var objectIdCollection2 = new ObjectIdCollection(tempIds);
                    var unionIds = Union(mergedIdCollection, objectIdCollection2);
                    mergedIdCollection = new ObjectIdCollection(unionIds.ToArray());
                }
            }
            ObjectId[] objectIds = mergedIdCollection.Cast<ObjectId>().ToArray();
            var ssReturn = SelectionSet.FromObjectIds(objectIds);
            return ssReturn;
        }

        public static IEnumerable<ObjectId> Union(ObjectIdCollection ids, ObjectIdCollection otherIds)
        {
            return ids.Cast<ObjectId>().ToArray().Union(otherIds.Cast<ObjectId>());
        }

        public static IEnumerable<ObjectId> UnionWithIds(ObjectIdCollection ids, IEnumerable<ObjectId> otherIds)
        {
            return ids.Cast<ObjectId>().Union(otherIds);
        }

        public static List<Entity> DeleteAllEntities(Document doc, SelectionSet ss)
        {
            var deletedEntities = new List<Entity>();
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord currentSpace = (BlockTableRecord)t.GetObject(db.CurrentSpaceId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    foreach (ObjectId objId in ss.GetObjectIds())
                    {
                        Entity entity = (Entity)t.GetObject(objId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        entity.Erase();
                        deletedEntities.Add(entity);
                    }
                    t.Commit();
                }
            }
            return deletedEntities;
        }

        public static SelectionSet SelectFence(Document doc, Point3dCollection polygon, SelectionFilter filter)
        {
            var selectionResult = doc.Editor.SelectFence(polygon, filter);
            var selectionSet = selectionResult.Value;
            if (selectionResult.Status == PromptStatus.Cancel)
            {
                doc.Editor.WriteMessage(Environment.NewLine + "Nothing selected");
                return null;
            }
            return selectionSet;
        }

        public static SelectionSet SelectFence(Document doc, Point3d pt1, Point3d pt2, SelectionFilter filter)
        {
            var polygon = new Point3dCollection();
            polygon.Add(pt1);
            polygon.Add(pt2);
            var selectionResult = doc.Editor.SelectFence(polygon, filter);
            var selectionSet = selectionResult.Value;
            if (selectionResult.Status == PromptStatus.Cancel)
            {
                doc.Editor.WriteMessage(Environment.NewLine + "Nothing selected");
                return null;
            }
            return selectionSet;
        }

        public static SelectionSet SelectAll(Document doc, SelectionFilter filter)
        {
            var selectionResult = doc.Editor.SelectAll(filter);
            var selectionSet = selectionResult.Value;
            if (selectionResult.Status == PromptStatus.Cancel)
            {
                doc.Editor.WriteMessage(Environment.NewLine + "Nothing selected");
                return null;
            }
            return selectionSet;
        }

        public static SelectionSet CrossingWindow(Document doc, Point3d point1, Point3d point2, SelectionFilter filter)
        {
            var selectionResult = doc.Editor.SelectCrossingWindow(point1, point2, filter);
            var selectionSet = selectionResult.Value;
            if (selectionResult.Status == PromptStatus.Cancel)
            {
                doc.Editor.WriteMessage(Environment.NewLine + "Nothing selected");
                return null;
            }
            return selectionSet;
        }

        public static SelectionSet ObjectIdListToSs(List<ObjectId> objectIdList)
        {
            ObjectId[] objectIdArray = objectIdList.Cast<ObjectId>().ToArray();
            var ssReturn = SelectionSet.FromObjectIds(objectIdArray);
            return ssReturn;
        }

        public static void HighlightAll(Document doc, SelectionSet ss, bool highlight)
        {
            var deletedEntities = new List<Entity>();
            var db = doc.Database;

            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord currentSpace = (BlockTableRecord)t.GetObject(db.CurrentSpaceId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    foreach (ObjectId objId in ss.GetObjectIds())
                    {
                        Entity entity = (Entity)t.GetObject(objId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        if (highlight)
                        {
                            entity.Highlight();
                        }
                        else
                        {
                            entity.Unhighlight();
                        }
                    }
                    t.Commit();
                }
            }
        }

        public static Entity GetEntityByHandle(Document doc, SelectionSet ss, string handle)
        {
            var deletedEntities = new List<Entity>();
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord currentSpace = (BlockTableRecord)t.GetObject(db.CurrentSpaceId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    foreach (ObjectId objId in ss.GetObjectIds())
                    {
                        Entity entity = (Entity)t.GetObject(objId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        if (entity.Handle.ToString().Equals(handle))
                        {
                            return entity;
                        }
                    }
                    t.Commit();
                }
            }
            return null;
        }

        public static List<Entity> GetEntityList(Document doc, SelectionSet ss)
        {
            var entities = new List<Entity>();
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord currentSpace = (BlockTableRecord)t.GetObject(db.CurrentSpaceId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    foreach (ObjectId objId in ss.GetObjectIds())
                    {
                        Entity entity = (Entity)t.GetObject(objId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        entities.Add(entity);
                    }
                }
            }
            return entities;
        }

        public static Point3d GetClosestPointOnAnyLineFromSelectionToAGivenPoint(Document doc, SelectionSet selectionSet, Point3d point)
        {

            Point3d closestPoint;
            Curve curveObj;
            var entities = new List<Entity>();
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    var closestPointList = new List<Tuple<double, Point3d>>();
                    foreach (ObjectId objectId in selectionSet.GetObjectIds())
                    {
                        Curve curve = objectId.GetObject(global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead) as Curve;
                        if (curve is not null)
                        {
                            closestPoint = curve.GetClosestPointTo(point, false);
                            closestPointList.Add(new Tuple<double, Point3d>(closestPoint.DistanceTo(point), closestPoint));
                        }
                    }

                    closestPointList.Sort((a, b) => a.Item1.CompareTo(b.Item1));

                    double distance = closestPointList[0].Item1;
                    closestPoint = closestPointList[0].Item2;

                    return closestPoint;
                }
            }


        }

        public static SelectionSet BlockRefListToSs(List<BlockReference> blockRefs)
        {
            var blockSs = SelectionSets.ObjectIdListToSs(blockRefs.Select(o => o.Id).ToList());
            return blockSs;
        }

    }
}