using System.Collections.Generic;

namespace cadwiki.DllReloader.AutoCAD.UiRibbon
{
    /// <summary>
    /// Describes a ribbon tab (a top-level tab in the AutoCAD ribbon bar)
    /// containing one or more <see cref="RibbonPanelDefinition"/> groups.
    /// Materialized by <see cref="RibbonBuilder.BuildTab"/>.
    ///
    /// <para><b>Example:</b></para>
    /// <code>
    ///   var tab = new RibbonTabDefinition("MyPlugin Dev")
    ///       .Add(toolsPanel)
    ///       .Add(infoPanel);
    /// </code>
    /// </summary>
    public class RibbonTabDefinition
    {
        /// <summary>Tab title and ID.</summary>
        public string Title { get; }

        /// <summary>Ordered list of panel definitions in this tab.</summary>
        public List<RibbonPanelDefinition> Panels { get; } = new List<RibbonPanelDefinition>();

        /// <summary>
        /// Whether this tab should become the active (selected) tab when installed.
        /// Default <c>true</c> for backward compatibility. Set to <c>false</c> to
        /// install the tab without stealing focus from the current ribbon tab.
        ///
        /// <para><b>Example — background tab:</b></para>
        /// <code>
        ///   var tab = new RibbonTabDefinition("Diagnostics")
        ///       .SetActive(false)   // don't steal focus
        ///       .Add(diagPanel);
        /// </code>
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Creates a new tab definition with the given title (also used as the tab ID).
        /// </summary>
        public RibbonTabDefinition(string title)
        {
            Title = title;
        }

        /// <summary>
        /// Creates a new tab definition with the given title and active state.
        /// </summary>
        /// <param name="title">Tab title and ID.</param>
        /// <param name="isActive">
        /// Whether the tab becomes active when installed.
        /// Default <c>true</c> for backward compatibility.
        /// </param>
        public RibbonTabDefinition(string title, bool isActive)
        {
            Title = title;
            IsActive = isActive;
        }

        /// <summary>
        /// Fluently add a panel definition to this tab.
        /// </summary>
        public RibbonTabDefinition Add(RibbonPanelDefinition panel)
        {
            Panels.Add(panel);
            return this;
        }

        /// <summary>
        /// Fluently set whether this tab should become active when installed.
        /// </summary>
        /// <param name="active">
        /// <c>true</c> to activate the tab (default), <c>false</c> to install
        /// without stealing focus.
        /// </param>
        public RibbonTabDefinition SetActive(bool active)
        {
            IsActive = active;
            return this;
        }
    }
}
