﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ComboBox = System.Windows.Controls.ComboBox;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace cadwiki.WpfLibrary.Controls
{
    /// <summary>
    /// Interaction logic for KeyPressSearchComboBox.xaml
    /// </summary>
    public partial class KeyPressSearchComboBox : UserControl
    {
        public KeyPressSearchComboBox()
        {
            InitializeComponent();
            ItemsFiltered = ItemsOriginal;
        }

        public static readonly DependencyProperty ItemsOriginalProperty =
            DependencyProperty.Register("ItemsOriginal", typeof(ObservableCollection<string>), typeof(KeyPressSearchComboBox), new PropertyMetadata(new ObservableCollection<string>()));

        public static readonly DependencyProperty ItemsFilteredProperty =
            DependencyProperty.Register("ItemsFiltered", typeof(ObservableCollection<string>), typeof(KeyPressSearchComboBox), new PropertyMetadata(new ObservableCollection<string>()));

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(KeyPressSearchComboBox), new PropertyMetadata(string.Empty, OnSearchTextChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(string), typeof(KeyPressSearchComboBox), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(KeyPressSearchComboBox), new PropertyMetadata(false));

        public static readonly DependencyProperty IsDropDownEnabledProperty =
            DependencyProperty.Register("IsDropDownEnabled", typeof(bool), typeof(KeyPressSearchComboBox), new PropertyMetadata(false));

        public ObservableCollection<string> ItemsOriginal
        {
            get { return (ObservableCollection<string>)GetValue(ItemsOriginalProperty); }
            set { 
                SetValue(ItemsOriginalProperty, value); 
            }
        }

        public ObservableCollection<string> ItemsFiltered
        {
            get { return (ObservableCollection<string>)GetValue(ItemsFilteredProperty); }
            set { 
                SetValue(ItemsFilteredProperty, value); 
            }
        }

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { 
                SetValue(SearchTextProperty, value);

                //user backspaced current text
                if (value?.Length > SearchText?.Length)
                {
                    //clear selected item
                    SelectedItem = null;
                }

                //empty string
                if (String.IsNullOrEmpty(SearchText))
                {
                    SelectedItem = null;
                    ItemsFiltered = ItemsOriginal;
                    IsDropDownOpen = false;
                }
            }
        }

        public string SelectedItem
        {
            get { return (string)GetValue(SelectedItemProperty); }
            set { 
                SetValue(SelectedItemProperty, value);
                if (SelectedItem != null)
                {
                    IsDropDownOpen = false;
                }
            }
        }

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { 
                if (IsDropDownEnabled)
                {
                    SetValue(IsDropDownOpenProperty, value);
                    ComboBox comboBox = this.ComboBox;
                    comboBox.ApplyTemplate();
                    TextBox editableTextBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
                    if (editableTextBox != null)
                    {
                        editableTextBox.Select(editableTextBox.Text.Length, 0);
                    }
                }
            }
        }

        public bool IsDropDownEnabled
        {
            get { return (bool)GetValue(IsDropDownEnabledProperty); }
            set
            {
                SetValue(IsDropDownEnabledProperty, value);
            }
        }

        private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var control = d as KeyPressSearchComboBox;
                control?.ExecuteFilter(control, e);
            }
            catch (Exception ex)
            {
                cadwiki.WpfLibrary.Globals.Ex.Log(ex);
            }
        }

        private void ExecuteFilter(KeyPressSearchComboBox control, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (ItemsOriginal != null)
                {
                    if (!String.IsNullOrEmpty(SearchText))
                    {
                        string searchText = SearchText.ToLower();
                        var filteredItems = ItemsOriginal.Where(item => item.ToLower().Contains(searchText)).ToList();
                        control.ItemsFiltered = new ObservableCollection<string>(filteredItems);
                        control.IsDropDownOpen = true;
                    }
                    else
                    {
                        string searchText = SearchText.ToLower();
                        var filteredItems = ItemsOriginal;
                        control.ItemsFiltered = new ObservableCollection<string>(filteredItems);
                        control.IsDropDownOpen = true;
                    }
                }
                ClearSelectedItemOnBackspace(control, e);
                //DeselectHighlightedCharactersOnBackspace(control, e);
            }
            catch (Exception ex)
            {
                cadwiki.WpfLibrary.Globals.Ex.Log(ex);
            }
        }

        private void ClearSelectedItemOnBackspace(KeyPressSearchComboBox control, DependencyPropertyChangedEventArgs e)
        {
            var oldStr = e.OldValue as string;
            var newStr = e.NewValue as string;
            if (oldStr.Length > newStr.Length)
            {
                ComboBox comboBox = control.ComboBox;
                comboBox.ApplyTemplate();
                comboBox.SelectedItem = null;
                SelectedItem = null;
            }
        }

        private static void DeselectHighlightedCharactersOnBackspace(KeyPressSearchComboBox control, DependencyPropertyChangedEventArgs e)
        {
            var oldStr = e.OldValue as string;
            var newStr = e.NewValue as string;
            if (oldStr.Length > newStr.Length)
            {
                ComboBox comboBox = control.ComboBox;
                comboBox.ApplyTemplate();
                TextBox editableTextBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
                if (editableTextBox != null)
                {
                    editableTextBox.Select(editableTextBox.Text.Length, 0);
                }
            }
        }

        private void MyComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Handle ComboBox loaded event to customize editable TextBox
                ComboBox comboBox = sender as ComboBox;
                if (comboBox != null)
                {
                    comboBox.ApplyTemplate();
                    TextBox editableTextBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
                    if (editableTextBox != null)
                    {
                        editableTextBox.Background = Brushes.Transparent;
                        editableTextBox.Foreground = Brushes.Black; // Adjust foreground color as needed
                    }
                }
            }
            catch (Exception ex)
            {
                cadwiki.WpfLibrary.Globals.Ex.Log(ex);
            }
        }

        private void MyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Handle ComboBox selection changed event
                ComboBox comboBox = sender as ComboBox;
                if (comboBox != null)
                {
                    //comboBox.ApplyTemplate();
                    TextBox editableTextBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
                    if (editableTextBox != null)
                    {
                        if (comboBox.IsEditable && comboBox.SelectedItem == null)
                        {
                            //editableTextBox.Text = comboBox.Text;
                        }
                        else
                        {
                            editableTextBox.Text = comboBox.SelectedItem?.ToString();
                            editableTextBox.Select(editableTextBox.Text.Length, 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                cadwiki.WpfLibrary.Globals.Ex.Log(ex);
            }
        }

    }

    public class FixedWidthComboBox : ComboBox
    {
        protected override Size MeasureOverride(Size constraint)
        {
            return base.MeasureOverride(new Size(ActualWidth, constraint.Height));
        }
    }
}
