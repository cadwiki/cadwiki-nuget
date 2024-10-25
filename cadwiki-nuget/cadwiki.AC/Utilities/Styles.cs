
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace cadwiki.AC.Utilities
{

    public class Styles
    {
        public class TextStyleProps
        {
            public string FontFilePath;
            public double HeightInMm;
        }

        public class DimStyleProps
        {
            public string TextStyleName;
            public double PlaceholderValueInMm;
            public short DimCLRD;
            public short DimCLRE;
            public short DimCLRT;
            public double DimEXO;
            public double DimASZ;
            public double DimCEN;
            public double DimTVP;
            public int DimDEC;
            public int DimLUNIT;
        }


        public static void CreateTextStyle(Document doc, string styleName, TextStyleProps props)
        {
            var db = doc.Database;

            using (var @lock = doc.LockDocument())
            {
                using (var tm = db.TransactionManager.StartTransaction())
                {

                    TextStyleTable st = (TextStyleTable)tm.GetObject(db.TextStyleTableId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite, false);
                    var str = new TextStyleTableRecord();

                    bool doesStyleExist = DoesTextStyleExist(doc, styleName);
                    if (doesStyleExist)
                    {
                        var oId = st[styleName];
                        str = (TextStyleTableRecord)tm.GetObject(oId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite, false);
                    }

                    str.FileName = props.FontFilePath;

                    if (doesStyleExist == false)
                    {
                        str.Name = styleName;
                        st.Add(str);
                        tm.AddNewlyCreatedDBObject(str, true);
                    }

                    db.Textstyle = str.ObjectId;
                    tm.Commit();
                }
            }
        }

        public static bool DoesTextStyleExist(Document doc, string styleName)
        {
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var transaction = db.TransactionManager.StartTransaction())
                {

                    var dbObject = transaction.GetObject(db.TextStyleTableId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    TextStyleTable textStyleTable = (TextStyleTable)dbObject;

                    var textStyleRecord = new TextStyleTableRecord();

                    bool doesStyleExist = textStyleTable.Has(styleName);
                    if (doesStyleExist)
                    {
                        transaction.Commit();
                        return true;
                    }
                    transaction.Commit();
                }
            }
            return false;
        }


        public static void CreateDimensionStyle(Document doc, string styleName, DimStyleProps props)
        {
            var db = doc.Database;

            using (var @lock = doc.LockDocument())
            {
                using (var tm = db.TransactionManager.StartTransaction())
                {

                    DimStyleTable st = (DimStyleTable)tm.GetObject(db.DimStyleTableId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite, false);
                    var str = new DimStyleTableRecord();

                    bool doesStyleExist = DoesDimStyleExist(doc, styleName);
                    if (doesStyleExist)
                    {
                        var oId = st[styleName];
                        str = (DimStyleTableRecord)tm.GetObject(oId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite, false);
                    }

                    var objectId = GetTextStyleId(doc, props.TextStyleName);

                    str.Dimtxsty = objectId;
                    str.Dimclrd = Color.FromColorIndex(ColorMethod.ByAci, props.DimCLRD);
                    str.Dimclre = Color.FromColorIndex(ColorMethod.ByAci, props.DimCLRE);
                    str.Dimclrt = Color.FromColorIndex(ColorMethod.ByAci, props.DimCLRT);
                    str.Dimexo = props.DimEXO;
                    str.Dimasz = props.DimASZ;
                    str.Dimcen = props.DimCEN;
                    str.Dimtvp = props.DimTVP;
                    str.Dimadec = props.DimDEC;
                    str.Dimlunit = props.DimLUNIT;

                    if (doesStyleExist == false)
                    {
                        str.Name = styleName;
                        st.Add(str);
                        tm.AddNewlyCreatedDBObject(str, true);
                    }

                    tm.Commit();
                }
            }
        }

        public static bool DoesDimStyleExist(Document doc, string styleName)
        {
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var transaction = db.TransactionManager.StartTransaction())
                {

                    var dbObject = transaction.GetObject(db.DimStyleTableId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                    DimStyleTable dimStyleTable = (DimStyleTable)dbObject;

                    var dimStyleTableRecord = new DimStyleTableRecord();

                    bool doesStyleExist = dimStyleTable.Has(styleName);
                    if (doesStyleExist)
                    {
                        transaction.Commit();
                        return true;
                    }


                    transaction.Commit();
                }
            }
            return false;
        }

        public static ObjectId GetTextStyleId(Document doc, string styleName)
        {
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var transaction = db.TransactionManager.StartTransaction())
                {

                    var dbObject = transaction.GetObject(db.TextStyleTableId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                    TextStyleTable textStyleTable = (TextStyleTable)dbObject;
                    if (textStyleTable.Has(styleName))
                    {
                        transaction.Commit();
                        var objectId = textStyleTable[styleName];
                        return objectId;
                    }
                    transaction.Commit();
                }
            }
            return default;
        }
    }
}