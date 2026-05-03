using System.Collections.Generic;

namespace cadwiki.DllReloader.AutoCAD.UiRibbon
{
    /// <summary>
    /// Describes a ribbon panel (a group of buttons within a tab) without
    /// creating any AutoCAD ribbon objects. Panels are materialized by
    /// <see cref="RibbonBuilder.BuildPanel"/>.
    ///
    /// <para><b>Example — define a panel with three buttons:</b></para>
    /// <code>
    ///   var panel = new RibbonPanelDefinition("Tools")
    ///       .Add(reloadButton)
    ///       .Add(pipelineButton)
    ///       .Add(settingsButton);
    /// </code>
    /// </summary>
    public class RibbonPanelDefinition
    {
        /// <summary>Panel title displayed in the ribbon.</summary>
        public string Title { get; }

        /// <summary>Ordered list of button definitions in this panel.</summary>
        public List<RibbonButtonDefinition> Buttons { get; } = new List<RibbonButtonDefinition>();

        /// <summary>
        /// Creates a new panel definition with the given title.
        /// </summary>
        public RibbonPanelDefinition(string title)
        {
            Title = title;
        }

        /// <summary>
        /// Fluently add a button definition to this panel.
        /// </summary>
        public RibbonPanelDefinition Add(RibbonButtonDefinition button)
        {
            Buttons.Add(button);
            return this;
        }
    }
}
