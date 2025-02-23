using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls.Primitives;
using System;


namespace cadwiki.WpfLibrary.Controls
{
    public class ShiftSelectableDataGrid : DataGrid
    {
        private int _lastSelectedIndex = -1;

        public ShiftSelectableDataGrid()
        {
            SelectionMode = DataGridSelectionMode.Extended;
            SelectionUnit = DataGridSelectionUnit.FullRow;
            // Subscribe to column header click event
            //this.AddHandler(DataGridColumnHeader.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnColumnHeaderClick_ToggleSelectedCheckBoxes), true);
            this.AddHandler(DataGridCell.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnCheckBoxRowClick_ToggleSelectedCheckBoxes), true);
        }

        private void OnCheckBoxRowClick_ToggleSelectedCheckBoxes(object sender, MouseButtonEventArgs e)
        {
            if (sender is ShiftSelectableDataGrid dg)
            {
                if (e.OriginalSource is System.Windows.Controls.Border border)
                {
                    var currentItem = dg.CurrentCell.Item;
                    if (currentItem is SelectableItem currenSelectableItem)
                    {
                        var toggleTo = !currenSelectableItem.IsSelected;
                        foreach (var item in this.SelectedItems)
                        {
                            if (item is SelectableItem selectableItem)
                            {
                                selectableItem.IsSelected = toggleTo;
                            }
                        }
                        e.Handled = true;
                    }
                }
            }
        }

        private void OnColumnHeaderClick_ToggleSelectedCheckBoxes(object sender, MouseButtonEventArgs e)
        {
            if (sender is ShiftSelectableDataGrid dg)
            {
                if (e.OriginalSource is TextBlock tb)
                {
                    var text = tb.Text;
                    if (text == "Select")
                    {
                        foreach (var item in this.SelectedItems)
                        {
                            if (item is SelectableItem selectableItem)
                            {
                                selectableItem.IsSelected = !selectableItem.IsSelected;
                            }
                        }
                        e.Handled = true;
                    }
                }
            }
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

    public class SelectableItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _name;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
