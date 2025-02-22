using Autodesk.AutoCAD.Runtime;
using cadwiki.AC.PalleteSets;
using cadwiki.AC.Utilities;
using cadwiki.MVVM.ViewModels;
using cadwiki.AC.Views;
using cadwiki.AC.ViewModels;
using cadwiki.MVVM.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cadwiki.AC.Commands
{
    public class Commands
    {
        [CommandMethod("SamplePallete")]
        public void SamplePallete()
        {
            try
            {
                ShowSamplePallete();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.WriteToEditor(ex);
            }
        }


        public void ShowSamplePallete()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic);
            var view = new ViewSample();
            var viewModel = new ViewModelSample();

            view.DataContext = viewModel;

            var palette = new PaletteSetSample(view, viewModel);

        }
    }
}
