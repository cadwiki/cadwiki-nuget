using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;
using System;
using System.Collections.Generic;
using System.IO;
using PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType;

namespace cadwiki.AC.Plotters
{
    public partial class PlotterSinglePage
    {
        public static List<String> DebugLogs = new List<String>();
        public static List<Exception> Exceptions = new List<Exception>();
        public static List<String> ProgressAlerts = new List<String>();
        /// <summary>
        /// This workflow only works for 1 layout at a time
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static PlotReturn PlotSingleLayoutToSinglePagePDF(Document doc, Database db, Input input)
        {
            var outputFile = "";
            var bgPlot = 0;
            var plotReturn = new PlotReturn();

            try
            {
                using (DocumentLock docLock = doc.LockDocument())
                {
                    using (Transaction trans = db.TransactionManager.StartTransaction())
                    {
                        bgPlot = (short)Application.GetSystemVariable("BACKGROUNDPLOT");
                        Application.SetSystemVariable("BACKGROUNDPLOT", 0);
                        LayoutManager layoutMgr = LayoutManager.Current;
                        ObjectId clayoutId = layoutMgr.GetLayoutId(layoutMgr.CurrentLayout);
                        Layout clayout = trans.GetObject(clayoutId, OpenMode.ForRead) as Layout;

                        DBDictionary layoutDict = (DBDictionary)trans.GetObject(db.LayoutDictionaryId, OpenMode.ForRead);

                        ProgressAlerts.Add("Processing layouts");

                        // Loop through all the layouts
                        foreach (DBDictionaryEntry entry in layoutDict)
                        {
                            // Open the layout for read
                            Layout layout = (Layout)trans.GetObject(entry.Value, OpenMode.ForRead);

                            if (input == null || !layout.LayoutName.Equals(input.LayoutName))
                            {
                                continue;
                            }

                            DebugLogs.Add("Attempting to print with settings: " + input.ToJson());

                            DebugLogs.Add("Attempting to get layout: " + layout.LayoutName);
                            ObjectId layoutId = layoutMgr.GetLayoutId(layout.LayoutName);
                            if (layoutMgr.CurrentLayout != layout.LayoutName)
                            {
                                layoutMgr.CurrentLayout = layout.LayoutName;
                                //layoutMgr.SetCurrentLayoutId(layoutId);
                            }

                            ProgressAlerts.Add("Validating plot config");

                            PlotSettings plotSettings = new PlotSettings(layout.ModelType);
                            PlotSettingsValidator plotValidator = PlotSettingsValidator.Current;

                            DebugLogs.Add("Attempting to copy plot settings from layout: " + layout.LayoutName);
                            plotSettings.CopyFrom(layout);

                            try
                            {
                                DebugLogs.Add("Attempting to set plot config name: " + layout.LayoutName);
                                plotValidator.SetPlotConfigurationName(plotSettings, input.PlotDeviceName, input.MediaName);
                            }
                            catch (Autodesk.AutoCAD.Runtime.Exception ex)
                            {
                                Exceptions.Add(ex);
                                var msg = string.Format("Error setting plot device '{0}' with media type '{1}'. " +
                                    "Please make sure your plotters are installed.",
                                    input.PlotDeviceName,
                                    input.MediaName);
                                plotReturn.ErrorMessage = msg;
                                DebugLogs.Add(plotReturn.ErrorMessage);
                                throw new InvalidOperationException(msg, ex);
                            }

                            DebugLogs.Add("Attempting to set plot config:");
                            plotValidator.SetDefaultPlotConfig(plotSettings);
                            try
                            {
                                DebugLogs.Add("Attempting to set style sheet: " + input.SheetStyleName);
                                plotValidator.SetCurrentStyleSheet(plotSettings, input.SheetStyleName);
                            }
                            catch (Exception ex)
                            {
                                Exceptions.Add(ex);
                                DebugLogs.Add("Falling back to default ctb");
                                plotValidator.SetCurrentStyleSheet(plotSettings, "monochrome.ctb");
                            }
                            DebugLogs.Add("Attempting to set plot rotation 0 degrees");
                            plotValidator.SetPlotRotation(plotSettings, PlotRotation.Degrees000);
                            DebugLogs.Add("Attempting to set plot type extents");
                            plotValidator.SetPlotType(plotSettings, PlotType.Extents);
                            DebugLogs.Add("Attempting to use standard scale");
                            plotValidator.SetUseStandardScale(plotSettings, true);
                            DebugLogs.Add("Attempting to set standard scale to fit");
                            plotValidator.SetStdScaleType(plotSettings, StdScaleType.ScaleToFit);
                            DebugLogs.Add("Attempting to set plot centered");
                            plotValidator.SetPlotCentered(plotSettings, true);


                            PlotInfo plotInfo = new PlotInfo();
                            plotInfo.Layout = layoutId;
                            plotInfo.OverrideSettings = plotSettings;

                            PlotInfoValidator plotInfoValidator = new PlotInfoValidator();
                            plotInfoValidator.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;

                            DebugLogs.Add("Attempting to validate plot config: " + layout.LayoutName);
                            plotInfoValidator.Validate(plotInfo);

                            DebugLogs.Add("Plot factory state is: " + PlotFactory.ProcessPlotState);

                            if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
                            {
                                DebugLogs.Add("Attempting to create publish engine");
                                ProgressAlerts.Add("Creating plot engine");

                                using (PlotEngine plotEngine = PlotFactory.CreatePublishEngine())
                                {
                                    ProgressAlerts.Add("Plotting with plot engine");

                                    DebugLogs.Add("Attempting to begin plot");
                                    plotEngine.BeginPlot(null, null);
                                    DebugLogs.Add("Attempting to begin document");
                                    plotEngine.BeginDocument(plotInfo, doc.Name, null, 1, true, input.OutputFilePath);

                                    PlotPageInfo pageInfo = new PlotPageInfo();
                                    DebugLogs.Add("Attempting to begin page");
                                    plotEngine.BeginPage(pageInfo, plotInfo, true, null);
                                    DebugLogs.Add("Attempting to begin generating graphics");
                                    plotEngine.BeginGenerateGraphics(null);
                                    DebugLogs.Add("Attempting to end generating graphics");
                                    plotEngine.EndGenerateGraphics(null);
                                    DebugLogs.Add("Attempting to end page");
                                    plotEngine.EndPage(null);

                                    DebugLogs.Add("Attempting to end document");
                                    plotEngine.EndDocument(null);
                                    DebugLogs.Add("Attempting to end plot");
                                    plotEngine.EndPlot(null);
                                    DebugLogs.Add("Attempting to destroy dialog");

                                    ProgressAlerts.Add("Plot complete");

                                    if (File.Exists(input.OutputFilePath))
                                    {
                                        DebugLogs.Add("PDF created: " + input.OutputFilePath);
                                        outputFile = input.OutputFilePath;
                                        plotReturn.Error = false;
                                        plotReturn.OutputPath = outputFile;
                                    }
                                    else
                                    {
                                        plotReturn.ErrorMessage = "PDF failed: " + input.OutputFilePath;
                                        DebugLogs.Add(plotReturn.ErrorMessage);
                                    }
                                }
                            }
                            else
                            {
                                plotReturn.ErrorMessage = "Plot factory is plotting, not able to start a new sheet";
                                DebugLogs.Add(plotReturn.ErrorMessage);
                            }
                        }
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
                plotReturn.ErrorMessage = ex.Message;
            }
            finally
            {
                Application.SetSystemVariable("BACKGROUNDPLOT", bgPlot);
            }
            return plotReturn;
        }
    }
}
