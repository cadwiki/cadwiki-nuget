using Autodesk.AutoCAD.Windows.Features.PointCloud.PointCloudColorMapping;
using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace cadwiki.AC.PalleteSets
{
    public class PaletteSetJson :  PaletteSetHost
    {
        public static UserControl View { get; set; } = new UserControl();
        public static PaletteSetViewModel ViewModel { get; set; } = new PaletteSetViewModel();
        public static Guid PaletteSetGuid = new Guid();

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


        public void CreateAndShow(string title)
        {
            try
            {
                GetNewView();
                //new view must be instantiated before setting default opts, since it relies on view
                Options opts = GetDefaultOpts(title);
                LoadPreset();
                MergeDefaultWithVm(opts, title);

                this.AcadPaletteSet = new Autodesk.AutoCAD.Windows.PaletteSet(opts.Name, opts.Guid);
                this.AcadPaletteSet.Style = opts.Styles;
                // Create the PaletteSet and Show it
                TrySetPaletteConfigFromViewModel(opts);
                CreatePaletteSet(opts);
                this.AcadPaletteSet.SizeChanged += AcadPaletteSet_SizeChanged;
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
        }

        private static void GetNewView()
        {
            View = new UserControl();
            View.DataContext = ViewModel;
        }

        private static void LoadPreset()
        {
            ViewModel.LoadFirstFoundPresetFileIntoViewModel<PaletteSetViewModel>();
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
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
        }

        public Options GetDefaultOpts(string title)
        {
            Options opts = new Options();
            opts.Name = "CUSTOM_PALETTE_NO_REOPEN";
            opts.Title = title;
            opts.UseElementHost = true;
            opts.View = View;
            opts.Height = 800;
            opts.Width = 700;
            opts.DockTo = DockSides.Left;
            opts.ResizeViewForPalette = true;
            opts.Guid = PaletteSetGuid;
            opts.Styles = PaletteSetStyles.Snappable | PaletteSetStyles.NameEditable | PaletteSetStyles.UsePaletteNameAsTitleForSingle;
            return opts;
        }

        private void MergeDefaultWithVm(Options defaultOpts, string title)
        {
            if (string.IsNullOrEmpty(ViewModel.PaletteDock))
            {
                ViewModel.PaletteDock = defaultOpts.DockTo.ToString();
            }
            if (ViewModel.PaletteHeight == 0)
            {
                ViewModel.PaletteHeight = defaultOpts.Height;
            }
            if (ViewModel.PaletteWidth == 0)
            {
                ViewModel.PaletteWidth = defaultOpts.Width;
            }
        }

        private void TrySetPaletteConfigFromViewModel(Options opts)
        {
            try
            {
                if (ViewModel.PaletteHeight > 0 && ViewModel.PaletteWidth > 0)
                {
                    var size = new System.Drawing.Size(ViewModel.PaletteWidth, ViewModel.PaletteHeight);
                    this.AcadPaletteSet.Size = size;
                }
                var dockSide = ParseDockSidesFromString(ViewModel.PaletteDock);
                this.AcadPaletteSet.Dock = dockSide;

                opts.DockTo = this.AcadPaletteSet.DockEnabled;
                opts.Height = this.AcadPaletteSet.Size.Height;
                opts.Width = this.AcadPaletteSet.Size.Width;
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
        }
    }
}
