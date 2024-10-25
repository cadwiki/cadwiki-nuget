using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cadwiki.AC.Utilities
{
    public class BoundingBoxes
    {
        public static List<Point3d> GetSsBoundingBox(SelectionSet ss, Database db)
        {
            Point3d minPoint = new Point3d(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3d maxPoint = new Point3d(double.MinValue, double.MinValue, double.MinValue);
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (SelectedObject selectedObject in ss)
                {
                    if (selectedObject != null)
                    {
                        Entity entity = tr.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as Entity;
                        if (entity != null)
                        {
                            // Get the entity's extents
                            Extents3d? extents = entity.Bounds;
                            if (extents.HasValue)
                            {
                                minPoint = new Point3d(
                                    Math.Min(minPoint.X, extents.Value.MinPoint.X),
                                    Math.Min(minPoint.Y, extents.Value.MinPoint.Y),
                                    Math.Min(minPoint.Z, extents.Value.MinPoint.Z));

                                maxPoint = new Point3d(
                                    Math.Max(maxPoint.X, extents.Value.MaxPoint.X),
                                    Math.Max(maxPoint.Y, extents.Value.MaxPoint.Y),
                                    Math.Max(maxPoint.Z, extents.Value.MaxPoint.Z));
                            }
                        }
                    }
                }
                tr.Commit();
            }
            return new List<Point3d> { minPoint, maxPoint };
        }
    }
}
