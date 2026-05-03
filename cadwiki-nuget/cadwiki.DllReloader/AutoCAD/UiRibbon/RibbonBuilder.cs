using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;

namespace cadwiki.DllReloader.AutoCAD.UiRibbon
{
    /// <summary>
    /// Materializes <see cref="RibbonTabDefinition"/> / <see cref="RibbonPanelDefinition"/> /
    /// <see cref="RibbonButtonDefinition"/> descriptors into live AutoCAD ribbon objects,
    /// and provides runtime injection of buttons into existing panels and tabs.
    ///
    /// <para><b>Design goals:</b></para>
    /// <list type="bullet">
    ///   <item><b>Single-line registration</b> — inject a button into any panel on any
    ///         tab without touching that panel's source code.</item>
    ///   <item><b>Idempotent</b> — <see cref="InstallTab"/> removes an existing tab
    ///         with the same ID before adding, so repeated calls are safe.</item>
    ///   <item><b>Runtime injection</b> — <see cref="InjectButton"/> can add buttons
    ///         to panels that already exist in the ribbon.</item>
    /// </list>
    ///
    /// <para><b>Full example — build and install a tab from definitions:</b></para>
    /// <code>
    ///   // 1. Define buttons
    ///   var reload = RibbonButtonDefinition.Builder("Reload")
    ///       .Tooltip("Hot-reload the plugin DLL")
    ///       .OnClick(() => ReloadService.Execute())
    ///       .Build();
    ///
    ///   var pipeline = RibbonButtonDefinition.Builder("Pipeline")
    ///       .Tooltip("Run build + test pipeline")
    ///       .OnClick(() => PipelineService.Run())
    ///       .Build();
    ///
    ///   // 2. Define panel and tab
    ///   var devPanel = new RibbonPanelDefinition("Dev Tools")
    ///       .Add(reload)
    ///       .Add(pipeline);
    ///
    ///   var devTab = new RibbonTabDefinition("MyPlugin Dev")
    ///       .Add(devPanel);
    ///
    ///   // 3. Install
    ///   RibbonBuilder.InstallTab(doc, devTab);
    ///
    ///   // 4. Later, inject a button into the existing "Dev Tools" panel:
    ///   var extra = RibbonButtonDefinition.Builder("Settings")
    ///       .OnClick(() => ShowSettings())
    ///       .Build();
    ///   RibbonBuilder.InjectButton("MyPlugin Dev", "Dev Tools", extra);
    /// </code>
    /// </summary>
    public static class RibbonBuilder
    {
        // =====================================================================
        // Build: Definition → AutoCAD ribbon objects
        // =====================================================================

        /// <summary>
        /// Materializes a <see cref="RibbonButtonDefinition"/> into a live
        /// <see cref="RibbonButton"/>.
        /// </summary>
        public static RibbonButton BuildButton(RibbonButtonDefinition def)
        {
            var btn = new RibbonButton
            {
                Name = def.Name,
                Text = def.Text,
                ShowText = true,
                Size = def.IsLarge ? RibbonItemSize.Large : RibbonItemSize.Standard,
                IsEnabled = def.IsEnabled,
                ToolTip = def.TooltipText
            };

            // Icon
            if (def.Icon != null)
            {
                var image = NetUtils.Bitmaps.CreateBitmapSourceFromBitmap(def.Icon);
                if (image != null)
                {
                    btn.Image = image;
                    btn.ShowImage = true;
                }
            }

            // Command handler precedence: custom handler > delegate > none
            if (def.CommandHandler != null)
            {
                btn.CommandHandler = def.CommandHandler;
            }
            else if (def.ClickAction != null)
            {
                btn.CommandHandler = new DelegateClickCommandHandler(def.ClickAction);
            }

            if (def.CommandParameter != null)
            {
                btn.CommandParameter = def.CommandParameter;
            }

            return btn;
        }

        /// <summary>
        /// Materializes a <see cref="RibbonPanelDefinition"/> into a live
        /// <see cref="RibbonPanel"/>, including all its buttons arranged
        /// vertically in a single column.
        /// </summary>
        public static RibbonPanel BuildPanel(RibbonPanelDefinition def)
        {
            var source = new RibbonPanelSource { Title = def.Title };
            var row = new RibbonRowPanel { IsTopJustified = true };

            for (int i = 0; i < def.Buttons.Count; i++)
            {
                row.Items.Add(BuildButton(def.Buttons[i]));
                if (i < def.Buttons.Count - 1)
                {
                    row.Items.Add(new RibbonRowBreak());
                }
            }

            source.Items.Add(row);
            return new RibbonPanel { Source = source };
        }

        /// <summary>
        /// Materializes a <see cref="RibbonTabDefinition"/> into a live
        /// <see cref="RibbonTab"/>, including all panels and buttons.
        /// </summary>
        public static RibbonTab BuildTab(RibbonTabDefinition def)
        {
            var tab = new RibbonTab
            {
                Title = def.Title,
                Id = def.Title,
                Name = def.Title
            };

            foreach (var panelDef in def.Panels)
            {
                tab.Panels.Add(BuildPanel(panelDef));
            }

            return tab;
        }

