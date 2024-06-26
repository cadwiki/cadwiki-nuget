using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace cadwiki.AutoCAD2021.Base.Utilities
{

    public class Mtexts
    {
        public class Settings
        {
            public Point3d Location;
            public double Height;
            public string Content;
        }

        public static MText Add(Settings settings)
        {
            // Get the current document and database
            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            // Start a transaction
            using (var trans = db.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);

                // Open the Model Space block table record for write
                BlockTableRecord ms = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], global::Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite);

                // Create a new text entity
                var mtext = new MText();

                mtext.Location = settings.Location;
                mtext.Height = settings.Height;
                mtext.Contents = settings.Content;

                // Add the text entity to the Model Space block table record
                ms.AppendEntity(mtext);
                trans.AddNewlyCreatedDBObject(mtext, true);

                // Commit the transaction
                trans.Commit();

                return mtext;
            }
            return null;

        }
    }
}