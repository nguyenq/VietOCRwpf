/// https://www.codeproject.com/Articles/45782/A-WPF-Combo-Box-with-Multiple-Selection

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;

namespace VietOCR
{
    public class DataSource : INotifyPropertyChanged
    {
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        
        #endregion
       
        public ObservableCollection<string> InstalledLanguages
        {
            get;
            set;
        }
        
        private string _selectedLanguage = "English";
        public string SelectedLanguage
        {
            get { return _selectedLanguage; }
            set 
            { 
                _selectedLanguage = value;
                OnPropertyChanged("SelectedLanguage");
            }
        }

        private ObservableCollection<string> _selectedLanguages;
        public ObservableCollection<string> SelectedLanguages
        {
            get
            {
                if (_selectedLanguages == null)
                {
                    _selectedLanguages = new ObservableCollection<string> { "English" };
                    SelectedLanguagesText = WriteSelectedLanguagesString(_selectedLanguages);
                    _selectedLanguages.CollectionChanged +=
                        (s, e) =>
                        {
                            SelectedLanguagesText = WriteSelectedLanguagesString(_selectedLanguages);
                            OnPropertyChanged("SelectedLanguages");
                        };
                }
                return _selectedLanguages;
            }
            set
            {
                _selectedLanguages.Clear();
                foreach (string lang in value)
                {
                    _selectedLanguages.Add(lang);
                }
            }
        }

        string _selectedLanguagesText;
        public string SelectedLanguagesText
        {
            get { return _selectedLanguagesText; }
            set 
            { 
                _selectedLanguagesText = value;
                OnPropertyChanged("SelectedLanguagesText");
            }
        } 

        private static string WriteSelectedLanguagesString(IList<string> list)
        {
            if (list.Count == 0)
                return String.Empty;

            StringBuilder builder = new StringBuilder(list[0]);

            for (int i = 1; i < list.Count; i++)
            {
                builder.Append(", ");
                builder.Append(list[i]);
            }

            return builder.ToString();
        }
    }
}
