using cadwiki.MVVM.ViewModels;
using cadwiki.MVVM.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cadwiki.AC;
using cadwiki.AC.Views;
using cadwiki.AC.ViewModels;

namespace cadwiki.AC.PalleteSets
{
    public class PaletteSetSample : PaletteSetJson
    {
        public PaletteSetSample(ViewSample view, ViewModels.ViewModelSample viewModel)
        {
            View = view;
            ViewModel = viewModel;
            CreateAndShow(viewModel.DisplayName);
        }
    }
}