        // =====================================================================
        // Install: Add a tab to the ribbon (idempotent)
        // =====================================================================

        /// <summary>
        /// Builds and installs a tab definition into the AutoCAD ribbon.
        /// Removes any existing tab with the same ID first (idempotent).
        /// </summary>
        /// <param name="doc">Active AutoCAD document (for editor logging).</param>
        /// <param name="tabDef">The tab definition to install.</param>
        /// <param name="makeActive">Whether to activate the tab after installing.</param>
        /// <returns><c>true</c> if installation succeeded.</returns>
        public static bool InstallTab(Document doc, RibbonTabDefinition tabDef, bool makeActive = true)
        {
            var ribbon = ComponentManager.Ribbon;
            if (ribbon == null)
            {
                doc?.Editor.WriteMessage(
                    Environment.NewLine + "Ribbon is not available — type RIBBON into the command line.");
                return false;
            }

            // Remove existing tab with same ID (idempotent)
            var existing = ribbon.FindTab(tabDef.Title);
            if (existing != null)
            {
                doc?.Editor.WriteMessage(
                    Environment.NewLine + "Removing existing tab: " + tabDef.Title);
                ribbon.Tabs.Remove(existing);
            }

            var tab = BuildTab(tabDef);
            ribbon.Tabs.Add(tab);

            if (makeActive)
            {
                tab.IsActive = true;
            }

            doc?.Editor.WriteMessage(
                Environment.NewLine + "Installed ribbon tab: " + tabDef.Title);
            return true;
        }

        /// <summary>
        /// Removes a tab from the ribbon by its title/ID.
        /// Safe to call even if the tab doesn't exist.
        /// </summary>
        /// <returns><c>true</c> if a tab was found and removed.</returns>
        public static bool RemoveTab(string tabTitle)
        {
            var ribbon = ComponentManager.Ribbon;
            if (ribbon == null) return false;

            var existing = ribbon.FindTab(tabTitle);
            if (existing != null)
            {
                ribbon.Tabs.Remove(existing);
                return true;
            }
            return false;
        }

        // =====================================================================
        // Inject: Add buttons to existing panels at runtime
        // =====================================================================

        /// <summary>
        /// Injects a button into an existing panel on an existing tab — without
        /// modifying the panel's source code. This is the key API for the
        /// button injection system.
        ///
        /// <para><b>Example — inject a button into another plugin's panel:</b></para>
        /// <code>
        ///   var myButton = RibbonButtonDefinition.Builder("My Tool")
        ///       .Tooltip("Does something cool")
        ///       .OnClick(() => DoSomething())
        ///       .Build();
        ///   RibbonBuilder.InjectButton("DevTab", "Tests", myButton);
        /// </code>
        /// </summary>
        /// <param name="tabTitle">Title/ID of the target tab.</param>
        /// <param name="panelTitle">Title of the target panel within that tab.</param>
        /// <param name="buttonDef">The button definition to inject.</param>
        /// <returns><c>true</c> if the button was injected successfully.</returns>
        public static bool InjectButton(string tabTitle, string panelTitle, RibbonButtonDefinition buttonDef)
        {
            var ribbon = ComponentManager.Ribbon;
            if (ribbon == null) return false;

            var tab = ribbon.FindTab(tabTitle);
            if (tab == null) return false;

            foreach (var panel in tab.Panels)
            {
                if (panel.Source != null && panel.Source.Title == panelTitle)
                {
                    var btn = BuildButton(buttonDef);

                    // Find or create a RibbonRowPanel to append to
                    RibbonRowPanel targetRow = null;
                    foreach (var item in panel.Source.Items)
                    {
                        if (item is RibbonRowPanel row)
                        {
                            targetRow = row;
                            break;
                        }
                    }

                    if (targetRow != null)
                    {
                        targetRow.Items.Add(new RibbonRowBreak());
                        targetRow.Items.Add(btn);
                    }
                    else
                    {
                        // Panel has no row — add the button directly to the source
                        panel.Source.Items.Add(btn);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Injects an entire panel definition into an existing tab at runtime.
        /// </summary>
        /// <param name="tabTitle">Title/ID of the target tab.</param>
        /// <param name="panelDef">The panel definition to inject.</param>
        /// <returns><c>true</c> if the panel was injected successfully.</returns>
        public static bool InjectPanel(string tabTitle, RibbonPanelDefinition panelDef)
        {
            var ribbon = ComponentManager.Ribbon;
            if (ribbon == null) return false;

            var tab = ribbon.FindTab(tabTitle);
            if (tab == null) return false;

            tab.Panels.Add(BuildPanel(panelDef));
            return true;
        }
    }
}
