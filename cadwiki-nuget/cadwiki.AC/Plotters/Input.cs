using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace cadwiki.AC.Plotters
{
    public class Input
    {
        public static List<String> DebugLogs = new List<String>();
        public static List<Exception> Exceptions = new List<Exception>();
        public static List<String> ProgressAlerts = new List<String>();

        public string LayoutName = PlotterDefaults.LayoutName;
        public string PlotDeviceName = PlotterDefaults.PlotDeviceName;
        public string MediaName = PlotterDefaults.MediaName;
        public string SheetStyleName = PlotterDefaults.SheetStyleName;
        public string OutputFilePath = PlotterDefaults.OutputFilePath;
        public PlotType PlotArea = PlotterDefaults.PlotArea;
        public Point2d PlotWindowStart = PlotterDefaults.PlotWindowStart;
        public Point2d PlotWindowEnd = PlotterDefaults.PlotWindowEnd;
        public StdScaleType PlotScale = PlotterDefaults.PlotScale;
        public PlotRotation PlotRotation = PlotterDefaults.PlotRotation;
        public bool ScaleLineWeights = PlotterDefaults.ScaleLineWeights;
        public bool PrintLineWeights = PlotterDefaults.PrintLineWeights;
        public bool PlotPlotStyles = PlotterDefaults.PlotPlotStyles;

        public string ToJson()
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
                return jsonString;
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
            return "";
        }
    }
}
