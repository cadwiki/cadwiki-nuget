using System;
using System.IO;

namespace cadwiki.PluginReloadService
{
    /// <summary>
    /// Orchestrates the complete zero-touch hot-reload pipeline for an external
    /// AutoCAD C# plugin:
    /// <list type="number">
    ///   <item>Resolve configuration (filter spec, staging root).</item>
    ///   <item>Stage the plugin build output via <see cref="StagingCopier"/>
    ///     (version-rewrite managed DLLs, copy the rest).</item>
    ///   <item>Trigger in-process reload via <see cref="ReloadOrchestrator"/>.</item>
    /// </list>
    ///
    /// Usage (from a Reload ribbon button handler):
    /// <code>
    ///   var pipeline = new PluginReloadPipeline(
    ///       pluginName:     "MyPlugin",
    ///       sourceBuildDir: @"C:\repos\MyPlugin\bin\Debug\",
    ///       mainDllName:    "MyPlugin.dll",
    ///       reloader:       this   // the current AutoCADAppDomainDllReloader
    ///   );
    ///   pipeline.Execute(doc);
    /// </code>
    /// </summary>
    public class PluginReloadPipeline
    {
        // -------------------------------------------------------------------------
        // Fields
        // -------------------------------------------------------------------------

        private readonly string _pluginName;
        private readonly string _sourceBuildDir;
        private readonly string _mainDllName;
        private readonly DllReloader.AutoCAD.AutoCADAppDomainDllReloader _reloader;
        private readonly string _iniPath;
        private readonly string[] _cliArgs;

        // -------------------------------------------------------------------------
        // Construction
        // -------------------------------------------------------------------------

        /// <summary>
        /// Creates a new pipeline instance.
        /// </summary>
        /// <param name="pluginName">
        ///   Logical name of the plugin, used as the staging sub-directory.
        ///   Typically the assembly name without the <c>.dll</c> extension.
        /// </param>
        /// <param name="sourceBuildDir">
        ///   Full path to the plugin's build output directory (e.g.
        ///   <c>C:\repos\MyPlugin\bin\Debug\</c>).
        /// </param>
        /// <param name="mainDllName">
        ///   Filename of the main plugin DLL (e.g. <c>MyPlugin.dll</c>).
        ///   The pipeline ensures this DLL is always version-rewritten.
        /// </param>
        /// <param name="reloader">
        ///   The live <see cref="DllReloader.AutoCAD.AutoCADAppDomainDllReloader"/>
        ///   instance that performs the AppDomain reload. May be <c>null</c> if
        ///   you only want to test the staging stage without triggering a reload.
        /// </param>
        /// <param name="iniPath">
        ///   Optional path to an INI file containing a <c>[DllFilter]</c> section.
        /// </param>
        /// <param name="cliArgs">
        ///   Optional CLI args (e.g. from a command-line driver) containing
        ///   <c>--include</c> / <c>--exclude</c> tokens.
        /// </param>
        public PluginReloadPipeline(
            string pluginName,
            string sourceBuildDir,
            string mainDllName,
            DllReloader.AutoCAD.AutoCADAppDomainDllReloader reloader = null,
            string iniPath   = null,
            string[] cliArgs = null)
        {
            _pluginName     = pluginName ?? throw new ArgumentNullException(nameof(pluginName));
            _sourceBuildDir = sourceBuildDir ?? throw new ArgumentNullException(nameof(sourceBuildDir));
            _mainDllName    = mainDllName ?? throw new ArgumentNullException(nameof(mainDllName));
            _reloader       = reloader;
            _iniPath        = iniPath;
            _cliArgs        = cliArgs;
        }

        // -------------------------------------------------------------------------
        // Public API
        // -------------------------------------------------------------------------

