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
        /// Creates a new tab definition with the given title (also used as the tab ID).
        /// </summary>
        public RibbonTabDefinition(string title)
        {
            Title = title;
        }

        /// <summary>
        /// Fluently add a panel definition to this tab.
        /// </summary>
        public RibbonTabDefinition Add(RibbonPanelDefinition panel)
        {
            Panels.Add(panel);
            return this;
        }
    }
}
