using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Interop;
using Microsoft.VisualBasic;
using System.Threading.Tasks;

namespace cadwiki.AutoCAD2021.Base.Utilities
{

    public class Commands
    {

        public static void ExecuteInApplicationContext(List<object> parameters)
        {
            var dm = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager;
            var doc = dm.CurrentDocument;
            dm.ExecuteInApplicationContext(obj => doc.Editor.Command(parameters.ToArray()), null);
        }

        public static void SendLispCommandStartUndoMark()
        {
            string str = "(vla-startundomark (vla-get-ActiveDocument (vlax-get-acad-object)))";
            global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute(str + "\n", true, false, false);
        }

        public static void SendLispCommandEndUndoMark()
        {
            string str = "(vla-endundomark (vla-get-ActiveDocument (vlax-get-acad-object)))";
            global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute(str + "\n", true, false, false);
        }

        public static void SendLispCommandUndoBack()
        {
            string str = "(command-s \"._undo\" \"back\" \"yes\")";
            global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute(str + "\n", true, false, false);
        }

        public static void SendCommand(string command)
        {
            var acadApp = (AcadApplication)Application.AcadApplication;
            var thisDrawing = (AcadDocument)acadApp.ActiveDocument;
            thisDrawing.SendCommand(command);
        }

        public static async Task ExecuteInCommandContextAsync(Document doc, object[] parameters)
        {
            await global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.ExecuteInCommandContextAsync(
                async (obj) =>
                {
                    await doc.Editor.CommandAsync(parameters);
                },
                null);
        }


        public static async Task RefreshRibbon(Document doc)
        {
            var refreshRibbonCommandLineArgs = new object[] { "RIBBON" };

            var myTask = Task.Run(() => ExecuteInCommandContextAsync(doc, refreshRibbonCommandLineArgs));
            await myTask;
            return;
        }

    }
}