using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;

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
            global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute(str + Environment.NewLine, true, false, false);
        }

        public static void SendLispCommandEndUndoMark()
        {
            string str = "(vla-endundomark (vla-get-ActiveDocument (vlax-get-acad-object)))";
            global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute(str + Environment.NewLine, true, false, false);
        }

        public static void SendLispCommandUndoBack()
        {
            string str = "(command-s \"._undo\" \"back\" \"yes\")";
            global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute(str + Environment.NewLine, true, false, false);
        }
    }
}