        /// <summary>
        /// Executes the full reload pipeline and returns a result object.
        /// Exceptions are caught, logged, and returned in
        /// <see cref="PipelineResult.Error"/> rather than thrown; the caller
        /// should check <see cref="PipelineResult.IsSuccess"/>.
        /// </summary>
        /// <param name="doc">
        ///   Active AutoCAD document (used for editor log output in the reload
        ///   stage). May be <c>null</c> when running headless / in unit tests.
        /// </param>
        public PipelineResult Execute(Autodesk.AutoCAD.ApplicationServices.Document doc = null)
        {
            const string Component = "PluginReloadPipeline";
            var startTime = DateTime.Now;

            // ---- Logger bootstrap (writes to %TEMP%\cadwiki.PluginStaging\{plugin}\{ts}\_reload.log)
            string logDir = Path.Combine(
                Path.GetTempPath(),
                StagingCopier.StagingRootName,
                _pluginName,
                startTime.ToString("yyyyMMdd--HH_mm_ss"));

            Directory.CreateDirectory(logDir);
            string logPath = Path.Combine(logDir, "_reload.log");
            var logger = new PluginReloadLogger(logPath, verbose: false);

            logger.Separator();
            logger.Info(Component, $"Pipeline STARTED — plugin: {_pluginName}");
            logger.Info(Component, $"Source dir: {_sourceBuildDir}");
            logger.Info(Component, $"Main DLL:   {_mainDllName}");

            try
            {
                // ---- Step 1: Resolve filter spec -----------------------------------
                var filterSpec = DllFilterSpec.LoadFilterSpec(
                    _sourceBuildDir,
                    _mainDllName,
                    _iniPath,
                    _cliArgs,
                    logger);

                logger.Info(Component, $"Filter spec: {filterSpec.DescribeMode()}");

                // ---- Step 2: Stage plugin ------------------------------------------
                var copier = new StagingCopier(
                    _pluginName,
                    _sourceBuildDir,
                    filterSpec,
                    logger);

                var staging = copier.StagePlugin(buildTime: startTime);

                logger.Info(Component,
                    $"Staging result: {staging.DllCount} DLL(s), " +
                    $"{staging.RewrittenCount} rewritten, " +
                    $"folder: {staging.StagingFolder}");

                // ---- Step 3: Trigger AutoCAD reload (if reloader is provided) ------
                bool reloadTriggered = false;
                Exception reloadError = null;

                if (_reloader != null)
                {
                    try
                    {
                        var orchestrator = new ReloadOrchestrator(_reloader, logger);
                        orchestrator.TriggerReload(doc, staging, _mainDllName);
                        reloadTriggered = true;
                        logger.Info(Component, "Reload triggered successfully.");
                    }
                    catch (Exception ex)
                    {
                        reloadError = ex;
                        logger.Exception(Component, "TriggerReload", ex);
                    }
                }
                else
                {
                    logger.Info(Component,
                        "No reloader configured — staging complete but AppDomain reload skipped.");
                }

                // ---- Finalize ------------------------------------------------------
                var elapsed = DateTime.Now - startTime;
                logger.WriteSummary(_pluginName, elapsed, staging.DllCount,
                    staging.RewrittenCount, staging.StagingFolder);

                return new PipelineResult
                {
                    PluginName      = _pluginName,
                    StagingResult   = staging,
                    ReloadTriggered = reloadTriggered,
                    ReloadError     = reloadError,
                    LogFilePath     = logPath,
                    Elapsed         = elapsed
                };
            }
            catch (Exception ex)
            {
                logger.Exception(Component, "Execute", ex);
                logger.Separator();

                return new PipelineResult
                {
                    PluginName    = _pluginName,
                    Error         = ex,
                    LogFilePath   = logPath,
                    Elapsed       = DateTime.Now - startTime
                };
            }
        }
    }

    // -------------------------------------------------------------------------
    // Result model
    // -------------------------------------------------------------------------

    /// <summary>Outcome of a <see cref="PluginReloadPipeline.Execute"/> call.</summary>
    public class PipelineResult
    {
        /// <summary>Plugin name.</summary>
        public string PluginName { get; set; }

        /// <summary>Staging outcome (null if pipeline failed before staging).</summary>
        public StagingResult StagingResult { get; set; }

        /// <summary>Whether the AppDomain reload was triggered successfully.</summary>
        public bool ReloadTriggered { get; set; }

        /// <summary>Exception thrown during the reload step, or <c>null</c>.</summary>
        public Exception ReloadError { get; set; }

        /// <summary>Unhandled exception from the pipeline itself, or <c>null</c>.</summary>
        public Exception Error { get; set; }

        /// <summary>Full path of the <c>_reload.log</c> file.</summary>
        public string LogFilePath { get; set; }

        /// <summary>Total wall-clock time for the pipeline execution.</summary>
        public TimeSpan Elapsed { get; set; }

        /// <summary><c>true</c> iff the pipeline completed without any unhandled error.</summary>
        public bool IsSuccess => Error == null;

        /// <summary>
        /// Opens the <c>_reload.log</c> file in the default associated application.
        /// </summary>
        public void OpenLog()
        {
            if (!string.IsNullOrEmpty(LogFilePath) && File.Exists(LogFilePath))
                System.Diagnostics.Process.Start(LogFilePath);
        }
    }
}
