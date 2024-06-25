using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace cadwiki.AutoCAD2021.Base.Utilities
{

    public class Layers
    {
        public static List<Entity> CopyVisibleEntitiesToNewLayer(Document doc, SelectionSet ss, LayerTableRecord newLayer)
        {
            var copiedEntities = new List<Entity>();
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord currentSpace = (BlockTableRecord)t.GetObject(db.CurrentSpaceId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    foreach (ObjectId objId in ss.GetObjectIds())
                    {
                        Entity entity = (Entity)t.GetObject(objId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                        if (entity is not null && entity.Visible)
                        {
                            Entity newEntity = entity.Clone() as Entity;
                            newEntity.LayerId = newLayer.Id;
                            var objectId = currentSpace.AppendEntity(newEntity);
                            t.AddNewlyCreatedDBObject(newEntity, true);
                            copiedEntities.Add(newEntity);
                        }
                    }
                    t.Commit();
                }
            }
            return copiedEntities;
        }

        public static void ThrowErrorIfLayerDoesNotExist(Document doc, string layerName)
        {
            var layer = GetLayer(doc, layerName);
            if (layer is null)
            {
                throw new Exception("Layer " + layerName + " does not exist in dwg.");
            }
        }

        public static LayerTableRecord CreateFirstAvailableLayerName(Document doc, string layerName)
        {
            int i = 0;
            string currentLayerName = layerName;
            bool layerExists = DoesLayerExist(doc, currentLayerName);
            while (layerExists)
            {
                i = i + 1;
                currentLayerName = layerName + "(" + i.ToString() + ")";
                layerExists = DoesLayerExist(doc, currentLayerName);
            }
            var layerTableRecord = CreateLayer(doc, currentLayerName);
            return layerTableRecord;
        }

        public static bool DoesLayerExist(Document doc, string layerName)
        {
            var layerTableRecord = GetLayer(doc, layerName);
            if (layerTableRecord is null)
            {
                return false;
            }
            return true;
        }

        public static LayerTableRecord CreateLayer(Document doc, string layerName)
        {
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var transaction = db.TransactionManager.StartTransaction())
                {
                    var dbObject = transaction.GetObject(db.LayerTableId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    LayerTable layerTable = (LayerTable)dbObject;

                    if (layerTable.Has(layerName))
                    {
                        var layerId = layerTable[layerName];
                        var layerObject = transaction.GetObject(layerId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                        LayerTableRecord layerTableRecord = (LayerTableRecord)layerObject;
                        return layerTableRecord;
                    }
                    else
                    {
                        var newRecord = new LayerTableRecord();
                        newRecord.Name = layerName;
                        layerTable.Add(newRecord);
                        transaction.AddNewlyCreatedDBObject(newRecord, true);
                        transaction.Commit();
                        return newRecord;
                    }

                }
            }
        }


        public static LayerTableRecord GetLayer(Document doc, string layerName)
        {
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var transaction = db.TransactionManager.StartTransaction())
                {
                    var dbObject = transaction.GetObject(db.LayerTableId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                    LayerTable layerTable = (LayerTable)dbObject;
                    if (layerTable.Has(layerName))
                    {
                        var layerId = layerTable[layerName];
                        var layerObject = transaction.GetObject(layerId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                        LayerTableRecord layerTableRecord = (LayerTableRecord)layerObject;
                        transaction.Commit();
                        return layerTableRecord;
                    }

                }
            }
            return null;
        }

        public static bool Delete(Document doc, string layerName)
        {
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var transaction = db.TransactionManager.StartTransaction())
                {
                    var dbObject = transaction.GetObject(db.LayerTableId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                    LayerTable layerTable = (LayerTable)dbObject;
                    if (layerTable.Has(layerName))
                    {
                        var layerId = layerTable[layerName];
                        var layerObject = transaction.GetObject(layerId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        LayerTableRecord layerTableRecord = (LayerTableRecord)layerObject;
                        layerTableRecord.Erase(true);
                        transaction.Commit();
                        return true;
                    }

                }
            }
            return false;
        }

        public static void SetLayerCurrent(Document doc, string layerName)
        {
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var transaction = db.TransactionManager.StartTransaction())
                {
                    var dbObject = transaction.GetObject(db.LayerTableId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                    LayerTable layerTable = (LayerTable)dbObject;
                    if (layerTable.Has(layerName) == true)
                    {
                        db.Clayer = layerTable[layerName];
                        transaction.Commit();
                    }
                }
            }
        }

        public static void TurnAllLayersOfExcept(Document doc, List<string> layerNames)
        {
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var transaction = db.TransactionManager.StartTransaction())
                {
                    var dbObject = transaction.GetObject(db.LayerTableId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    LayerTable layerTable = (LayerTable)dbObject;
                    foreach (ObjectId id in layerTable)
                    {
                        LayerTableRecord layerTableRecord = (LayerTableRecord)transaction.GetObject(id, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        string layerName = layerTableRecord.Name;
                        if (!layerNames.Contains(layerName))
                        {
                            layerTableRecord.IsOff = true;
                        }
                    }
                    transaction.Commit();
                }
            }
        }
    }
}