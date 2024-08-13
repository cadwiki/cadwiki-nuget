using System;

namespace AutoCADAddin
{
    public class ErrorHandler
    {
        public static void Show(Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(
                ex.Message, 
                "Error", 
                System.Windows.Forms.MessageBoxButtons.OK, 
                System.Windows.Forms.MessageBoxIcon.Error);  
        }

        public static void Ed(Exception ex)
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var msg = "";
            msg = "Message: " + ex.Message;
            msg += Environment.NewLine + "Stack Trace: " + ex.StackTrace;
            doc.Editor.WriteMessage(Environment.NewLine + msg);
        }
    }
}