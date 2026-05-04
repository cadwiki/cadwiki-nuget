using System;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using cadwiki.DllReloader.AutoCAD.UiRibbon;
using cadwiki.NetUtils;

namespace cadwiki.DllReloader.AutoCAD
{
    /// <summary>
    /// Abstract base class for AutoCAD IExtensionApplication plugins that use
    /// the cadwiki ribbon pipeline. Encapsulates the boilerplate of:
    /// <list type="bullet">
    ///   <item>Creating and configuring the <see cref="AutoCADAppDomainDllReloader"/>.</item>
    ///   <item>Wiring up <c>AppDomain.AssemblyResolve</c>.</item>
    ///   <item>Building and installing ribbon tabs (dev and/or production).</item>
    ///   <item>Optionally showing the <see cref="DevRibbon"/>.</item>
    ///   <item>Attaching quiescent-state reactors for deferred ribbon setup.</item>
    /// </list>
    ///
    /// <para><b>Subclasses override a few methods</b> to define their ribbon
    /// structure and behavior — everything else is handled by the base class.</para>
    ///
    /// <para><b>Minimal example:</b></para>
    /// <code>
    ///   public class MyApp : cwExtApp
    ///   {
    ///       protected override bool ShowDevRibbon => true;
    ///
    ///       protected override RibbonTabDefinition CreateProductionTab()
    ///       {
    ///           var panel = new RibbonPanelDefinition("My Tools")
    ///               .Add(RibbonButtonDefinition.Builder("Do Thing")
    ///                   .Tooltip("Does the thing")
    ///                   .OnClick(() => DoThing())
    ///                   .Build());
    ///           return new RibbonTabDefinition("My Plugin")
    ///               .Add(panel);
    ///       }
    ///   }
    /// </code>
    ///
    /// <para><b>Advanced example with pipeline and inactive tab:</b></para>
    /// <code>
    ///   public class MyApp : cwExtApp
    ///   {
    ///       protected override bool ShowDevRibbon => true;
    ///       protected override bool SkipCadwikiDlls => false;
    ///
    ///       protected override Action CreatePipelineAction(Assembly asm)
    ///       {
    ///           return () => { /* custom pipeline logic */ };
    ///       }
    ///
    ///       protected override RibbonTabDefinition CreateProductionTab()
    ///       {
    ///           // Tab is installed but won't steal focus from the current tab
    ///           return new RibbonTabDefinition("My Plugin", isActive: false)
    ///               .Add(myPanel);
    ///       }
    ///
    ///       protected override IEnumerable&lt;RibbonTabDefinition&gt; CreateAdditionalTabs()
    ///       {
    ///           // A background diagnostics tab that doesn't activate on load
    ///           yield return new RibbonTabDefinition("Diagnostics")
    ///               .SetActive(false)
    ///               .Add(diagPanel);
    ///       }
    ///
    ///       protected override void OnInitialized(Document doc, Assembly asm)
    ///       {
    ///           doc.Editor.WriteMessage("\nMy plugin loaded!\n");
    ///       }
    ///   }
    /// </code>
    /// </summary>
    public abstract class cwExtApp : IExtensionApplication
    {
        // ── Public state ────────────────────────────────────────────────────

        /// <summary>
        /// The DLL reloader instance. Accessible by subclasses and their
        /// ribbon button handlers.
        /// </summary>
        public AutoCADAppDomainDllReloader Reloader { get; private set; }

        // ── Overridable configuration ───────────────────────────────────────

        /// <summary>
        /// Whether to show the dev ribbon tab (Reload / Pipeline buttons).
        /// Default <c>false</c> — override and return <c>true</c> in debug builds.
        /// </summary>
        protected virtual bool ShowDevRibbon => false;

        /// <summary>
        /// Whether to skip cadwiki DLLs during reload. Default <c>true</c>.
        /// </summary>
        protected virtual bool SkipCadwikiDlls => true;

        /// <summary>
        /// Whether to use quiescent-state reactors for deferred ribbon setup.
        /// Default <c>true</c>. Set to <c>false</c> if your plugin doesn't
        /// need the retry-on-idle pattern.
        /// </summary>
        protected virtual bool UseQuiescentReactors => true;

        // ── Abstract / virtual hooks ────────────────────────────────────────

        /// <summary>
        /// Override to define a production ribbon tab. Return <c>null</c> to
        /// skip (e.g. dev-only plugins). The tab is installed via
        /// <see cref="RibbonBuilder.InstallTab"/>.
        /// </summary>
        /// <returns>A tab definition, or <c>null</c> to skip.</returns>
        protected virtual RibbonTabDefinition CreateProductionTab()
        {
            return null;
        }

