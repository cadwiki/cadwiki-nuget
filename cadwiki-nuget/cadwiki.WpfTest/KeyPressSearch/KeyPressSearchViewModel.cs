using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace cadwiki.WpfTest.KeyPressSearch
{
    public class KeyPressSearchViewModel : Screen
    {
        private AlphaAutoSortObservableCollection<string> _listFilteredStrings = new AlphaAutoSortObservableCollection<string>();
        public AlphaAutoSortObservableCollection<string> ListFilteredStrings
        {
            get { return _listFilteredStrings; }
            set
            {
                _listFilteredStrings = value;
                NotifyOfPropertyChange(nameof(ListFilteredStrings));
            }
        }

        private AlphaAutoSortObservableCollection<string> _listOriginalStrings = new AlphaAutoSortObservableCollection<string>();
        public AlphaAutoSortObservableCollection<string> ListOriginalStrings
        {
            get { return _listOriginalStrings; }
            set
            {
                _listOriginalStrings = value;
                NotifyOfPropertyChange(nameof(ListOriginalStrings));
            }
        }


        private string _selectedString;
        public string SelectedString
        {
            get { return _selectedString; }
            set
            {
                _selectedString = value;
                NotifyOfPropertyChange(nameof(SelectedString));
            }
        }

        private Boolean _isScreenOpen;
        public Boolean IsScreenOpen
        {
            get { return _isScreenOpen; }
            set
            {
                _isScreenOpen = value;
                NotifyOfPropertyChange(nameof(IsScreenOpen));
            }
        }

        public KeyPressSearchViewModel()
        {
            SelectedString = "";
            var strings = new List<string>() { "a string", "b string", "test" };
            ListOriginalStrings.ClearAndAddRange(strings);
            ListFilteredStrings.ClearAndAddRange(strings);
            IsScreenOpen = true;
        }

    }
}
