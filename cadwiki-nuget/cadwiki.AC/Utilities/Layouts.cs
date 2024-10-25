
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace cadwiki.AC.Utilities
{

    public class Layouts
    {
        public static ObjectId CreateTab(Document doc, string originalTabName)
        {
            string actualTabName = originalTabName;
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {

                    var existingId = LayoutManager.Current.GetLayoutId(actualTabName);
                    int count = 1;
                    while (!existingId.IsNull)
                    {
                        actualTabName = originalTabName + "-" + count.ToString();
                        existingId = LayoutManager.Current.GetLayoutId(actualTabName);
                        count += 1;
                    }
                    var id = LayoutManager.Current.CreateLayout(actualTabName);
                    var dbObj = t.GetObject(id, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);
                    Layout layout = (Layout)dbObj;
                    LayoutManager.Current.CurrentLayout = layout.LayoutName;
                    // t.AddNewlyCreatedDBObject(dbObj, True)
                    t.Commit();
                    return id;
                }
            }

            return default;
        }

        public static void SetCurrentTab(Document doc, string tab)
        {
            var db = doc.Database;
            int number2 = db.TransactionManager.NumberOfActiveTransactions;
            using (var @lock = doc.LockDocument())
            {
                using (var tm = db.TransactionManager.StartTransaction())
                {
                    var existingId = LayoutManager.Current.GetLayoutId(tab);
                    if (!existingId.IsNull)
                    {
                        LayoutManager.Current.CurrentLayout = tab;
                        tm.Commit();
                    }

                }
            }
        }
    }
}