using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace cadwiki.AC.ViewModels
{
    public class ViewModelSample : PalleteSets.PaletteSetViewModel
    {
        public ViewModelSample()
        {
            Title = "Sample";
            FileNamePrefix = "Sample";
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
