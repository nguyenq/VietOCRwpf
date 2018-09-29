using Net.SourceForge.Vietpad.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VietOCR
{
    /// <summary>
    /// Interaction logic for FindReplaceDialog.xaml
    /// </summary>
    public partial class FindReplaceDialog : Window
    {
        // Public events
        public event RoutedEventHandler FindNext;
        public event RoutedEventHandler Replace;
        public event RoutedEventHandler ReplaceAll;
        public event RoutedEventHandler CloseDlg;

        // Properties
        public string FindText
        {
            set { cbFind.Text = value; }
            get { return cbFind.Text; }
        }
        public string ReplaceText
        {
            set { cbReplace.Text = value; }
            get { return cbReplace.Text; }
        }
        public bool MatchCase
        {
            set { chkboxMatchCase.IsChecked = value; }
            get { return chkboxMatchCase.IsChecked.Value; }
        }
        public bool MatchWholeWord
        {
            set { chkboxMatchWholeWord.IsChecked = value; }
            get { return chkboxMatchWholeWord.IsChecked.Value; }
        }
        public bool MatchDiacritics
        {
            set { chkboxMatchDiacritics.IsChecked = value; }
            get { return chkboxMatchDiacritics.IsChecked.Value; }
        }
        public bool MatchRegex
        {
            set { this.chkboxMatchRegex.IsChecked = value; }
            get { return this.chkboxMatchRegex.IsChecked.Value; }
        }
        public bool SearchDown
        {
            set
            {
                if (value)
                    radiobtnSearchDown.IsChecked = true;
                else
                    radiobtnSearchUp.IsChecked = true;
            }
            get { return radiobtnSearchDown.IsChecked.Value; }
        }

        public FindReplaceDialog()
        {
            InitializeComponent();
        }

        private void btnFindNext_Click(object sender, RoutedEventArgs e)
        {
            PopulateComboBox("Find");
            FindNext?.Invoke(this, e);
        }

        private void btnReplace_Click(object sender, RoutedEventArgs e)
        {
            PopulateComboBox("Find");
            PopulateComboBox("Replace");
            Replace?.Invoke(this, e);
        }

        private void btnReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            PopulateComboBox("Find");
            PopulateComboBox("Replace");
            ReplaceAll?.Invoke(this, e);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            CloseDlg?.Invoke(this, e);
        }

        /**
         * Populates the combobox with entries from the corresponding text field
         */
        void PopulateComboBox(string button)
        {
            string text;
            ComboBox comboBox;

            if (button == "Find")
            {
                text = this.FindText;
                comboBox = this.cbFind;
            }
            else
            {
                text = this.ReplaceText;
                comboBox = this.cbReplace;
            }
            if (text == String.Empty) return;

            if (!comboBox.Items.Contains(text))
            {
                comboBox.Items.Insert(0, text);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            btnFindNext.IsEnabled =
                btnReplace.IsEnabled = cbFind.Text.Length > 0;
            btnReplaceAll.IsEnabled = cbFind.Text.Length > 0 && !(MatchRegex && !MatchDiacritics);
            cbFind.Focus();
        }

        private void cbFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnFindNext.IsEnabled =
                btnReplace.IsEnabled = cbFind.Text.Length > 0;
            btnReplaceAll.IsEnabled = cbFind.Text.Length > 0 && !(MatchRegex && !MatchDiacritics);
        }

        private void option_Changed(object sender, RoutedEventArgs e)
        {
            if (sender == this.chkboxMatchDiacritics || sender == this.chkboxMatchRegex)
            {
                btnReplaceAll.IsEnabled = !this.chkboxMatchRegex.IsChecked.Value || this.chkboxMatchDiacritics.IsChecked.Value;
            }
            if (sender == this.chkboxMatchRegex)
            {
                this.chkboxMatchWholeWord.IsEnabled = !this.chkboxMatchRegex.IsChecked.Value;
            }
            this.btnFindNext.Focus();
        }
    }
}
