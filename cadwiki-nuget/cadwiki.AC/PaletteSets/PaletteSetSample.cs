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
using Autodesk.AutoCAD.Windows;

namespace cadwiki.AC.PalleteSets
{
    public class PaletteSetSample : PaletteSetJson
    {
        public PaletteSetSample(ViewSample view, ViewModels.ViewModelSample viewModel)
        {
            View = view;
            ViewModel = viewModel;
            Show(viewModel.DisplayName);
        }

        public override void GetNewView()
        {
            View = new ViewSample();
            View.DataContext = ViewModel;
        }

        public override void LoadPresetFromJsonIntoViewModel()
        {
            ViewModel.LoadFirstFoundPresetFileIntoViewModel<ViewModelSample>();
        }

        public override Options GetDefaultOpts(string title)
        {
            PaletteSetGuid = new Guid("22598b1a-7ad0-4442-ae9e-a794c785c7ec");
            Options opts = new Options();
            opts.Name = "CUSTOM_PALETTE_NO_REOPEN";
            opts.Title = title;
            opts.UseElementHost = true;
            opts.View = View;
            opts.Height = 800;
            opts.Width = 400;
            opts.DockTo = DockSides.Left;
            opts.ResizeViewForPalette = true;
            opts.Guid = PaletteSetGuid;
            opts.Styles = PaletteSetStyles.ShowCloseButton | PaletteSetStyles.Snappable | PaletteSetStyles.NameEditable | PaletteSetStyles.UsePaletteNameAsTitleForSingle;
            return opts;
        }
    }
}
