using Autodesk.AutoCAD.Windows.Features.PointCloud.PointCloudColorMapping;
using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using cadwiki.MVVM.ViewModels;

namespace cadwiki.AC.PalleteSets
{
    public class PaletteSetJson :  PaletteSetHost
    {
        public UserControl View { get; set; } = new UserControl();
        public PaletteSetViewModel ViewModel { get; set; } = new PaletteSetViewModel();
        public Guid PaletteSetGuid = new Guid();

        public PaletteSetJson()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
        }

        public virtual void GetNewView()
        {
            View = new UserControl();
            View.DataContext = ViewModel;
        }

        public virtual void LoadPresetFromJsonIntoViewModel()
        {
            ViewModel.LoadFirstFoundPresetFileIntoViewModel<PaletteSetViewModel>();
        }

        public virtual Options GetDefaultOpts(string title)
        {
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


        public void Show(string title)
        {
            try
            {
                GetNewView();
                Options opts = GetDefaultOpts(title);
                LoadPresetFromJsonIntoViewModel();
                LoadViewModelIntoOptions(opts);
                CreatePaletteSet(opts);
                this.AcadPaletteSet.SizeChanged += AcadPaletteSet_SizeChanged;
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
        }

        private void LoadViewModelIntoOptions(Options opts)
        {
            opts.Width = ViewModel.PaletteWidth;
            opts.Height = ViewModel.PaletteHeight;
            opts.DockTo = ParseDockSidesFromString(ViewModel.PaletteDock);
        }

        private void AcadPaletteSet_SizeChanged(object sender, PaletteSetSizeEventArgs e)
        {
            try
            {
                GetNewView();
                Options opts = GetDefaultOpts(this.CurrentPaletteName);
                opts.Width = e.Width;
                opts.Height = e.Height;
                ReAdd(opts);
                ViewModel.PaletteHeight = e.Height;
                ViewModel.PaletteWidth = e.Width;
                ViewModel.PaletteDock = this.AcadPaletteSet.DockEnabled.ToString();
                ViewModel.SaveViewModelToFirstJson();
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
        }



    }
}
