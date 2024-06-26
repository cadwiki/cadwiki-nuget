
namespace cadwiki.WpfUi
{

    public partial class WindowCadWiki
    {

        public bool WasOkayClicked;

        public WindowCadWiki()
        {
            // This call is required by the designer.
            this.InitializeComponent();
        }

        public WindowCadWiki(string inputValueFromBusinessLogic)
        {
            this.InitializeComponent();
            this.TextBoxDisplay.Text = inputValueFromBusinessLogic;
        }

        private void ButtonOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WasOkayClicked = true;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

    }
}