using System;
using System.Windows;
using System.Windows.Media;

// https://www.codeproject.com/Tips/1155345/How-to-Remove-the-Close-Button-from-a-WPF-ToolWind
namespace cadwiki.WpfUi.Templates
{
    public partial class CustomWindowTemplate : Window
    {

        public SolidColorBrush _solidColorBrushLimeGreen { get; set; } = new SolidColorBrush();

        public SolidColorBrush SolidColorBrushLimeGreen
        {
            get
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));
            }
            set
            {
                _solidColorBrushLimeGreen = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));
            }
        }


        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public CustomWindowTemplate()
        {
            // https://stackoverflow.com/questions/25468920/converting-c-sharp-to-vb-trouble-with-events-and-lambda-expression
            Loaded += (s, e) => ToolWindow_Loaded(s, e);
            DataContext = this;
        }

        private void ToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            CustomWindowTemplate.SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }

}