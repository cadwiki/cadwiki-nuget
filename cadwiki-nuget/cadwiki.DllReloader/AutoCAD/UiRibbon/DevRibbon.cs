using System;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons;

namespace cadwiki.DllReloader.AutoCAD.UiRibbon
{
    /// <summary>
    /// A ready-to-use development ribbon tab with Reloader and Pipeline buttons.
    /// Designed to be shown/hidden based on debug mode.
    ///
    /// <para><b>Features:</b></para>
    /// <list type="bullet">
    ///   <item><b>Reloader</b> button — triggers DLL hot-reload via <see cref="AutoCADAppDomainDllReloader"/>.</item>
    ///   <item><b>Pipeline</b> button — runs a caller-supplied build/test pipeline action.</item>
    ///   <item><b>Show/Hide</b> — call <see cref="Show"/> and <see cref="Hide"/> to toggle
    ///         visibility based on debug mode or user preference.</item>
    /// </list>
    ///
    /// <para><b>Usage (in your IExtensionApplication.Initialize):</b></para>
    /// <code>
    ///   // Show the dev ribbon only in debug builds:
    ///   #if DEBUG
    ///       DevRibbon.Show(doc, reloader, Assembly.GetExecutingAssembly(),
    ///           pipelineAction: () => {
    ///               var pipeline = new PluginReloadPipeline(...);
    ///               pipeline.Execute(doc);
    ///           });
    ///   #endif
    ///
    ///   // Or toggle at runtime:
    ///   if (isDebugMode)
    ///       DevRibbon.Show(doc, reloader, Assembly.GetExecutingAssembly());
    ///   else
    ///       DevRibbon.Hide();
    /// </code>
    ///
    /// <para><b>Extending:</b> Inject additional buttons after installation:</para>
    /// <code>
    ///   DevRibbon.Show(doc, reloader, asm);
    ///   RibbonBuilder.InjectButton(DevRibbon.TabTitle, DevRibbon.DevToolsPanelTitle,
    ///       RibbonButtonDefinition.Builder("Custom Tool")
    ///           .OnClick(() => RunMyTool())
    ///           .Build());
    /// </code>
    /// </summary>
    public static class DevRibbon
    {
        /// <summary>The tab title/ID used for the dev ribbon.</summary>
        public const string TabTitle = "cw Dev";

        /// <summary>The panel title for the main dev tools panel.</summary>
        public const string DevToolsPanelTitle = "cw Dev Tools";

        /// <summary>The panel title for the info panel.</summary>
        public const string InfoPanelTitle = "cw Info";

        private static bool _isVisible;

        /// <summary>Whether the dev ribbon is currently visible.</summary>
        public static bool IsVisible => _isVisible;

        /// <summary>
        /// Builds and installs the Dev ribbon tab with Reloader and Pipeline buttons.
        /// Safe to call multiple times (idempotent).
        /// </summary>
        /// <param name="doc">Active AutoCAD document.</param>
        /// <param name="reloader">The DLL reloader instance for the Reloader button.</param>
        /// <param name="currentAssembly">
        ///   The executing assembly (passed to the reload handler).
        /// </param>
        /// <param name="pipelineAction">
        ///   Optional action to run when the Pipeline button is clicked. If null,
        ///   the Pipeline button is omitted. This keeps DevRibbon decoupled from
        ///   any specific pipeline implementation.
        /// </param>
        public static void Show(
            Document doc,
            AutoCADAppDomainDllReloader reloader,
            Assembly currentAssembly,
            Action pipelineAction = null)
        {
            if (reloader == null) throw new ArgumentNullException(nameof(reloader));
            if (currentAssembly == null) throw new ArgumentNullException(nameof(currentAssembly));

            // ── Reloader button ─────────────────────────────────────────────
            var reloadButton = RibbonButtonDefinition.Builder("Reload DLL")
                .Tooltip("Hot-reload the plugin DLL into AutoCAD (Reload Count: "
                    + reloader.GetReloadCount() + ")")
                .Handler(new DllReloadClickCommandHandler())
                .Parameter(new UiRouter(
                    "n/a", "n/a", "n/a", new object[] { },
                    reloader, currentAssembly))
                .Build();

            // ── Dev Tools panel ─────────────────────────────────────────────
            var devToolsPanel = new RibbonPanelDefinition(DevToolsPanelTitle)
                .Add(reloadButton);

            // ── Pipeline button (optional) ──────────────────────────────────
            if (pipelineAction != null)
            {
                var pipelineButton = RibbonButtonDefinition.Builder("Pipeline")
                    .Tooltip("Run build → stage → reload pipeline")
                    .OnClick(pipelineAction)
                    .Build();
                devToolsPanel.Add(pipelineButton);
            }

            // ── Info panel ──────────────────────────────────────────────────
            string versionStr = currentAssembly.GetName().Version?.ToString() ?? "?";
            string dllName = System.IO.Path.GetFileName(currentAssembly.Location);

            var versionButton = RibbonButtonDefinition.Builder("Version")
                .Text(" v" + versionStr + " ")
                .Disabled()
                .Build();

            var dllNameButton = RibbonButtonDefinition.Builder("DLL")
                .Text(" Dll: " + dllName)
                .Disabled()
                .Build();

            var reloadCountButton = RibbonButtonDefinition.Builder("Reload Count")
                .Text(" Reloads: " + reloader.GetReloadCount())
                .Disabled()
                .Build();

            var infoPanel = new RibbonPanelDefinition(InfoPanelTitle)
                .Add(versionButton)
                .Add(dllNameButton)
                .Add(reloadCountButton);

            // ── Tab ─────────────────────────────────────────────────────────
            var devTab = new RibbonTabDefinition(TabTitle)
                .Add(devToolsPanel)
                .Add(infoPanel);

            _isVisible = RibbonBuilder.InstallTab(doc, devTab);
        }

        /// <summary>
        /// Removes the Dev ribbon tab from the AutoCAD ribbon.
        /// Safe to call even if the tab isn't currently shown.
        /// </summary>
        public static void Hide()
        {
            RibbonBuilder.RemoveTab(TabTitle);
            _isVisible = false;
        }

        /// <summary>
        /// Toggles the Dev ribbon visibility.
        /// </summary>
        public static void Toggle(
            Document doc,
            AutoCADAppDomainDllReloader reloader,
            Assembly currentAssembly,
            Action pipelineAction = null)
        {
            if (_isVisible)
            {
                Hide();
            }
            else
            {
                Show(doc, reloader, currentAssembly, pipelineAction);
            }
        }
    }
}
