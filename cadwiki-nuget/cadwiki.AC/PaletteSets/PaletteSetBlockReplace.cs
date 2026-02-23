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
    public class PaletteSetBlockReplace : PaletteSetJson
    {
        public PaletteSetBlockReplace(BlockReplaceView view, BlockReplaceViewModel viewModel)
        {
            View = view;
            ViewModel = viewModel;
            Show(viewModel.DisplayName);
        }

        public override void GetNewView()
        {
            View = new BlockReplaceView();
            View.DataContext = ViewModel;
        }

        public override void LoadPresetFromJsonIntoViewModel()
        {
            ViewModel.LoadFirstFoundPresetFileIntoViewModel<BlockReplaceViewModel>();
        }

        public override Options GetDefaultOpts(string title)
        {
            PaletteSetGuid = new Guid("3a7f8c2d-1b4e-4f9a-8d3c-5e6b7a8f9c0d");
            Options opts = new Options();
            opts.Name = "BLOCK_REPLACE_PALETTE";
            opts.Title = title;
            opts.UseElementHost = true;
            opts.View = View;
            opts.Height = 700;
            opts.Width = 480;
            opts.DockTo = DockSides.Right;
            opts.ResizeViewForPalette = true;
            opts.Guid = PaletteSetGuid;
            opts.Styles = PaletteSetStyles.ShowCloseButton | PaletteSetStyles.Snappable | PaletteSetStyles.NameEditable | PaletteSetStyles.UsePaletteNameAsTitleForSingle;
            return opts;
        }
    }
}