
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace cadwiki.AC
{

    public class Polylines
    {
        public static bool IsPointInside(Polyline pline, Point3d point)
        {
            var dbCollection = new DBObjectCollection();
            dbCollection.Add(pline);

            var regionCollection = Region.CreateFromCurves(dbCollection);
            Region acRegion = (Region)regionCollection[0];

            var pointCont = new PointContainment();

            var brep = new Brep(acRegion);

            brep.GetPointContainment(point, out pointCont);
            if (!(pointCont == PointContainment.Outside))
            {
                return true;
            }
            return false;
        }
    }
}