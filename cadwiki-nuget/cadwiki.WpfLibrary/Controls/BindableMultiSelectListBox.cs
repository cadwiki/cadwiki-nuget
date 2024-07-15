using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace cadwiki.WpfLibrary.Controls
{
    public class BindableMultiSelectListBox : ListBox
    {
        public static new DependencyProperty SelectedItemsProperty =
                    DependencyProperty.Register("SelectedItems", typeof(IList), typeof(BindableMultiSelectListBox), new PropertyMetadata(default(IList)));

        public new IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set
            {
                //throw new Exception("This property is read-only. To bind to it you must use 'Mode=OneWayToSource'.");
                SetValue(SelectedItemsProperty, value);
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            SetValue(SelectedItemsProperty, base.SelectedItems);
        }
    }
}
