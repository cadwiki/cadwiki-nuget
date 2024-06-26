
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace cadwiki.WpfUi
{

    public class Utils
    {
        public static BrushConverter Converter = new BrushConverter();
        public static readonly Brush Green = (Brush)Converter.ConvertFromString("#00FF00");
        public static readonly Brush Red = (Brush)Converter.ConvertFromString("#FF0000");
        public static readonly Brush Normal = SystemColors.WindowBrush;
        public static readonly Brush Yellow = (Brush)Converter.ConvertFromString("#FFFF00");


        public static void SetNormalStatus(TextBlock tbs, TextBlock tbm, string statusMessage, string message)
        {
            tbs.Text = statusMessage;
            tbs.Background = Normal;
            tbm.Text = message;
        }

        public static void SetSuccessStatus(TextBlock tbs, TextBlock tbm, string message)
        {
            tbs.Text = "Success";
            tbs.Background = Green;
            tbm.Text = message;
        }

        public static void SetProcessingStatus(TextBlock tbs, TextBlock tbm, string message)
        {
            tbs.Text = "Processing...";
            tbs.Background = Yellow;
            tbm.Text = message;
        }

        public static void SetYellowStatus(TextBlock tbs, TextBlock tbm, string statusMessage, string message)
        {
            tbs.Text = statusMessage;
            tbs.Background = Yellow;
            tbm.Text = message;
        }

        public static void SetErrorStatus(TextBlock tbs, TextBlock tbm, string message)
        {
            tbs.Text = "Error...";
            tbs.Background = Red;
            tbm.Text = message;
        }
    }
}