        /// <summary>
        /// Override to supply additional ribbon tab definitions beyond the
        /// primary production tab. All returned tabs are installed in order.
        /// </summary>
        protected virtual IEnumerable<RibbonTabDefinition> CreateAdditionalTabs()
        {
            return Array.Empty<RibbonTabDefinition>();
        }

        /// <summary>
        /// Override to provide a pipeline action for the dev ribbon's Pipeline
        /// button. Return <c>null</c> to omit the Pipeline button.
        /// </summary>
        /// <param name="executingAssembly">The assembly of the concrete subclass.</param>
        protected virtual Action CreatePipelineAction(Assembly executingAssembly)
        {
            return null;
        }

        /// <summary>
        /// Called after all initialization is complete (reloader configured,
        /// ribbons installed, reactors attached). Override for custom
        /// post-init logic.
        /// </summary>
        /// <param name="doc">The active document at init time.</param>
        /// <param name="executingAssembly">The subclass assembly.</param>
        protected virtual void OnInitialized(Document doc, Assembly executingAssembly)
        {
        }

        /// <summary>
        /// Called during <see cref="Terminate"/>. Override for custom cleanup.
        /// </summary>
        protected virtual void OnTerminating()
        {
        }

        /// <summary>
        /// Returns the executing assembly for the concrete subclass.
        /// Override only if you need a different assembly (rare).
        /// Default calls <see cref="Assembly.GetCallingAssembly()"/>.
        /// </summary>
        protected virtual Assembly GetPluginAssembly()
        {
            // Subclasses should override this to return Assembly.GetExecutingAssembly()
            // from their own assembly context. The default is a best-effort fallback.
            return Assembly.GetCallingAssembly();
        }

        // ── IExtensionApplication implementation ────────────────────────────

        /// <summary>
        /// Initializes the plugin: configures the reloader, installs ribbon
        /// tabs, optionally shows the dev ribbon, and attaches reactors.
        /// Catches all exceptions to prevent AutoCAD from unloading the addin.
        /// </summary>
        public void Initialize()
        {
            try
            {
                var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application
                    .DocumentManager.MdiActiveDocument;

                var pluginAssembly = GetPluginAssembly();
                var pluginVersion = AssemblyUtils.GetVersion(pluginAssembly);

                doc?.Editor.WriteMessage(
                    Environment.NewLine + GetType().Name + " initializing...");

                // Wire up assembly resolution for DLL reloading
                AppDomain.CurrentDomain.AssemblyResolve += AutodeskAppDomainReloader.AssemblyResolve;

                // Configure reloader
                Reloader = new AutoCADAppDomainDllReloader();
                Reloader.SkipCadwikiDlls = SkipCadwikiDlls;
                Reloader.Configure(pluginAssembly);
                Reloader.Reload(pluginAssembly);

                doc?.Editor.WriteMessage(
                    Environment.NewLine + GetType().Name + " v" + pluginVersion + " loaded.");

                // Dev ribbon (optional)
                if (ShowDevRibbon)
                {
                    var pipelineAction = CreatePipelineAction(pluginAssembly);
                    DevRibbon.Show(doc, Reloader, pluginAssembly, pipelineAction);
                }

                // Production tab (optional)
                var productionTab = CreateProductionTab();
                if (productionTab != null)
                {
                    RibbonBuilder.InstallTab(doc, productionTab);
                }

                // Additional tabs
                foreach (var tabDef in CreateAdditionalTabs())
                {
                    if (tabDef != null)
                    {
                        RibbonBuilder.InstallTab(doc, tabDef);
                    }
                }

                // Quiescent reactors for deferred ribbon setup
                if (UseQuiescentReactors && doc != null)
                {
                    var reactors = new ReactorsRibbonCreate(this);
                    reactors.AttachQuiescentReactors(doc);
                }

                OnInitialized(doc, pluginAssembly);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[cwExtApp] {GetType().Name}.Initialize failed: {ex}");
                try
                {
                    var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application
                        .DocumentManager.MdiActiveDocument;
                    doc?.Editor.WriteMessage(
                        Environment.NewLine + "Warning: " + GetType().Name +
                        " initialization error: " + ex.Message);
                }
                catch
                {
                    // Swallow — can't even write to editor
                }
            }
        }

        /// <summary>
        /// Terminates the plugin and cleans up the reloader.
        /// </summary>
        public void Terminate()
        {
            try
            {
                OnTerminating();
                Reloader?.Terminate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[cwExtApp] {GetType().Name}.Terminate failed: {ex.Message}");
            }
        }

        // ── Inner reactor class for deferred ribbon setup ───────────────────

        /// <summary>
        /// Handles deferred ribbon setup via AutoCAD quiescent-state events.
        /// Retries up to <see cref="MaxRetries"/> times before giving up.
        /// </summary>
        public class ReactorsRibbonCreate
        {
            private readonly cwExtApp _app;
            private bool _isSetupComplete;
            private int _numberOfTries;

