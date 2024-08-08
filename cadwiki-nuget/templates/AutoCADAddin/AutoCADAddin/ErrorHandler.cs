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
    }
}