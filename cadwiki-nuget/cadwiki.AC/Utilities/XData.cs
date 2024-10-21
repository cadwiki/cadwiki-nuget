using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace cadwiki.AC
{

    public class XData
    {
        public static XDataFound GetXDataByRegAppAndKey(Document doc, ObjectId objId, string regApp, string xDataKey)
        {
            var db = doc.Database;
            var xDataNotFound = new XDataFound();
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    Entity entity = (Entity)t.GetObject(objId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead, false);
                    string handle = entity.Handle.ToString();
                    var rb = entity.XData;
                    if (rb is not null)
                    {
                        var found = GetXDataValueFromBuffer(rb, regApp, xDataKey);
                        return found;
                    }
                    t.Commit();
                }
            }
            return xDataNotFound;
        }

        public class XDataFound
        {
            public int Index = -1;
            public string Value = "";
        }

        public static XDataFound GetXDataValueFromBuffer(ResultBuffer rb, string regApp, string xDataKey)
        {
            TypedValue[] tvArray = rb.AsArray();
            bool hasTagBeenFound = false;
            string xDataTag = "";
            string xDataValue = "";
            int i = 0;
            var found = new XDataFound();
            var xDataNotFound = new XDataFound();
            foreach (TypedValue tv in tvArray)
            {
                short code = tv.TypeCode;
                switch (code)
                {
                    case 1001:
                        {
                            string appName = tv.Value.ToString();
                            if (!appName.Equals(regApp))
                            {
                                return xDataNotFound;
                                // "{" or "}" are stored as control strings
                            }

                            break;
                        }

                    case 1002:
                        {
                            // Tags and Values are stored as strings
                            string controlString = tv.Value.ToString();
                            break;
                        }

                    case 1000:
                        {
                            if (hasTagBeenFound == false)
                            {
                                xDataTag = tv.Value.ToString();
                                hasTagBeenFound = true;
                            }
                            else
                            {
                                xDataValue = tv.Value.ToString();
                                if (xDataTag.Equals(xDataKey))
                                {
                                    found.Index = i;
                                    found.Value = xDataValue;
                                    return found;
                                }
                                xDataTag = "";
                                xDataValue = "";
                                hasTagBeenFound = false;
                            }

                            break;
                        }

                }
                i = i + 1;
            }
            return xDataNotFound;
        }

        public static void AddRegAppTableRecord(string regAppName)
        {
            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;

            var db = doc.Database;

            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    RegAppTable rat = (RegAppTable)t.GetObject(db.RegAppTableId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead, false);
                    if (!rat.Has(regAppName))
                    {
                        rat.UpgradeOpen();
                        var ratr = new RegAppTableRecord();
                        ratr.Name = regAppName;
                        rat.Add(ratr);
                        t.AddNewlyCreatedDBObject(ratr, true);
                    }
                    t.Commit();
                }

            }
        }

        public static ResultBuffer StringValue(string appKey, string dataKey, string dataValue)
        {
            return new ResultBuffer(new TypedValue(1001, appKey), new TypedValue(1002, "{"), new TypedValue(1000, dataKey), new TypedValue(1000, dataValue), new TypedValue(1002, "}"));
        }

        public static ResultBuffer CreateAppKey(string appKey)
        {
            return new ResultBuffer(new TypedValue(1001, appKey));
        }


        public static ResultBuffer CreateStringKeyAndValue(string dataKey, string dataValue)
        {
            return new ResultBuffer(new TypedValue(1002, "{"), new TypedValue(1000, dataKey), new TypedValue(1000, dataValue), new TypedValue(1002, "}"));
        }

        public static TypedValue[] CreateTypedValues(string dataKey, string dataValue)
        {
            TypedValue[] typesValues = new TypedValue[] { new TypedValue(1002, "{"), new TypedValue(1000, dataKey), new TypedValue(1000, dataValue), new TypedValue(1002, "}") };

            return typesValues;
        }

        public static ResultBuffer AddTypedValues(ResultBuffer xDataBuffer, TypedValue[] typedValues)
        {
            foreach (TypedValue tv in typedValues)
                xDataBuffer.Add(tv);
            return xDataBuffer;
        }

        public static ResultBuffer RemovedTypeValue(ResultBuffer xDataBuffer, string regApp, string xDataKey)
        {
            var found = GetXDataValueFromBuffer(xDataBuffer, regApp, xDataKey);
            var newBuffer = new ResultBuffer();
            if (found is not null)
            {
                if (!(found.Index == -1))
                {
                    TypedValue[] tvArray = xDataBuffer.AsArray();
                    int i = 0;
                    foreach (TypedValue tv in tvArray)
                    {
                        if (!(i == found.Index) & !(i + 1 == found.Index) & !(i - 1 == found.Index) & !(i + 2 == found.Index))
                        {
                            newBuffer.Add(tv);
                        }
                        i = i + 1;
                    }
                    return newBuffer;
                }
            }
            return xDataBuffer;
        }

        public static List<ObjectId> GetSsOfMatchingXdata(Document doc, SelectionSet ss, string xDataRegAppKey, string xDataKey, string xdataValue)
        {
            var matchingXDataObjectIdList = new List<ObjectId>();
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord currentSpace = (BlockTableRecord)t.GetObject(db.CurrentSpaceId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    foreach (ObjectId objId in ss.GetObjectIds())
                    {
                        Entity entity = (Entity)t.GetObject(objId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                        var existingXData = entity.XData;
                        if (existingXData == null)
                        {
                            continue;
                        }
                        var xdataFound = GetXDataValueFromBuffer(existingXData, xDataRegAppKey, xDataKey);
                        if (xdataFound is not null)
                        {
                            if (!(xdataFound.Index == -1))
                            {
                                if ((xdataFound.Value ?? "") == (xdataValue ?? ""))
                                {
                                    matchingXDataObjectIdList.Add(entity.Id);
                                }
                            }
                        }
                    }
                    t.Commit();
                }
            }
            return matchingXDataObjectIdList;
        }

        public static ResultBuffer SetXdataValue(Document doc, ResultBuffer existingBuffer, string xDataRegAppKey, string xDataKey, string xdataValue)
        {
            string existingDataEntry = GetXDataValueFromBuffer(existingBuffer, xDataRegAppKey, xDataKey).Value;
            TypedValue[] newKeyAndValue = CreateTypedValues(xDataKey, xdataValue);
            var newBuffer = existingBuffer;
            if (existingDataEntry is null)
            {
                newBuffer = AddTypedValues(newBuffer, newKeyAndValue);
            }
            else
            {
                newBuffer = RemovedTypeValue(newBuffer, xDataRegAppKey, xDataKey);
                newBuffer = AddTypedValues(newBuffer, newKeyAndValue);
            }
            return newBuffer;
        }
    }
}