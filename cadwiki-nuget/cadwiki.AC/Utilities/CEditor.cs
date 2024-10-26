using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace cadwiki.AC.Utilities
{
    public class CEditor
    {
        private void Write(string msg)
        {
            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                msg = msg.StartsWith(Environment.NewLine) ? msg : Environment.NewLine + msg;
                doc.Editor.WriteMessage(msg);
            }
        }
    }
}
