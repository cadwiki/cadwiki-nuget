using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType;

namespace cadwiki.AC.Plotters
{
    public class PlotterMultiPage
    {
        public static List<String> DebugLogs = new List<String>();
        public static List<Exception> Exceptions = new List<Exception>();
        public static List<String> ProgressAlerts = new List<String>();

        /// <summary>
        /// Plots all non modelspace layouts in current drawing
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static PlotReturn PlotDrawingToMultiPagePdf(Document doc, Database db, Input input, bool closeCurrentDrawingAfterPlot = false)
        {
            var bgPlot = 0;
            var plotReturn = new PlotReturn();
            try
            {
                var startingLayout = LayoutManager.Current.CurrentLayout;
                using (var lk = doc.LockDocument())
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        bgPlot = (short)Application.GetSystemVariable("BACKGROUNDPLOT");
                        Application.SetSystemVariable("BACKGROUNDPLOT", 0);
                        BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                        if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
                        {
                            using (PlotEngine pe = PlotFactory.CreatePublishEngine())
                            {
                                ObjectIdCollection layoutsToPlot = new ObjectIdCollection();
                                foreach (ObjectId btrId in bt)
                                {
                                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
                                    if (btr.IsLayout && btr.Name.ToUpper() != BlockTableRecord.ModelSpace.ToUpper())
                                    {
                                        layoutsToPlot.Add(btrId);
                                    }
                                }
                                var layouts = new List<Layout>();
                                foreach (ObjectId btrId in layoutsToPlot)
                                {
                                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
                                    Layout lo = (Layout)tr.GetObject(btr.LayoutId, OpenMode.ForRead);
                                    layouts.Add(lo);
                                }
                                layouts = layouts.OrderBy(l => l.LayoutName).ToList();

                                int numSheet = 1;
                                var sheetProgress = $"Layout Progress - {numSheet} of {layoutsToPlot.Count}";

                                var statusText = $"Plotting {System.IO.Path.GetFileName(doc.Name)} with {layoutsToPlot.Count} paperspace layouts(s)";
                                Autodesk.AutoCAD.Geometry.Point2d firstPaperSize = new Autodesk.AutoCAD.Geometry.Point2d();
                                Layout firstLayout = null;
                                int i = 0;
                                foreach (var layout in layouts)
                                {
                                    Layout lo = (Layout)tr.GetObject(layout.Id, OpenMode.ForRead);

                                    if (i == 0)
                                    {
                                        firstPaperSize = lo.PlotPaperSize;
                                        firstLayout = layout;
                                    }

                                    PlotSettings ps = new PlotSettings(lo.ModelType);
                                    //https://www.keanw.com/2007/09/driving-a-multi.html

                                    //Error in the AutoCAD API.
                                    //plotting with multiple layouts with different paper format sizes, results in error
                                    //pe.BeginPage(ppi, pi, (numSheet == layoutsToPlot.Count), null);
                                    //named eInvalidPlotinfo

                                    //The root is the function pe.BeginDocument which uses the PlotInfo Object from the first Layout
                                    //pe.BeginDocument(pi, doc.Name, null, 1, true, Exportpath + NewFileNameWithoutExtension);


                                    ///workaround for multi plots with layouts of differing sizes
                                    if (i == 0 || firstPaperSize == lo.PlotPaperSize)
                                    {
                                        ps.CopyFrom(lo);
                                    }
                                    ///fall back to original layout size
                                    else
                                    {
                                        ps.CopyFrom(firstLayout);
                                    }

                                    i++;

                                    ps.ScaleLineweights = input.ScaleLineWeights;
                                    ps.PrintLineweights = input.PrintLineWeights;
                                    ps.PlotPlotStyles = input.PlotPlotStyles;

                                    PlotSettingsValidator psv = PlotSettingsValidator.Current;
                                    //set default first, these settings will be overridden below
                                    psv.SetDefaultPlotConfig(ps);

                                    if (input.PlotArea == PlotType.Window)
                                    {
                                        try
                                        {
                                            var window = new Extents2d(input.PlotWindowStart, input.PlotWindowEnd);
                                            psv.SetPlotWindowArea(ps, window);
                                            psv.SetPlotType(ps, input.PlotArea);
                                        }
                                        catch (Exception ex)
                                        {
                                            Exceptions.Add(ex);
                                            DebugLogs.Add("Failed to set plot window, falling back to extents");
                                            psv.SetPlotType(ps, PlotType.Extents);
                                            plotReturn.ErrorMessage += ex.Message + Environment.NewLine;
                                            plotReturn.Exceptions.Add(ex);
                                        }
                                    }
                                    else
                                    {
                                        psv.SetPlotType(ps, input.PlotArea);
                                    }
                                    psv.SetUseStandardScale(ps, true);
                                    psv.SetStdScaleType(ps, input.PlotScale);
                                    psv.SetPlotCentered(ps, true);
                                    psv.SetPlotConfigurationName(ps, input.PlotDeviceName, input.MediaName);
                                    try
                                    {
                                        psv.SetCurrentStyleSheet(ps, input.SheetStyleName);
                                    }
                                    catch (Exception ex)
                                    {
                                        Exceptions.Add(ex);
                                        DebugLogs.Add("Falling back to default ctb");
                                        psv.SetCurrentStyleSheet(ps, "monochrome.ctb");
                                        plotReturn.ErrorMessage += ex.Message + Environment.NewLine;
                                        plotReturn.Exceptions.Add(ex);
                                    }

                                    psv.SetPlotRotation(ps, input.PlotRotation);

                                    PlotInfo pi = new PlotInfo();
                                    pi.Layout = lo.ObjectId;
                                    if (LayoutManager.Current.CurrentLayout != lo.LayoutName && !lo.IsErased)
                                    {
                                        try
                                        {
                                            LayoutManager.Current.SetCurrentLayoutId(lo.Id);
                                        }
                                        catch (Exception ex)
                                        {
                                            Exceptions.Add(ex);
                                            numSheet++;
                                            plotReturn.ErrorMessage += ex.Message + Environment.NewLine;
                                            plotReturn.Exceptions.Add(ex);
                                            continue;
                                        }
                                    }


                                    pi.OverrideSettings = ps;
                                    PlotInfoValidator piv = new PlotInfoValidator();
                                    piv.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;
                                    piv.Validate(pi);

                                    if (numSheet == 1)
                                    {
                                        DebugLogs.Add("Attempting to begin plot");
                                        pe.BeginPlot(null, null);
                                        DebugLogs.Add("Attempting to begin document");
                                        pe.BeginDocument(pi, doc.Name, null, 1, true, input.OutputFilePath);
                                    }

                                    var progress = $"Plotting {System.IO.Path.GetFileName(doc.Name)} - layout {lo.LayoutName} sheet {numSheet} of {layoutsToPlot.Count}";
                                    DebugLogs.Add(progress);

                                    if (numSheet % 2 != 0)
                                    {
                                        if (layoutsToPlot.Count > 10)
                                        {
                                            var test = 1;
                                        }
                                        sheetProgress = $"Layout Progress - {numSheet} of {layoutsToPlot.Count}";
                                        var sheetPercent = (int)((numSheet / (layoutsToPlot.Count * 1.0)) * 100);
                                    }

                                    PlotPageInfo ppi = new PlotPageInfo();
                                    DebugLogs.Add("Attempting to begin page");
                                    pe.BeginPage(ppi, pi, (numSheet == layoutsToPlot.Count), null);
                                    DebugLogs.Add("Attempting to begin generating graphics");
                                    pe.BeginGenerateGraphics(null);
                                    DebugLogs.Add("Attempting to end generating graphics");
                                    pe.EndGenerateGraphics(null);

                                    DebugLogs.Add("Attempting to end page");
                                    pe.EndPage(null);
                                    numSheet++;

                                    ppi.Dispose();
                                    ps.Dispose();
                                    psv.Dispose();
                                    piv.Dispose();
                                    pi.Dispose();
                                }

                                DebugLogs.Add("Attempting to end document");
                                pe.EndDocument(null);

                                DebugLogs.Add("Attempting to end plot");
                                pe.EndPlot(null);
                            }
                        }
                        else
                        {
                            DebugLogs.Add("Another plot is in progress.");
                        }
                        LayoutManager.Current.CurrentLayout = startingLayout;
                        tr.Abort();
                    }
                }

                if (File.Exists(input.OutputFilePath))
                {
                    DebugLogs.Add("PDF created: " + input.OutputFilePath);
                    plotReturn.Error = false;
                    plotReturn.OutputPath = input.OutputFilePath;
                }
                else
                {
                    DebugLogs.Add("PDF failed: " + input.OutputFilePath);
                }
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
                plotReturn.ErrorMessage += ex.Message + Environment.NewLine;
                plotReturn.Exceptions.Add(ex);
            }
            finally
            {
                Application.SetSystemVariable("BACKGROUNDPLOT", bgPlot);
            }

            if (closeCurrentDrawingAfterPlot)
            {
                try
                {
                    Application.DocumentManager.MdiActiveDocument.CloseAndDiscard();
                }
                catch (Exception ex)
                {
                    Exceptions.Add(ex);
                }
            }

            return plotReturn;

        }



    }
}
