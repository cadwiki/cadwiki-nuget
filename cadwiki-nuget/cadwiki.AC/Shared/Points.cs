using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Microsoft.VisualBasic.CompilerServices;

namespace cadwiki.AC
{

    public class Points
    {
        public static Point3d RoundToNearestWholeNumber(Point3d point)
        {
            double x = Math.Round(point.X, 0);
            double y = Math.Round(point.Y, 0);
            double z = Math.Round(point.Z, 0);
            var newPoint = new Point3d(x, y, z);

            return newPoint;
        }

        public static Point3d ToTwoDecimalPlaces(Point3d point)
        {
            double x = Conversions.ToDouble(point.X.ToString("N2"));
            double y = Conversions.ToDouble(point.Y.ToString("N2"));
            double z = Conversions.ToDouble(point.Z.ToString("N2"));
            var newPoint = new Point3d(x, y, z);
            return newPoint;
        }

        public static Point3d ToThreeDecimalPlaces(Point3d point)
        {
            double x = Conversions.ToDouble(point.X.ToString("N3"));
            double y = Conversions.ToDouble(point.Y.ToString("N3"));
            double z = Conversions.ToDouble(point.Z.ToString("N3"));
            var newPoint = new Point3d(x, y, z);
            return newPoint;
        }

        public static Point3d ToFourDecimalPlaces(Point3d point)
        {
            double x = Conversions.ToDouble(point.X.ToString("N4"));
            double y = Conversions.ToDouble(point.Y.ToString("N4"));
            double z = Conversions.ToDouble(point.Z.ToString("N4"));
            var newPoint = new Point3d(x, y, z);
            return newPoint;
        }

        public static Point3d PolarPoint(Point3d pPt, double radians, double dDist)
        {

            return new Point3d(pPt.X + dDist * Math.Cos(radians), pPt.Y + dDist * Math.Sin(radians), pPt.Z);
        }

        public static Point2d Convert3dTo2d(Point3d point3d)
        {
            var point2d = new Point2d(point3d.X, point3d.Y);
            return point2d;
        }

        public static Point3d MidPoint3d(Point3d pt1, Point3d pt2)
        {
            double midX = (pt1.X + pt2.X) / 2.0d;
            double midY = (pt1.Y + pt2.Y) / 2.0d;
            double midZ = (pt1.Z + pt2.Z) / 2.0d;

            var point3d = new Point3d(midX, midY, midZ);
            return point3d;
        }

        public static Point2dCollection GetAllVertices(Polyline polyline)
        {
            var verCollection = new Point2dCollection();
            Point2d vertex;
            int i = 0;
            var loopTo = polyline.NumberOfVertices - 1;
            for (i = 0; i <= loopTo; i++)
            {
                vertex = polyline.GetPoint2dAt(i);
                verCollection.Add(vertex);
            }
            return verCollection;
        }

        public static Point3d TransformByUCS(Point3d point, Database db)
        {
            return point.TransformBy(GetUcsMatrix(db));
        }

        public static Matrix3d GetUcsMatrix(Database db)
        {
            var origin = db.Ucsorg;
            var xAxis = db.Ucsxdir;
            var yAxis = db.Ucsydir;
            var zAxis = xAxis.CrossProduct(yAxis);
            return Matrix3d.AlignCoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis, origin, xAxis, yAxis, zAxis);
        }

        public static List<Point3d> SortPointsByProximityToPoint(List<Point3d> points, Point3d point)
        {
            IEnumerable<Point3d> sortedPoints = from o in points
                                                orderby o.DistanceTo(point)
                                                select o;
            return sortedPoints.ToList();
            return null;
        }
    }
}