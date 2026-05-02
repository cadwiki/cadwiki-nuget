using System;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using cadwiki.DllReloader.AutoCAD;

namespace cadwiki.PluginReloadService
{
    /// <summary>
    /// Bridges the staging pipeline to the existing
    /// <see cref="AutoCADAppDomainDllReloader"/> hot-reload infrastructure.
    ///
    /// This class is intentionally thin: all reload logic stays in
    /// <see cref="AutoCADAppDomainDllReloader"/>. The orchestrator's only job is
    /// to translate a <see cref="StagingResult"/> into the parameters needed
    /// by <see cref="AutoCADAppDomainDllReloader.ReloadFromStagedFolder"/>.
    ///
    /// <para><b>Design note:</b> The original <c>ReloadDll(doc, assembly, dllPath)</c>
    /// method requires a live <see cref="System.Reflection.Assembly"/> object for
    /// the current version of the plugin, which is not available for
    /// <em>external</em> plugins (those not loaded by the cadwiki assembly itself).
    /// <see cref="AutoCADAppDomainDllReloader.ReloadFromStagedFolder"/> handles
    /// the null-assembly case gracefully.
    /// </para>
    /// </summary>
    public class ReloadOrchestrator
    {
        // -------------------------------------------------------------------------
        // Fields
        // -------------------------------------------------------------------------

        private readonly AutoCADAppDomainDllReloader _reloader;
        private readonly PluginReloadLogger _logger;

        // -------------------------------------------------------------------------
        // Construction
        // -------------------------------------------------------------------------

        /// <summary>
        /// Creates a new orchestrator.
        /// </summary>
        /// <param name="reloader">
        ///   Live reloader instance. Must not be <c>null</c>.
        /// </param>
        /// <param name="logger">Optional structured logger.</param>
        public ReloadOrchestrator(AutoCADAppDomainDllReloader reloader,
                                   PluginReloadLogger logger = null)
        {
            _reloader = reloader ?? throw new ArgumentNullException(nameof(reloader));
            _logger   = logger;
        }

        // -------------------------------------------------------------------------
        // Public API
        // -------------------------------------------------------------------------

        /// <summary>
        /// Invokes the AppDomain reload for the files in the staging folder.
        /// </summary>
        /// <param name="doc">
        ///   Active AutoCAD document. May be <c>null</c> in headless/test scenarios;
        ///   in that case, the reloader fetches the active document internally.
        /// </param>
        /// <param name="staging">
        ///   The result of the preceding <see cref="StagingCopier.StagePlugin"/> call.
        /// </param>
        /// <param name="mainDllName">
        ///   Filename of the main plugin DLL (e.g. <c>MyPlugin.dll</c>).
        ///   Used to locate the entry-point assembly in the staging folder.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///   Thrown when the staging result is null or the staging folder is missing.
        /// </exception>
        public void TriggerReload(
            Document doc,
            StagingResult staging,
            string mainDllName)
        {
            const string Component = "ReloadOrchestrator";

            if (staging == null)
                throw new ArgumentNullException(nameof(staging));

            if (!Directory.Exists(staging.StagingFolder))
            {
                string msg = $"Staging folder does not exist: {staging.StagingFolder}";
                _logger?.Error(Component, "TriggerReload", msg, staging.StagingFolder,
                    "Ensure StagingCopier.StagePlugin completed without errors.");
                throw new InvalidOperationException(msg);
            }

            _logger?.Separator();
            _logger?.Info(Component, "TriggerReload STARTED");
            _logger?.Info(Component, $"  Staged folder : {staging.StagingFolder}");
            _logger?.Info(Component, $"  Main DLL      : {mainDllName}");
            _logger?.Info(Component, $"  DLL count     : {staging.DllCount}");
            _logger?.Info(Component, $"  Rewritten     : {staging.RewrittenCount}");

            // Resolve the document: fall back to AutoCAD's active document
            var resolvedDoc = doc
                ?? Autodesk.AutoCAD.ApplicationServices.Core.Application
                       .DocumentManager.MdiActiveDocument;

            try
            {
                // Delegate to the new wrapper method on the existing reloader.
                // This reuses the full ReloadAll pipeline without duplication.
                _reloader.ReloadFromStagedFolder(
                    resolvedDoc,
                    staging.StagingFolder,
                    mainDllName);

                _logger?.Info(Component, "ReloadFromStagedFolder completed successfully.");
            }
            catch (Exception ex)
            {
                _logger?.Exception(Component, "TriggerReload", ex, staging.StagingFolder);
                throw;
            }
            finally
            {
                _logger?.Separator();
            }
        }
    }
}
