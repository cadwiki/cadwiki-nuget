using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Forms.Integration;

namespace cadwiki.AC.PalleteSets
{
    public class PaletteSetHost
    {
        public static List<PaletteSetHost> OpenPalettes = new List<PaletteSetHost>();

        public ElementHost ElementHost;
        public string CurrentPaletteName { get; set; }
        public Autodesk.AutoCAD.Windows.PaletteSet AcadPaletteSet = null;
        public bool IsOpen { get; set; }
        public static List<string> Messages = new List<string>();
        public static List<Exception> Exceptions = new List<Exception>();

        public PaletteSetHost()
        {
            var test = "";
        }

        public class Options
        {
            public string Name { get; set; } = "";
            public string Title { get; set; } = "";
            public System.Windows.Controls.UserControl View { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public DockSides DockTo { get; set; } = DockSides.Left;
            public bool ResizeViewForPalette { get; set; }
            public bool UseElementHost { get; set; }
            public Guid Guid { get; set; } = Guid.Empty;
            public PaletteSetStyles Styles { get; set; }
        }

        public void CreatePaletteSet(Options opts)
        {
            if (OpenPalettes.Count >= 1)
            {
                var msg = "Warning: there is already " + OpenPalettes.Count + " palette(s) open";
                Messages.Add(msg);
            }

            if (opts.Guid == Guid.Empty)
            {
                AcadPaletteSet = new Autodesk.AutoCAD.Windows.PaletteSet("CUSTOM_PALETTE_NO_REOPEN", Guid.NewGuid());
            }
            else
            {
                if (AcadPaletteSet == null)
                {
                    AcadPaletteSet = new Autodesk.AutoCAD.Windows.PaletteSet("CUSTOM_PALETTE_NO_REOPEN", opts.Guid);
                }
            }

            CurrentPaletteName = opts.Title;

            AcadPaletteSet.Style = opts.Styles;

            var size = new System.Drawing.Size(opts.Width, opts.Height);

            AcadPaletteSet.MinimumSize = size;
            AcadPaletteSet.Size = size;


            if (opts.UseElementHost)
            {
                var hostSettings = new HostCreator.Settings();
                hostSettings.Width = opts.Width;
                hostSettings.Height = opts.Height;
                hostSettings.Control = opts.View;
                ElementHost = HostCreator.CreateHost(hostSettings);
                AcadPaletteSet.Add(opts.Title, ElementHost);
            }
            else
            {
                AcadPaletteSet.AddVisual(opts.Title, opts.View, opts.ResizeViewForPalette);
            }
            AcadPaletteSet.Text = opts.Title;

            AcadPaletteSet.RecalculateDockSiteLayout();

            AcadPaletteSet.Visible = true;
            if (opts.DockTo != DockSides.None)
            {
                AcadPaletteSet.Dock = opts.DockTo;
            }
            AcadPaletteSet.StateChanged += AcadPaletteSet_StateChanged;
            IsOpen = true;
            Messages.Add(opts.Title + " palette opened");
            OpenPalettes.Add(this);
        }

        private void AcadPaletteSet_StateChanged(object sender, PaletteSetStateEventArgs e)
        {
            if (e.NewState == Autodesk.AutoCAD.Windows.StateEventIndex.Hide)
            {
                IsOpen = false;
            }
            else if (e.NewState == Autodesk.AutoCAD.Windows.StateEventIndex.Show)
            {
                IsOpen = true;
            }
        }

        public void ReAdd(Options opts)
        {
            //this line ensures that new views don't show up as extra tabs on the palette
            AcadPaletteSet.Remove(0);
            var size = new System.Drawing.Size(opts.Width, opts.Height);
            if (opts.UseElementHost)
            {
                var hostSettings = new HostCreator.Settings();
                hostSettings.Width = opts.Width;
                hostSettings.Height = opts.Height;
                hostSettings.Control = opts.View;
                ElementHost = HostCreator.CreateHost(hostSettings);
                AcadPaletteSet.Add(opts.Title, ElementHost);
            }
            else
            {
                AcadPaletteSet.AddVisual(opts.Title, opts.View, opts.ResizeViewForPalette);
            }
            IsOpen = true;
        }

        public virtual void Close()
        {
            try
            {
                Messages.Add("Closing palette " + this.CurrentPaletteName);

                if (AcadPaletteSet != null && !AcadPaletteSet.IsDisposed)
                {
                    AcadPaletteSet.Visible = false;
                    AcadPaletteSet.Close();
                    AcadPaletteSet.Dispose();
                    IsOpen = false;
                    OpenPalettes.Remove(this);
                }
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
        }
        public void Hide()
        {
            try
            {
                if (AcadPaletteSet != null)
                    AcadPaletteSet.Visible = false;
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
        }
        public void Show()
        {
            try
            {
                if (AcadPaletteSet != null)
                    AcadPaletteSet.Visible = true;
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
        }

        /// <summary>
        /// Handles a command seperated input string from DockSides.ToString()
        /// Also handles a | seperated input string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static DockSides ParseDockSidesFromString(string input)
        {
            try
            {
                if (!string.IsNullOrEmpty(input))
                {
                    // Split the input string
                    var flags = input.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    DockSides result = DockSides.None;
                    foreach (var flag in flags)
                    {
                        if (Enum.TryParse(flag, true, out DockSides parsedFlag))
                        {
                            result |= parsedFlag;
                        }
                        else
                        {
                            Messages.Add($"Failed to parse DockSide enum from invalid flag value: {flag}");
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
            return DockSides.Right;
        }
    }

    public class HostCreator
    {
        private const UInt32 DLGC_WANTARROWS = 0x1;
        private const UInt32 DLGC_WANTTAB = 0x2;
        private const UInt32 DLGC_WANTALLKEYS = 0x4;
        private const UInt32 DLGC_HASSETSEL = 0x8;
        private const UInt32 DLGC_WANTCHARS = 0x80;
        private const UInt32 WM_GETDLGCODE = 0x87;

        public class Settings
        {
            public System.Windows.Controls.UserControl Control;
            public int Height = 0;
            public int Width = 0;
        }

        static IntPtr ChildHwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WM_GETDLGCODE) return IntPtr.Zero;
            handled = true;
            return new IntPtr(DLGC_WANTCHARS | DLGC_WANTARROWS | DLGC_HASSETSEL);
        }

        public static ElementHost CreateHost(Settings settings)
        {
            //create host for user settings.Control
            ElementHost host = new ElementHost();

            //enable settings.Control
            settings.Control.IsEnabled = true;
            settings.Control.Focusable = true;
            settings.Control.Focus();
            settings.Control.RenderSize = new System.Windows.Size(settings.Width, settings.Height);
            //add child to host and enable
            host.Child = settings.Control;

            host.Enabled = true;
            host.Size = new System.Drawing.Size(settings.Width, settings.Height);

            //var tempForm = new Form();
            //tempForm.Size = new System.Drawing.Size(settings.Width, settings.Height);
            //tempForm.Controls.Add(host);

            //// Retrieve the Visual of the host
            //var visualHost =  PresentationSource.FromVisual(host) as Visual;

            //var s = PresentationSource.FromVisual(settings.Control) as HwndSource;
            //s?.AddHook(ChildHwndSourceHook);

            return host;
        }

    }
}
