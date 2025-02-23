﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace cadwiki.WpfTest.ShiftSelectDataGrid
{
    public class ShiftSelectDataGridViewModel : Screen
    {
        private ObservableCollection<string> _items;

        public ObservableCollection<string> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public ShiftSelectDataGridViewModel()
        {
            Items = new ObservableCollection<string>();
            for (int i = 1; i <= 20; i++)
            {
                Items.Add($"Item {i}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
