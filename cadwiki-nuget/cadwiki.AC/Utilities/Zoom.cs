
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Microsoft.VisualBasic.CompilerServices;

namespace cadwiki.AC.Utilities
{

    public class Zoom
    {
        public static void Window(Document doc, Point3d min, Point3d max)
        {
            using (var @lock = doc.LockDocument())
            {
                using (var tr = doc.TransactionManager.StartTransaction())
                {

                    var min2d = new Point2d(min.X, min.Y);
                    var max2d = new Point2d(max.X, max.Y);

                    var view = new ViewTableRecord();

                    view.CenterPoint = min2d + (max2d - min2d) / 2.0d;
                    view.Height = max2d.Y - min2d.Y;
                    view.Width = max2d.X - min2d.X;
                    doc.Editor.SetCurrentView(view);
                    tr.Commit();
                }
            }
        }

        public static void Extents(Document doc)
        {
            using (var @lock = doc.LockDocument())
            {
                using (var tr = doc.TransactionManager.StartTransaction())
                {

                    var db = doc.Database;

                    var min = db.Extmin;
                    var max = db.Extmax;

                    var min2d = new Point2d(min.X, min.Y);
                    var max2d = new Point2d(max.X, max.Y);

                    var view = new ViewTableRecord();

                    view.CenterPoint = min2d + (max2d - min2d) / 2.0d;
                    view.Height = max2d.Y - min2d.Y;
                    view.Width = max2d.X - min2d.X;
                    doc.Editor.SetCurrentView(view);
                    tr.Commit();
                }
            }
        }

        public class ZoomCoordinates
        {
            public Point2d LowerLeft;
            public Point2d UpperRight;

            public ZoomCoordinates(Point2d lowerLeftPt, Point2d upperRightPt)
            {
                LowerLeft = lowerLeftPt;
                UpperRight = upperRightPt;
            }
        }

        public static ZoomCoordinates GetApplicationViewZoomCoordinates()
        {
            Point3d sysVarViewCenter = (Point3d)global::Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("VIEWCTR");
            double centerX = sysVarViewCenter.X;
            double centerY = sysVarViewCenter.Y;

            Point2d sysVarViewResolution = (Point2d)global::Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("SCREENSIZE");
            double viewResolution0 = sysVarViewResolution.X;
            double viewResolution1 = sysVarViewResolution.Y;


            double sysVarViewHeight = Conversions.ToDouble(global::Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("VIEWSIZE"));
            double viewWidth = sysVarViewHeight * (viewResolution0 / viewResolution1);

            double minX = centerX - viewWidth / 2d;
            double minY = centerY - sysVarViewHeight / 2d;
            double maxX = centerX + viewWidth / 2d;
            double maxY = centerY + sysVarViewHeight / 2d;


            var lowerLeft = new Point2d(minX, minY);
            var upperRight = new Point2d(maxX, maxY);

            var zc = new ZoomCoordinates(lowerLeft, upperRight);
            return zc;
        }

        public static void RestoreZoomCoordinates(Document doc, ZoomCoordinates zoomCoordinates)
        {
            if (zoomCoordinates is not null)
            {
                var pt1 = new Point3d(zoomCoordinates.LowerLeft.X, zoomCoordinates.LowerLeft.Y, 0d);
                var pt2 = new Point3d(zoomCoordinates.UpperRight.X, zoomCoordinates.UpperRight.Y, 0d);
                Window(doc, pt1, pt2);
            }
        }
    }
}