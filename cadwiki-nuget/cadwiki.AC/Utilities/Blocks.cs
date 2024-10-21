using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Microsoft.VisualBasic;
using OpenMode = Autodesk.AutoCAD.DatabaseServices.OpenMode;

namespace cadwiki.AC.Utilities
{

    public class Blocks
    {
        public static List<BlockReference> SortSelectionSetByProximityToPoint(Document doc, SelectionSet ss, Point3d point)
        {
            var blockRefs = new List<BlockReference>();
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord currentSpace = (BlockTableRecord)t.GetObject(db.CurrentSpaceId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    ObjectId[] objectIds = ss.GetObjectIds();
                    foreach (ObjectId objId in objectIds)
                    {
                        BlockReference blockRef = (BlockReference)t.GetObject(objId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        blockRefs.Add(blockRef);
                    }

                    IEnumerable<BlockReference> sortedBlockRefs = from o in blockRefs
                                                                  orderby o.Position.DistanceTo(point)
                                                                  select o;
                    return sortedBlockRefs.ToList();
                }
            }
            return null;
        }

        public static string GetAttributeValue(Document doc, BlockReference blockRef, string attributeName)
        {
            var db = doc.Database;

            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId attId in blockRef.AttributeCollection)
                    {
                        try
                        {
                            AttributeReference attRef = (AttributeReference)t.GetObject(attId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead, false);
                            if ((attRef.Tag ?? "") == (attributeName ?? ""))
                            {
                                return attRef.TextString;
                            }
                        }
                        catch (global::Autodesk.AutoCAD.Runtime.Exception ex)
                        {
                            Debug.WriteLine("Attribute no longer exists because it was deleted.");
                        }

                    }
                }
            }


            return null;
        }

        public static bool SetEntityAttributeValue(Entity entity, string attributeName, string newValue)
        {
            bool wasAttributeUpdated = false;
            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var objectIdsToInsertionPoints = new Dictionary<ObjectId, Point3d>();
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var transaction = db.TransactionManager.StartTransaction())
                {
                    BlockReference blockRefObj = (BlockReference)entity;
                    var attributeCollection = blockRefObj.AttributeCollection;
                    foreach (ObjectId attId in attributeCollection)
                    {
                        try
                        {
                            var obj = transaction.GetObject(attId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                            AttributeReference attribute = (AttributeReference)obj;
                            if ((attribute.Tag ?? "") == (attributeName ?? ""))
                            {
                                attribute.TextString = newValue;
                                wasAttributeUpdated = true;
                            }
                        }
                        catch (global::Autodesk.AutoCAD.Runtime.Exception ex)
                        {
                            Debug.WriteLine("Attribute no longer exists because it was deleted.");
                        }

                    }
                    transaction.Commit();
                }
            }
            return wasAttributeUpdated;
        }

        public static void AddAttributesFromBlockTable(Document doc, BlockTableRecord acBlkTblRec, BlockReference acBlkRef)
        {
            var attDefClass = RXObject.GetClass(typeof(AttributeDefinition));
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId objID in acBlkTblRec)
                    {
                        if (objID.ObjectClass.IsDerivedFrom(attDefClass))
                        {
                            var dbObj = tr.GetObject(objID, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                            if (dbObj is AttributeDefinition)
                            {
                                AttributeDefinition acAtt = (AttributeDefinition)dbObj;
                                if (!acAtt.Constant)
                                {
                                    using (var acAttRef = new AttributeReference())
                                    {
                                        acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform);
                                        acAttRef.Position = acAtt.Position.TransformBy(acBlkRef.BlockTransform);
                                        acAttRef.TextString = acAtt.TextString;
                                        acBlkRef.AttributeCollection.AppendAttribute(acAttRef);
                                        tr.AddNewlyCreatedDBObject(acAttRef, true);
                                    }
                                }
                            }
                        }
                    }
                    tr.Commit();
                }
            }
        }

        public static void DeleteAttributes(Document doc, BlockReference acBlkRef)
        {
            var attDefClass = RXObject.GetClass(typeof(AttributeDefinition));
            var db = doc.Database;
            var purgedIds = new ObjectIdCollection();

            using (var @lock = doc.LockDocument())
            {
                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    if (acBlkRef.AttributeCollection.Count > 0)
                    {
                        acBlkRef.UpgradeOpen();
                        foreach (ObjectId attId in acBlkRef.AttributeCollection)
                        {
                            if (attId.IsErased == false)
                            {
                                AttributeReference attRef = (AttributeReference)tr.GetObject(attId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                                purgedIds.Add(attId);
                                attRef.Erase(true);
                            }
                        }
                    }
                    db.Purge(purgedIds);
                    tr.Commit();
                }
            }
        }

        public static BlockReference InsertBlockFromFile(Document doc, string filePath, Point3d insertionPoint, double rotation, string layerName)
        {
            var db = doc.Database;
            using (var xDb = new Database(false, true))
            {
                xDb.ReadDwgFile(filePath, FileOpenMode.OpenForReadAndReadShare, true, null);
                using (var @lock = doc.LockDocument())
                {
                    using (var tr = doc.TransactionManager.StartTransaction())
                    {
                        string name = SymbolUtilityServices.GetBlockNameFromInsertPathName(filePath);
                        var id = db.Insert(name, xDb, true);
                        if (id.IsNull)
                        {
                            doc.Editor.WriteMessage(Environment.NewLine + "Failed to insert block");
                            return null;
                        }
                        BlockTableRecord currSpace = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        var coordS = new CoordinateSystem3d(insertionPoint, db.Ucsxdir, db.Ucsydir);
                        // Determine UCS
                        var insert = new BlockReference(insertionPoint, id);
                        insert.Normal = coordS.Zaxis; // Align to UCS
                        insert.Rotation = rotation;
                        currSpace.AppendEntity(insert);
                        insert.SetDatabaseDefaults();
                        insert.Layer = layerName;
                        tr.AddNewlyCreatedDBObject(insert, true);
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(insert.BlockTableRecord, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        AddAttributesFromBlockTable(doc, btr, insert);
                        // Autodesk.AutoCAD.DatabaseServices.SynchronizeAttributes(btr, insert)
                        tr.Commit();
                        return insert;
                    }
                }

            }
        }


        public static List<string> GetAllBlockNamesFromDrawing(Document doc)
        {
            List<string> blockNames = new List<string>();
            try
            {
                Database db = doc.Database;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable blockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (blockTable != null)
                    {
                        foreach (ObjectId blockId in blockTable)
                        {
                            BlockTableRecord blockRecord = tr.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
                            if (blockRecord != null)
                            {
                                blockNames.Add(blockRecord.Name);
                            }
                        }
                    }
                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.WriteToEditor(ex);
            }

            return blockNames;
        }
    }


    namespace Autodesk.AutoCAD.DatabaseServices
    {
        static class ExtensionMethods
        {
            public static RXClass attDefClass = RXObject.GetClass(typeof(AttributeDefinition));

            public static void SynchronizeAttributes(this BlockTableRecord target, BlockReference instance)
            {
                if (target is null)
                    throw new ArgumentNullException("target");
                var tr = target.Database.TransactionManager.TopTransaction;
                if (tr is null)
                    throw new global::Autodesk.AutoCAD.Runtime.Exception(ErrorStatus.NoActiveTransactions);
                var attDefs = target.GetAttributes(tr);

                foreach (ObjectId id in target.GetBlockReferenceIds(true, false))
                {
                    BlockReference br = (BlockReference)tr.GetObject(id, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    br.ResetAttributes(attDefs, tr);
                }

                if (target.IsDynamicBlock == false)
                {
                    target.UpdateAnonymousBlocks();

                    foreach (ObjectId id in target.GetAnonymousBlockIds())
                    {
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(id, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                        attDefs = btr.GetAttributes(tr);

                        foreach (ObjectId brId in btr.GetBlockReferenceIds(true, false))
                        {
                            BlockReference br = (BlockReference)tr.GetObject(brId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                            br.ResetAttributes(attDefs, tr);
                        }
                    }

                    attDefs = target.GetAttributes(tr);
                    instance.ResetAttributes(attDefs, tr);

                }
            }

            private static List<AttributeDefinition> GetAttributes(this BlockTableRecord target, Transaction tr)
            {
                var attDefs = new List<AttributeDefinition>();

                foreach (ObjectId id in target)
                {

                    if (id.ObjectClass == attDefClass)
                    {
                        AttributeDefinition attDef = (AttributeDefinition)tr.GetObject(id, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                        attDefs.Add(attDef);
                    }
                }

                return attDefs;
            }

            private static void ResetAttributes(this BlockReference br, List<AttributeDefinition> attDefs, Transaction tr)
            {
                var attValues = new Dictionary<string, string>();

                foreach (ObjectId id in br.AttributeCollection)
                {

                    if (!id.IsErased)
                    {
                        AttributeReference attRef = (AttributeReference)tr.GetObject(id, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        attValues.Add(attRef.Tag, attRef.IsMTextAttribute ? attRef.MTextAttribute.Contents : attRef.TextString);
                        attRef.Erase();
                    }
                }

                foreach (AttributeDefinition attDef in attDefs)
                {
                    var attRef = new AttributeReference();
                    attRef.SetAttributeFromBlock(attDef, br.BlockTransform);

                    if (attDef.Constant)
                    {
                        attRef.TextString = attDef.IsMTextAttributeDefinition ? attDef.MTextAttributeDefinition.Contents : attDef.TextString;
                    }
                    else if (attValues.ContainsKey(attRef.Tag))
                    {
                        attRef.TextString = attValues[attRef.Tag];
                    }

                    br.AttributeCollection.AppendAttribute(attRef);
                    tr.AddNewlyCreatedDBObject(attRef, true);
                }
            }
        }
    }
}