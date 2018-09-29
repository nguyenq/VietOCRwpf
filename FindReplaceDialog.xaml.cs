// Modified by Quan Nguyen for VietOCR.NET
// Version: 2.0, 28 September 2018
// See: http://vietocr.sourceforge.net
// Change: - Fixed a bug on Search direction, 18 Feb 2003
// 

//------------------------------------------------
// FindReplaceDialog.cs © 2001 by Charles Petzold
//------------------------------------------------

using Net.SourceForge.Vietpad.InputMethod;
using System;
using System.Windows;
using System.Windows.Controls;

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
        public bool? MatchWholeWord
        {
            set { chkboxMatchWholeWord.IsChecked = value; }
            get { return chkboxMatchWholeWord.IsEnabled ? chkboxMatchWholeWord.IsChecked : null; }
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
            // integrate Viet Input Method
            new VietKeyHandler(this.cbFind);
            new VietKeyHandler(this.cbReplace);

            cbFind.Focus();
        }

        private void cbFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnFindNext.IsEnabled = btnReplace.IsEnabled = cbFind.Text.Length > 0;
            btnReplaceAll.IsEnabled = cbFind.Text.Length > 0 && !(MatchRegex && !MatchDiacritics);
        }

        private void option_Changed(object sender, RoutedEventArgs e)
        {
            if (sender == this.chkboxMatchDiacritics || sender == this.chkboxMatchRegex)
            {
                btnReplaceAll.IsEnabled = cbFind.Text.Length > 0 && (!MatchRegex || MatchDiacritics);
            }
            if (sender == this.chkboxMatchRegex)
            {
                this.chkboxMatchWholeWord.IsEnabled = !MatchRegex;
            }
            this.btnFindNext.Focus();
        }
    }
}
