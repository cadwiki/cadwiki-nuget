using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;


namespace cadwiki.WpfLibrary.Controls
{
    public class BindableMultiSelectDataGrid : DataGrid // TODO DataGrid is no longer supported. Use DataGridView instead. For more details see https://docs.microsoft.com/en-us/dotnet/core/compatibility/winforms#removed-controls
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList), typeof(BindableMultiSelectDataGrid), new PropertyMetadata(default(IList)));

        public new IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { throw new Exception("This property is read-only. To bind to it you must use 'Mode=OneWayToSource'."); }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            SetValue(SelectedItemsProperty, base.SelectedItems);
        }
    }
}
