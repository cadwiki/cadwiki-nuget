using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cadwiki.AC.Plotters
{
    public class PlotterDefaults
    {
        public static readonly string LayoutName = "Model";
        public static readonly string PlotDeviceName = "DWG To PDF.pc3";
        public static readonly string MediaName = "ANSI_D_(34.00_x_22.00_Inches)";
        public static readonly string SheetStyleName = "monochrome.ctb";
        public static readonly string OutputFilePath = "C:\\idc\\output.pdf";
        public static readonly PlotType PlotArea = PlotType.Extents;
        public static readonly Point2d PlotWindowStart = new Point2d();
        public static readonly Point2d PlotWindowEnd = new Point2d();
        public static readonly StdScaleType PlotScale = StdScaleType.ScaleToFit;
        public static readonly PlotRotation PlotRotation = PlotRotation.Degrees000;
        public static readonly bool ScaleLineWeights = true;
        public static readonly bool PrintLineWeights = true;
        public static readonly bool PlotPlotStyles = true;


        public static readonly string MediaName34x22 = "ANSI_D_(34.00_x_22.00_Inches)";
        public static readonly string MediaName11x17 = "ANSI_full_bleed_B_(17.00_x_11.00_Inches)";
    }
}
