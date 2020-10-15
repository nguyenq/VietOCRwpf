using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VietOCR.NET.Utilities;

namespace VietOCR
{
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private OpenFileDialog openFileDialog1;

        public string WatchFolder
        {
            get;
            set;
        }

        public string OutputFolder
        {
            get;
            set;
        }

        public bool WatchEnabled
        {
            get;
            set;
        }

        public string DangAmbigsPath
        {
            get;
            set;
        }

        public string CurLangCode
        {
            get;
            set;
        }

        public bool DangAmbigsEnabled
        {
            get;
            set;
        }

        public ProcessingOptions ProcessingOptions { get; set; }

        public string OutputFormat
        {
            get
            {
                List<string> list = new List<string>();
                foreach (MenuItem item in menuOutputFormat.Items)
                {
                    if (item.IsChecked)
                    {
                        list.Add(item.Header.ToString());
                    }
                }
                return string.Join(",", list);
            }

            set
            {
                string[] list = value.Split(',');
                foreach (MenuItem item in menuOutputFormat.Items)
                {
                    if (list.Contains(item.Header.ToString()))
                    {
                        item.IsChecked = true;
                    }
                }
            }
        }

        public int SelectedTab
        {
            set
            {
                this.tabControl.SelectedIndex = value;
            }
        }

        public OptionsDialog()
        {
            InitializeComponent();

            foreach (string name in Enum.GetNames(typeof(Tesseract.RenderedFormat)))
            {
                MenuItem item = new MenuItem { Header = name };
                item.IsCheckable = true;
                item.Click += srMenuItem_Click;
                this.menuOutputFormat.Items.Add(item);
            }

            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new OpenFileDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBoxWatch.Text = WatchFolder;
            this.textBoxOutput.Text = OutputFolder;
            this.checkBoxWatch.IsChecked = WatchEnabled;
            this.textBoxDangAmbigs.Text = DangAmbigsPath;
            this.checkBoxDangAmbigs.IsChecked = DangAmbigsEnabled;
            this.checkBoxReplaceHyphens.IsChecked = ProcessingOptions.ReplaceHyphens;
            this.checkBoxRemoveHyphens.IsChecked = ProcessingOptions.RemoveHyphens;
            this.checkBoxDeskew.IsChecked = ProcessingOptions.Deskew;
            this.checkBoxCorrectLetterCases.IsChecked = ProcessingOptions.CorrectLetterCases;
            this.checkBoxPostProcessing.IsChecked = ProcessingOptions.PostProcessing;
            this.checkBoxRemoveLines.IsChecked = ProcessingOptions.RemoveLines;
            this.checkBoxRemoveLineBreaks.IsChecked = ProcessingOptions.RemoveLineBreaks;

            //this.toolTip1.SetToolTip(this.btnWatch, Properties.Resources.Browse);
            //this.toolTip1.SetToolTip(this.btnOutput, Properties.Resources.Browse);
            //this.toolTip1.SetToolTip(this.btnDangAmbigs, Properties.Resources.Browse);
        }

        protected override void OnClosed(EventArgs ea)
        {
            base.OnClosed(ea);

            WatchEnabled = this.checkBoxWatch.IsChecked.Value;
            DangAmbigsEnabled = this.checkBoxDangAmbigs.IsChecked.Value;
            ProcessingOptions.ReplaceHyphens = this.checkBoxReplaceHyphens.IsChecked.Value;
            ProcessingOptions.RemoveHyphens = this.checkBoxRemoveHyphens.IsChecked.Value;
            ProcessingOptions.Deskew = this.checkBoxDeskew.IsChecked.Value;
            ProcessingOptions.CorrectLetterCases = this.checkBoxCorrectLetterCases.IsChecked.Value; 
            ProcessingOptions.PostProcessing = this.checkBoxPostProcessing.IsChecked.Value;
            ProcessingOptions.RemoveLines = this.checkBoxRemoveLines.IsChecked.Value;
            ProcessingOptions.RemoveLineBreaks = this.checkBoxRemoveLineBreaks.IsChecked.Value;
        }

        public virtual void ChangeUILanguage(string locale)
        {
            FormLocalizer localizer = new FormLocalizer(this, typeof(OptionsDialog));
            localizer.ApplyCulture(new CultureInfo(locale));
        }

        private void btnWatch_Click(object sender, RoutedEventArgs e)
        {
            this.folderBrowserDialog1.Description = "Set Watch Folder.";
            this.folderBrowserDialog1.SelectedPath = WatchFolder;

            if (this.folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                WatchFolder = this.folderBrowserDialog1.SelectedPath;
                this.textBoxWatch.Text = WatchFolder;
            }
        }

        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {
            this.folderBrowserDialog1.Description = "Set Output Folder.";
            this.folderBrowserDialog1.SelectedPath = OutputFolder;

            if (this.folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OutputFolder = this.folderBrowserDialog1.SelectedPath;
                this.textBoxOutput.Text = OutputFolder;
            }
        }

        private void btnDangAmbigs_Click(object sender, RoutedEventArgs e)
        {
            this.openFileDialog1.Title = String.Format("Set Path to {0}.DangAmbigs.txt", CurLangCode);
            this.openFileDialog1.InitialDirectory = DangAmbigsPath;

            Nullable<bool> result = openFileDialog1.ShowDialog();

            if (result.HasValue && result.Value)
            {
                DangAmbigsPath = System.IO.Path.GetDirectoryName(this.openFileDialog1.FileName);
                this.textBoxDangAmbigs.Text = DangAmbigsPath;
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        bool srClicked;

        private void buttonOutputFormat_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            btn.ContextMenu.PlacementTarget = btn;
            btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            btn.ContextMenu.IsOpen = !srClicked;
            srClicked ^= true;
            BtnArrowDown.Visibility = srClicked ? Visibility.Collapsed : Visibility.Visible;
            BtnArrowUp.Visibility = srClicked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            srClicked = false;
            BtnArrowDown.Visibility = srClicked ? Visibility.Collapsed : Visibility.Visible;
            BtnArrowUp.Visibility = srClicked ? Visibility.Visible : Visibility.Collapsed;
            Keyboard.ClearFocus();
        }

        private void srMenuItem_Click(object sender, RoutedEventArgs e)
        {
            srClicked = false;
            Keyboard.ClearFocus();
        }
    }
}
