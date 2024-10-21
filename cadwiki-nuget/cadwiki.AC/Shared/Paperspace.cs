using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Microsoft.VisualBasic.CompilerServices;

namespace cadwiki.AC
{

    public class Paperspace
    {
        public static ObjectId CreateViewPort(Document doc)
        {
            var db = doc.Database;
            using (var @lock = doc.LockDocument())
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)t.GetObject(db.BlockTableId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                    BlockTableRecord rec = (BlockTableRecord)t.GetObject(bt[BlockTableRecord.PaperSpace], global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);

                    global::Autodesk.AutoCAD.ApplicationServices.Core.Application.SetSystemVariable("TILEMODE", 0);
                    doc.Editor.SwitchToPaperSpace();

                    var vp = new Viewport();
                    vp.SetDatabaseDefaults();
                    vp.CenterPoint = new Point3d(3.25d, 3d, 0d);
                    vp.Width = 6d;
                    vp.Height = 5d;

                    var objectId = rec.AppendEntity(vp);
                    t.AddNewlyCreatedDBObject(vp, true);
                    vp.On = true;

                    t.Commit();
                    return objectId;
                }
            }


            return default;
        }


        private static void ZoomCurrentViewPort(Document doc, Point3d lowerLeft, Point3d upperRight)
        {
            int cvId = Convert.ToInt32(global::Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("CVPORT"));
            using (var gm = doc.GraphicsManager)
            {
                using (var vw = gm.GetCurrentAcGsView(cvId))
                {
                    string currentTab = Conversions.ToString(global::Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("CTAB"));
                    Layouts.SetCurrentTab(doc, "Model");
                    vw.ZoomExtents(lowerLeft, upperRight);
                    vw.Zoom(0.95d);
                    // SetCurrentLayoutTab(currentTab)
                    // gm.SetViewportFromView(cvId, vw, True, True, False)
                }
            }
        }
    }
}