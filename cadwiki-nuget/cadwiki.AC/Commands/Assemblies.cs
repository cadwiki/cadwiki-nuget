using Autodesk.AutoCAD.Runtime;
using cadwiki.MVVM.ViewModels;
using cadwiki.MVVM.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using cadwiki.AC.Utilities;

[assembly: CommandClass(typeof(cadwiki.AC.Commands.Assemblies))]


namespace cadwiki.AC.Commands
{
    public class Assemblies
    {
        [CommandMethod("Dlls")]
        public void Dlls()
        {
            try
            {
                ShowDLLView();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.WriteToEditor(ex);
            }

        }

        public void ShowDLLView()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic);
            var dllView = new DLLAutoCADView();
            var dllViewModel = new DLLAutoCADViewModel();
            try
            {
                dllViewModel.LoadedAssemblies = assemblies.ToList();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.WriteToEditor(ex);
            }

            dllView.DataContext = dllViewModel;

            var window = new System.Windows.Window();
            window.Content = dllView;
            window.Height = 500;
            window.MinHeight = 500;
            window.MaxHeight = 500;

            window.Width = 1200;
            window.MinWidth = 1200;
            window.MaxWidth = 1800;

            //dllView.LoadedAssembliesDataGrid.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Visible;

            window.Topmost = false;
            window.Show();

        }
    }

}