            /// <summary>Maximum number of quiescent-state retries.</summary>
            public int MaxRetries { get; set; } = 5;

            /// <summary>Delay (ms) before each retry attempt.</summary>
            public int RetryDelayMs { get; set; } = 2000;

            internal ReactorsRibbonCreate(cwExtApp app)
            {
                _app = app;
            }

            /// <summary>
            /// Attaches document events that trigger ribbon setup on idle.
            /// </summary>
            public void AttachQuiescentReactors(Document doc)
            {
                try
                {
                    Application.DocumentManager.DocumentBecameCurrent += DocumentManager_DocumentBecameCurrent;
                    Application.DocumentManager.DocumentToBeActivated += DocumentManager_DocumentToBeActivated;
                    Application.DocumentManager.DocumentToBeDestroyed += DocumentManager_DocumentToBeDestroyed;
                    doc.Editor.EnteringQuiescentState += Editor_EnteringQuiescentState;
                }
                catch (Exception ex)
                {
                    WriteToEditor(ex.Message);
                }
            }

            /// <summary>Detaches all document event handlers.</summary>
            public void DetachAllReactors()
            {
                try
                {
                    Application.DocumentManager.DocumentBecameCurrent -= DocumentManager_DocumentBecameCurrent;
                    Application.DocumentManager.DocumentToBeActivated -= DocumentManager_DocumentToBeActivated;
                    Application.DocumentManager.DocumentToBeDestroyed -= DocumentManager_DocumentToBeDestroyed;
                    var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application
                        .DocumentManager.MdiActiveDocument;
                    if (doc != null)
                    {
                        doc.Editor.EnteringQuiescentState -= Editor_EnteringQuiescentState;
                    }
                }
                catch (Exception ex)
                {
                    WriteToEditor(ex.Message);
                }
            }

            private void Editor_EnteringQuiescentState(object sender, EventArgs e)
            {
                try
                {
                    var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application
                        .DocumentManager.MdiActiveDocument;
                    _numberOfTries++;
                    System.Threading.Thread.Sleep(RetryDelayMs);

                    if (_numberOfTries >= MaxRetries)
                    {
                        WriteToEditor(Environment.NewLine +
                            "Max number of ribbon create re-tries exceeded.");
                        _isSetupComplete = true;
                        DetachAllReactors();
                        return;
                    }

                    if (doc != null && !_isSetupComplete)
                    {
                        SetupRibbon(doc);
                    }
                }
                catch (Exception ex)
                {
                    WriteToEditor(ex.Message);
                }
            }

            private void SetupRibbon(Document doc)
            {
                bool success = true;

                // Re-install production tab
                var productionTab = _app.CreateProductionTab();
                if (productionTab != null)
                {
                    success &= RibbonBuilder.InstallTab(doc, productionTab);
                }

                // Re-install additional tabs
                foreach (var tabDef in _app.CreateAdditionalTabs())
                {
                    if (tabDef != null)
                    {
                        success &= RibbonBuilder.InstallTab(doc, tabDef);
                    }
                }

                if (success)
                {
                    WriteToEditor(Environment.NewLine + "Ribbon setup complete.");
                    _isSetupComplete = true;
                    DetachAllReactors();
                }
            }

            private void DocumentManager_DocumentBecameCurrent(object sender, DocumentCollectionEventArgs e)
            {
                try
                {
                    if (e.Document != null)
                    {
                        e.Document.Editor.EnteringQuiescentState += Editor_EnteringQuiescentState;
                    }
                }
                catch (Exception ex)
                {
                    WriteToEditor(ex.Message);
                }
            }

            private void DocumentManager_DocumentToBeActivated(object sender, DocumentCollectionEventArgs e)
            {
                try
                {
                    if (e.Document != null)
                    {
                        e.Document.Editor.EnteringQuiescentState -= Editor_EnteringQuiescentState;
                    }
                }
                catch (Exception ex)
                {
                    WriteToEditor(ex.Message);
                }
            }

            private void DocumentManager_DocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
            {
                try
                {
                    if (e.Document != null)
                    {
                        e.Document.Editor.EnteringQuiescentState -= Editor_EnteringQuiescentState;
                    }
                }
                catch (Exception ex)
                {
                    WriteToEditor(ex.Message);
                }
            }

            private void WriteToEditor(string msg)
            {
                try
                {
                    var doc = global::Autodesk.AutoCAD.ApplicationServices.Core.Application
                        .DocumentManager.MdiActiveDocument;
                    doc?.Editor.WriteMessage(msg);
                }
                catch
                {
                    // Swallow — editor not available
                }
            }
        }
    }
}
