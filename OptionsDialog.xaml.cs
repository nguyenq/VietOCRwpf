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

        public bool WatchDeskewEnabled
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

        public bool ReplaceHyphensEnabled
        {
            get;
            set;
        }

        public bool RemoveHyphensEnabled
        {
            get;
            set;
        }

        public string OutputFormat
        {
            get { return this.comboBoxOutputFormat.SelectedItem.ToString(); }
            set
            {
                this.comboBoxOutputFormat.SelectedItem = value;
                if (this.comboBoxOutputFormat.SelectedIndex == -1) this.comboBoxOutputFormat.SelectedIndex = 0;
            }
        }

        public OptionsDialog()
        {
            InitializeComponent();

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
            this.checkBoxReplaceHyphens.IsChecked = ReplaceHyphensEnabled;
            this.checkBoxRemoveHyphens.IsChecked = RemoveHyphensEnabled;
            this.checkBoxDeskew.IsChecked = WatchDeskewEnabled;

            //this.toolTip1.SetToolTip(this.btnWatch, Properties.Resources.Browse);
            //this.toolTip1.SetToolTip(this.btnOutput, Properties.Resources.Browse);
            //this.toolTip1.SetToolTip(this.btnDangAmbigs, Properties.Resources.Browse);
        }

        protected override void OnClosed(EventArgs ea)
        {
            base.OnClosed(ea);

            WatchEnabled = this.checkBoxWatch.IsChecked.Value;
            DangAmbigsEnabled = this.checkBoxDangAmbigs.IsChecked.Value;
            ReplaceHyphensEnabled = this.checkBoxReplaceHyphens.IsChecked.Value;
            RemoveHyphensEnabled = this.checkBoxRemoveHyphens.IsChecked.Value;
            WatchDeskewEnabled = this.checkBoxDeskew.IsChecked.Value;
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

        private void comboBoxOutputFormat_MouseEnter(object sender, MouseEventArgs e)
        {
            string val = this.comboBoxOutputFormat.SelectedItem.ToString();
            switch (val)
            {
                case "text+":
                    val = "Text with postprocessing";
                    break;
                case "text":
                    val = "Text with no postprocessing";
                    break;
                case "pdf":
                    val = "PDF";
                    break;
                case "hocr":
                    val = "hOCR";
                    break;
                default:
                    val = null;
                    break;
            }

            this.comboBoxOutputFormat.ToolTip = val;
        }

    }
}
