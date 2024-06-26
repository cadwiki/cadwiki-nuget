using System;
using System.Windows;

namespace cadwiki.WpfUi.Templates
{
    public partial class WindowAutoCADException : CustomWindowTemplate
    {
        public bool WasOkayClicked;

        public WindowAutoCADException()
        {
            this.InitializeComponent();
        }

        public WindowAutoCADException(Exception ex)
        {
            this.InitializeComponent();
            this.TextBoxMessage.Text = ex.Message;
            this.TextBoxStackTrace.Text = ex.StackTrace;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            WasOkayClicked = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}