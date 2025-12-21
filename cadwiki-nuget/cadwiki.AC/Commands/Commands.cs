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
using cadwiki.NUnitTestRunner.Creators;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

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

        [CommandMethod("TakeSnapshot")]
        public void TakeSnapshot()
        {
            try
            {
                var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                var time = DateTime.Now;
                var format = "yyyyMMdd--HH_mm_ss";
                var timeStamp = time.ToString(format);
                var input = new cadwiki.AC.Plotters.Input();
                input.LayoutName = "Model";
                input.OutputFilePath = TestEvidenceCreator.LocalScreenShotCache + "\\" + timeStamp + ".pdf";
                var output = cadwiki.AC.Plotters.PlotterSinglePage.PlotSingleLayoutToSinglePagePDF(doc, doc.Database, input);
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.WriteToEditor(ex);
            }
        }

    }
}
