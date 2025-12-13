using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.Interop.Common;
using System;
using System.Collections.Generic;

namespace cadwiki.AC.DocProcessor
{

    public class DocProcessor : DocProcessorBase
    {

        public static void CodeSample()
        {
            var processor = new DocProcessor();
            var drawings = new List<string>();
            DocumentCollection dm = Application.DocumentManager;
            var activeDoc = dm.MdiActiveDocument;
            var openAutoCADFilePath = activeDoc?.Database.Filename;

            foreach (var drawing in drawings)
            {
                try
                {
                    var res = processor.Open(drawing);
                    if (OpenResultChecker.WasOpened(res))
                    {
                        var newlyOpenedDocument = dm.MdiActiveDocument;
                        //add logic here
                        processor.Save(drawing);
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.Add(ex);
                }
            }
        }
    }

    public abstract partial class DocProcessorBase
    {
        public static List<String> DebugLogs = new List<String>();
        public static List<Exception> Exceptions = new List<Exception>();
        public static List<String> ProgressAlerts = new List<String>();


        public DwgVersion SaveToVersion = DwgVersion.Current;
        public bool ClosePreviousOnNewOpen = true;
        public bool SavePreviousOnNewOpen = true;




        public OpenResult Open(string file)
        {
            try
            {
                var sdiMode = SdiModeChecker.GetSdiMode();
                if (sdiMode == SdiMode.SingleDocument)
                {
                    return OpenFromSdi(file);
                }
                return OpenFromMdi(file);
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
                OnException(ex);
                return OpenResult.Failed;
            }
        }

        public SaveResult Save(string drawing)
        {
            try
            {
                var sdiMode = SdiModeChecker.GetSdiMode();
                if (sdiMode == SdiMode.SingleDocument)
                {
                    return SaveFromSdi();
                }
                return SaveFromMdi();
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
                OnException(ex);
                return SaveResult.Error;
            }
        }

        public virtual void OnException(Exception ex1)
        {
            try
            {
                Exceptions.Add(ex1);
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(
                    $"\nError processing: {ex1.Message}");
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
        }

        private OpenResult OpenFromMdi(string file)
        {
            try
            {
                var isDocAlreadyOpen = ToggleToAlreadyOpenFromMdi(file);
                if (isDocAlreadyOpen == OpenResult.AlreadyOpen)
                {
                    return isDocAlreadyOpen;
                }
                return OpenClosedDocumentFromMdi(file);
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
                OnException(ex);
                return OpenResult.Failed;
            }
        }

        private OpenResult OpenClosedDocumentFromMdi(string file)
        {
            try
            {
                DocumentCollection dm = Application.DocumentManager;
                var previous = dm.MdiActiveDocument;
                dm.AppContextOpenDocument(file);
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (ClosePreviousOnNewOpen && SavePreviousOnNewOpen)
                {
                    previous.CloseAndSave(previous.Name);
                }
                else if (ClosePreviousOnNewOpen)
                {
                    previous.CloseAndDiscard();
                }
                return OpenResult.Opened;
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
                OnException(ex);
                return OpenResult.Failed;
            }
        }

        private OpenResult ToggleToAlreadyOpenFromMdi(string file)
        {
            try
            {
                DocumentCollection dm = Application.DocumentManager;
                foreach (Document doc in dm)
                {
                    var drawingDatabasePath = doc?.Database.Filename;
                    if (file.ToLower().Equals(drawingDatabasePath.ToLower()))
                    {
                        dm.MdiActiveDocument = doc;
                        return OpenResult.AlreadyOpen;
                    }
                }
                return OpenResult.NotFound;
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
                OnException(ex);
                return OpenResult.Failed;
            }
        }


        private SaveResult SaveFromMdi()
        {
            try
            {
                DocumentCollection dm = Application.DocumentManager;
                var doc = dm.MdiActiveDocument;
                doc.Database.SaveAs(doc.Name, true,
                    SaveToVersion, doc.Database.SecurityParameters);
                return SaveResult.Saved;
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
                OnException(ex);
                return SaveResult.Error;
            }
        }

        private OpenResult OpenFromSdi(string file)
        {
            try
            {

                //already open document
                DocumentCollection dm = Application.DocumentManager;
                foreach (Document doc in dm)
                {
                    var drawingDatabasePath = doc?.Database.Filename;
                    if (file.ToLower().Equals(drawingDatabasePath.ToLower()))
                    {
                        dm.MdiActiveDocument = doc;
                        return OpenResult.AlreadyOpen;
                    }
                }
                var previous = dm.MdiActiveDocument;
                AcadApplication acadApp = (AcadApplication)Application.AcadApplication;
                AcadDocument newDoc = acadApp.Documents.Open(file, false);
                return OpenResult.Opened;
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
                OnException(ex);
                return OpenResult.Failed;
            }
        }

        private SaveResult SaveFromSdi()
        {
            try
            {
                DocumentCollection dm = Application.DocumentManager;
                var doc = dm.CurrentDocument;
                AcSaveAsType saveAsType = AcSaveAsType.ac2018_dwg;
                AcadDocument newDoc = (AcadDocument)doc.GetAcadDocument();
                newDoc.SaveAs(doc.Database.Filename, saveAsType);
                return SaveResult.Saved;
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
                OnException(ex);
                return SaveResult.Error;
            }
        }

    }
}
