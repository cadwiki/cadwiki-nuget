using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace cadwiki.AC.Utilities
{

    public class ObjectIds
    {
        public static Entity GetEntity(Document doc, ObjectId objectId)
        {
            var deletedEntities = new List<Entity>();
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    Entity entity = (Entity)t.GetObject(objectId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                    return entity;
                }
            }
        }
    }
}