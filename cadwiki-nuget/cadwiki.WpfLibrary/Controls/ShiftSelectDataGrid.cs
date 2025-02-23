using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;


namespace cadwiki.WpfLibrary.Controls
{
    public class ShiftSelectableDataGrid : DataGrid
    {
        private int _lastSelectedIndex = -1;

        public ShiftSelectableDataGrid()
        {
            SelectionMode = DataGridSelectionMode.Extended;
            SelectionUnit = DataGridSelectionUnit.FullRow;
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (e.OriginalSource is FrameworkElement fe && fe.DataContext != null)
            {
                var row = ItemContainerGenerator.ContainerFromItem(fe.DataContext) as DataGridRow;
                if (row != null)
                {
                    int currentIndex = ItemContainerGenerator.IndexFromContainer(row);

                    if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && 
                        _lastSelectedIndex >= 0)
                    {
                        SelectRange(_lastSelectedIndex, currentIndex);
                    }
                    else
                    {
                        _lastSelectedIndex = currentIndex;
                    }
                }
            }
        }

        private void SelectRange(int start, int end)
        {
            if (start > end)
            {
                int temp = start;
                start = end;
                end = temp;
            }

            SelectedItems.Clear();
            for (int i = start; i <= end; i++)
            {
                SelectedItems.Add(Items[i]);
            }
        }
    }
}
