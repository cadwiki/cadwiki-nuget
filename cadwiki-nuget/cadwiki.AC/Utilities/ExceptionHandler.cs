using System;

namespace cadwiki.AC
{

    public class ExceptionHandler
    {
        public static void WriteToEditor(Exception ex)
        {
            var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var list = NetUtils.Exceptions.GetPrettyStringList(ex);
            foreach (string str in list)
                ed.WriteMessage(Environment.NewLine.ToString() + str);
        }



    }
}