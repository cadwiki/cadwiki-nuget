using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Microsoft.VisualBasic;

namespace cadwiki.AutoCAD2021.Base.Utilities.Workflows
{
    public class BreakSs
    {

        public class UserInput
        {
            public string LayerNameToBreak;
        }

        public class Result
        {

        }

        public class BreakSsInputs
        {
            public SelectionSet SelectionToBreak;
            public SelectionSet SelectionToBreakWith;
            public bool Self;
            public double Gap;
            public string NewLayer = "ss-broken-at-intersection-points";
            public bool DeleteOriginal;
        }

        public class BreakList
        {
            public Dictionary<ObjectId, List<Point3d>> ObjectidToBreakPointList = new Dictionary<ObjectId, List<Point3d>>();
        }


        public static List<ObjectId> BreakSsWithSs(Document doc, BreakSsInputs inputs)
        {
            var intersectionPoints = new Point3dCollection();
            var breakList = new BreakList();
            var newIds = new List<ObjectId>();
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    var ltr = Layers.GetLayer(doc, inputs.NewLayer);
                    if (ltr is null)
                    {
                        ltr = Layers.CreateFirstAvailableLayerName(doc, inputs.NewLayer);
                    }
                    ltr.UpgradeOpen();
                    ltr.Color = Color.FromColorIndex(ColorMethod.ByLayer, 4);
                    foreach (ObjectId objIdToBreak in inputs.SelectionToBreak.GetObjectIds())
                    {
                        var entityBreakPoints = new List<Point3d>();
                        Entity entityToBreak = (Entity)t.GetObject(objIdToBreak, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        foreach (ObjectId objIdToBreakWith in inputs.SelectionToBreakWith.GetObjectIds())
                        {
                            Entity entityToBreakWith = (Entity)t.GetObject(objIdToBreakWith, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                            intersectionPoints = new Point3dCollection();
                            if (inputs.Self | !(objIdToBreak == objIdToBreakWith))
                            {
                                entityToBreak.IntersectWith(entityToBreakWith, Intersect.OnBothOperands, intersectionPoints, IntPtr.Zero, IntPtr.Zero);
                            }
                            if (intersectionPoints.Count > 0)
                            {
                                entityBreakPoints.AddRange(intersectionPoints.Cast<Point3d>().ToList());
                            }
                        }
                        breakList.ObjectidToBreakPointList.Add(objIdToBreak, entityBreakPoints);
                    }
                    foreach (KeyValuePair<ObjectId, List<Point3d>> pair in breakList.ObjectidToBreakPointList)
                    {
                        var objectId = pair.Key;
                        Line lineToBreak = (Line)t.GetObject(objectId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        var breakPointList = pair.Value;

                        var sortedBreakPoints = Points.SortPointsByProximityToPoint(breakPointList, lineToBreak.StartPoint);
                        var nearSide = lineToBreak.StartPoint;
                        var farSide = sortedBreakPoints[0];
                        BlockTableRecord curSpace = (BlockTableRecord)t.GetObject(db.CurrentSpaceId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        Line newLine;
                        foreach (Point3d point in sortedBreakPoints)
                        {
                            farSide = point;
                            newLine = new Line(nearSide, farSide);
                            newLine.Layer = inputs.NewLayer;
                            nearSide = farSide;
                            curSpace.AppendEntity(newLine);
                            t.AddNewlyCreatedDBObject(newLine, true);
                        }

                        farSide = lineToBreak.EndPoint;
                        newLine = new Line(nearSide, farSide);
                        newLine.Layer = inputs.NewLayer;
                        nearSide = farSide;
                        var newId = curSpace.AppendEntity(newLine);
                        newIds.Add(newId);
                        t.AddNewlyCreatedDBObject(newLine, true);
                        newIds.Add(newId);
                        if (inputs.DeleteOriginal)
                        {
                            lineToBreak.Erase();
                        }

                    }
                    t.Commit();
                }
            }
            return newIds;
        }

        public static Result Run()
        {
            try
            {
                var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                var userInput = GetUserInput();
                if (userInput is null)
                {
                }
                // Commands.AutoCADExceptions.WriteInvalidUserInputToCommandLine()
                else
                {
                    return RunWithUserInput(userInput);
                }
            }
            catch (Exception ex)
            {
                // Commands.AutoCADExceptions.Handle(ex)
            }
            return null;
        }

        private static UserInput GetUserInput()
        {
            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var promptEntityOptions = new PromptEntityOptions("Select entity on layer to break: " + Environment.NewLine);
            var objectId = Utilities.UserInput.GetEntityFromUser(doc, promptEntityOptions);
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    Entity entityOnLayer = (Entity)t.GetObject(objectId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    string layer = entityOnLayer.Layer;
                    var userInput = new UserInput();
                    userInput.LayerNameToBreak = layer;
                    return userInput;
                }
            }
            return null;
        }

        public static Result RunWithUserInput(UserInput userInput)
        {
            try
            {
                var acadApp = Application.AcadApplication;
                var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                var filter = SelectionFilters.GetAllLineBasedEntitiesOnLayer(userInput.LayerNameToBreak);
                var selectionToBreak = SelectionSets.SelectAll(doc, filter);
                var inputs = new BreakSsInputs();
                inputs.SelectionToBreak = selectionToBreak;
                inputs.SelectionToBreakWith = selectionToBreak;
                inputs.Self = true;
                BreakSsWithSs(doc, inputs);
                var result = new Result();
                return result;
            }
            catch (Exception ex)
            {
                // AutoCADExceptions.Handle(ex)
            }

            return null;

        }








    }

}