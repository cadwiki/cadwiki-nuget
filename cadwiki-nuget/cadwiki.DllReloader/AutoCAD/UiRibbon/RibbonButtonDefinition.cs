using System;
using System.Drawing;
using System.Windows.Input;

namespace cadwiki.DllReloader.AutoCAD.UiRibbon
{
    /// <summary>
    /// A lightweight, immutable descriptor for a ribbon button. Captures all
    /// the information needed to materialize an <see cref="Autodesk.Windows.RibbonButton"/>
    /// without requiring callers to touch the AutoCAD ribbon API directly.
    ///
    /// <para><b>Design goals:</b></para>
    /// <list type="bullet">
    ///   <item>Minimal boilerplate — a fluent builder eliminates the 10+ parameter
    ///         constructor that plagued the old <c>Creator.Create</c> API.</item>
    ///   <item>Decoupled from runtime state — definitions can be created at static
    ///         init time and materialized later when the ribbon is ready.</item>
    ///   <item>Supports both reflection-routed buttons (via <see cref="Buttons.UiRouter"/>)
    ///         and simple delegate-based click handlers.</item>
    /// </list>
    ///
    /// <para><b>Example — single-line registration:</b></para>
    /// <code>
    ///   var def = RibbonButtonDefinition.Builder("Reload")
    ///       .Tooltip("Reload the plugin DLL")
    ///       .Icon(Properties.Resources.ReloadIcon)
    ///       .OnClick(() => ReloadService.Execute())
    ///       .Build();
    /// </code>
    /// </summary>
    public class RibbonButtonDefinition
    {
        // ── Properties ──────────────────────────────────────────────────────────

        /// <summary>Display name (also used as the button Name/Id).</summary>
        public string Name { get; }

        /// <summary>Button text shown in the ribbon.</summary>
        public string Text { get; }

        /// <summary>Tooltip shown on hover.</summary>
        public string TooltipText { get; }

        /// <summary>Optional icon bitmap (16×16 recommended for Standard size).</summary>
        public Bitmap Icon { get; }

        /// <summary>
        /// Simple click handler. When set, the button uses a
        /// <see cref="DelegateClickCommandHandler"/> instead of the reflection-based
        /// <see cref="Buttons.GenericClickCommandHandler"/>.
        /// </summary>
        public Action ClickAction { get; }

        /// <summary>
        /// Custom <see cref="ICommand"/> handler. Takes precedence over
        /// <see cref="ClickAction"/> when both are set.
        /// </summary>
        public ICommand CommandHandler { get; }

        /// <summary>
        /// Optional command parameter (e.g. a <see cref="Buttons.UiRouter"/>).
        /// </summary>
        public object CommandParameter { get; }

        /// <summary>
        /// Whether the button is enabled. Default <c>true</c>.
        /// </summary>
        public bool IsEnabled { get; }

        /// <summary>
        /// Whether the button should be large (RibbonItemSize.Large) or
        /// standard (default).
        /// </summary>
        public bool IsLarge { get; }

        // ── Constructor (private — use Builder) ─────────────────────────────────

        private RibbonButtonDefinition(
            string name, string text, string tooltipText,
            Bitmap icon, Action clickAction, ICommand commandHandler,
            object commandParameter, bool isEnabled, bool isLarge)
        {
            Name = name;
            Text = text ?? name;
            TooltipText = tooltipText;
            Icon = icon;
            ClickAction = clickAction;
            CommandHandler = commandHandler;
            CommandParameter = commandParameter;
            IsEnabled = isEnabled;
            IsLarge = isLarge;
        }

        // ── Fluent Builder ──────────────────────────────────────────────────────

        /// <summary>
        /// Start building a new button definition.
        /// </summary>
        /// <param name="name">Button name / display text.</param>
        public static RibbonButtonBuilder Builder(string name) => new RibbonButtonBuilder(name);

        /// <summary>Fluent builder for <see cref="RibbonButtonDefinition"/>.</summary>
        public class RibbonButtonBuilder
        {
            private readonly string _name;
            private string _text;
            private string _tooltip;
            private Bitmap _icon;
            private Action _clickAction;
            private ICommand _commandHandler;
            private object _commandParameter;
            private bool _isEnabled = true;
            private bool _isLarge;

            internal RibbonButtonBuilder(string name) { _name = name; }

            /// <summary>Override the display text (defaults to name).</summary>
            public RibbonButtonBuilder Text(string text) { _text = text; return this; }

            /// <summary>Set the tooltip.</summary>
            public RibbonButtonBuilder Tooltip(string tooltip) { _tooltip = tooltip; return this; }

            /// <summary>Set the icon bitmap.</summary>
            public RibbonButtonBuilder Icon(Bitmap icon) { _icon = icon; return this; }

            /// <summary>Set a simple click handler (no reflection routing needed).</summary>
            public RibbonButtonBuilder OnClick(Action action) { _clickAction = action; return this; }

            /// <summary>Set a custom ICommand handler.</summary>
            public RibbonButtonBuilder Handler(ICommand handler) { _commandHandler = handler; return this; }

            /// <summary>Set a command parameter (e.g. UiRouter).</summary>
            public RibbonButtonBuilder Parameter(object param) { _commandParameter = param; return this; }

            /// <summary>Mark the button as disabled.</summary>
            public RibbonButtonBuilder Disabled() { _isEnabled = false; return this; }

            /// <summary>Use large button size.</summary>
            public RibbonButtonBuilder Large() { _isLarge = true; return this; }

            /// <summary>Build the immutable definition.</summary>
            public RibbonButtonDefinition Build()
            {
                return new RibbonButtonDefinition(
                    _name, _text, _tooltip, _icon, _clickAction,
                    _commandHandler, _commandParameter, _isEnabled, _isLarge);
            }
        }
    }
}
