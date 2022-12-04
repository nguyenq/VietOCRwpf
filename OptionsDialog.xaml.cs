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

        public string CurLangCode
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
                        list.Add(((TextBlock)item.Header).Text);
                    }
                }
                return string.Join(",", list);
            }

            set
            {
                string[] list = value.Split(',');
                foreach (MenuItem item in menuOutputFormat.Items)
                {
                    if (list.Contains(((TextBlock)item.Header).Text))
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
                TextBlock tb = new TextBlock { Text = name };
                MenuItem item = new MenuItem { Header = tb };
                item.IsCheckable = true;
                item.StaysOpenOnClick = true;
                item.Click += Item_Click;
                this.menuOutputFormat.Items.Add(item);
            }

            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new OpenFileDialog();
        }

        /// <summary>
        /// Allows only one PDF option selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Item_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            if (menuItem.IsChecked)
            {
                string label = ((TextBlock)menuItem.Header).Text;

                if (Tesseract.RenderedFormat.PDF.ToString() == label)
                {
                    MenuItem menuItemPDF_TEXTONLY = this.menuOutputFormat.Items.Cast<MenuItem>().Single(item => ((TextBlock)item.Header).Text == Tesseract.RenderedFormat.PDF_TEXTONLY.ToString());
                    menuItemPDF_TEXTONLY.IsChecked = false;
                }
                else if (Tesseract.RenderedFormat.PDF_TEXTONLY.ToString() == label)
                {
                    MenuItem menuItemPDF = this.menuOutputFormat.Items.Cast<MenuItem>().Single(item => ((TextBlock)item.Header).Text == Tesseract.RenderedFormat.PDF.ToString());
                    menuItemPDF.IsChecked = false;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBoxWatch.Text = WatchFolder;
            this.textBoxOutput.Text = OutputFolder;
            this.checkBoxWatch.IsChecked = WatchEnabled;
            this.textBoxDangAmbigs.Text = ProcessingOptions.DangAmbigsPath;
            this.checkBoxDangAmbigs.IsChecked = ProcessingOptions.DangAmbigsEnabled;
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
            ProcessingOptions.DangAmbigsEnabled = this.checkBoxDangAmbigs.IsChecked.Value;
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
            this.openFileDialog1.InitialDirectory = ProcessingOptions.DangAmbigsPath;

            Nullable<bool> result = openFileDialog1.ShowDialog();

            if (result.HasValue && result.Value)
            {
                ProcessingOptions.DangAmbigsPath = System.IO.Path.GetDirectoryName(this.openFileDialog1.FileName);
                this.textBoxDangAmbigs.Text = ProcessingOptions.DangAmbigsPath;
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.checkBoxWatch.IsChecked.Value && OutputFormat.Length == 0)
            {
                MessageBox.Show(this, Properties.Resources.Please_select_output_format, this.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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
        }
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            BtnArrowDown.Visibility = Visibility.Collapsed;
            BtnArrowUp.Visibility = Visibility.Visible;
        }
        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            srClicked = false;
            BtnArrowDown.Visibility = Visibility.Visible;
            BtnArrowUp.Visibility = Visibility.Collapsed;
            Keyboard.ClearFocus();
        }
    }
}
