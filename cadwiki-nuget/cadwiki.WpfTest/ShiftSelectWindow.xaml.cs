using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace cadwiki.WpfTest
{
    /// <summary>
    /// Interaction logic for ShiftSelectWindow.xaml
    /// </summary>
    public partial class ShiftSelectWindow : Window
    {
        public ShiftSelectWindow()
        {
            InitializeComponent();
            this.testView.DataContext = new ShiftSelectDataGrid.ShiftSelectDataGridViewModel();
        }
    }
}
