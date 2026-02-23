using Autodesk.AutoCAD.Runtime;
using cadwiki.AC.PalleteSets;
using cadwiki.AC.Views;
using cadwiki.AC.ViewModels;
using cadwiki.AC.Utilities;
using System;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;

namespace cadwiki.AC.Commands
{
    public class BlockReplaceCommands
    {
        [CommandMethod("BlockReplacer")]
        public void BlockReplacer()
        {
            try
            {
                ShowBlockReplacerPalette();
            }
            catch (System.Exception ex)
            {
                Utilities.ExceptionHandler.WriteToEditor(ex);
            }
        }

        public void ShowBlockReplacerPalette()
        {
            var view = new BlockReplaceView();
            var viewModel = new BlockReplaceViewModel();

            view.DataContext = viewModel;

            var palette = new PaletteSetBlockReplace(view, viewModel);
        }


    }
}