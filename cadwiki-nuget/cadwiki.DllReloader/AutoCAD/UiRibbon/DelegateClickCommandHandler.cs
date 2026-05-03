using System;
using System.Windows.Input;

namespace cadwiki.DllReloader.AutoCAD.UiRibbon
{
    /// <summary>
    /// A simple <see cref="ICommand"/> implementation that invokes an <see cref="Action"/>
    /// delegate when executed. This eliminates the need for reflection-based routing
    /// when a button's behavior can be expressed as a simple lambda or method group.
    ///
    /// <para>Used internally by <see cref="RibbonBuilder"/> when materializing
    /// <see cref="RibbonButtonDefinition"/> instances that have a <c>ClickAction</c>.</para>
    ///
    /// <para><b>Example:</b></para>
    /// <code>
    ///   button.CommandHandler = new DelegateClickCommandHandler(() => {
    ///       doc.Editor.WriteMessage("Button clicked!");
    ///   });
    /// </code>
    /// </summary>
    public class DelegateClickCommandHandler : ICommand
    {
        private readonly Action _action;

        /// <summary>
        /// Creates a new handler that wraps the given action.
        /// </summary>
        /// <param name="action">The delegate to invoke on button click.</param>
        public DelegateClickCommandHandler(Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>Always returns true — the button is always enabled.</summary>
        public bool CanExecute(object parameter) => true;

        /// <summary>Not used — required by ICommand interface.</summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>Invokes the wrapped delegate.</summary>
        public void Execute(object parameter)
        {
            try
            {
                _action();
            }
            catch (Exception ex)
            {
                var window = new WpfUi.Templates.WindowAutoCADException(ex);
                window.Show();
            }
        }
    }
}
