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

        private string watchFolder;

        public string WatchFolder
        {
            get { return watchFolder; }
            set { watchFolder = value; }
        }
        private string outputFolder;

        public string OutputFolder
        {
            get { return outputFolder; }
            set { outputFolder = value; }
        }
        private bool watchEnabled;

        public bool WatchEnabled
        {
            get { return watchEnabled; }
            set { watchEnabled = value; }
        }

        private string dangAmbigsPath;

        public string DangAmbigsPath
        {
            get { return dangAmbigsPath; }
            set { dangAmbigsPath = value; }
        }

        private string curLangCode;

        public string CurLangCode
        {
            get { return curLangCode; }
            set { curLangCode = value; }
        }

        private bool dangAmbigsEnabled;

        public bool DangAmbigsEnabled
        {
            get { return dangAmbigsEnabled; }
            set { dangAmbigsEnabled = value; }
        }

        private bool replaceHyphensEnabled;
        public bool ReplaceHyphensEnabled
        {
            get { return replaceHyphensEnabled; }
            set { replaceHyphensEnabled = value; }
        }

        private bool removeHyphensEnabled;
        public bool RemoveHyphensEnabled
        {
            get { return removeHyphensEnabled; }
            set { removeHyphensEnabled = value; }
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
            this.textBoxWatch.Text = watchFolder;
            this.textBoxOutput.Text = outputFolder;
            this.checkBoxWatch.IsChecked = watchEnabled;
            this.textBoxDangAmbigs.Text = dangAmbigsPath;
            this.checkBoxDangAmbigs.IsChecked = dangAmbigsEnabled;
            this.checkBoxReplaceHyphens.IsChecked = replaceHyphensEnabled;
            this.checkBoxRemoveHyphens.IsChecked = removeHyphensEnabled;

            //this.toolTip1.SetToolTip(this.btnWatch, Properties.Resources.Browse);
            //this.toolTip1.SetToolTip(this.btnOutput, Properties.Resources.Browse);
            //this.toolTip1.SetToolTip(this.btnDangAmbigs, Properties.Resources.Browse);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        protected override void OnClosed(EventArgs ea)
        {
            base.OnClosed(ea);

            watchFolder = this.textBoxWatch.Text;
            outputFolder = this.textBoxOutput.Text;
            watchEnabled = this.checkBoxWatch.IsChecked.Value;
            dangAmbigsPath = this.textBoxDangAmbigs.Text;
            dangAmbigsEnabled = this.checkBoxDangAmbigs.IsChecked.Value;
            replaceHyphensEnabled = this.checkBoxReplaceHyphens.IsChecked.Value;
            removeHyphensEnabled = this.checkBoxRemoveHyphens.IsChecked.Value;
        }

        public virtual void ChangeUILanguage(string locale)
        {
            FormLocalizer localizer = new FormLocalizer(this, typeof(OptionsDialog));
            localizer.ApplyCulture(new CultureInfo(locale));
        }

        private void btnWatch_Click(object sender, RoutedEventArgs e)
        {
            this.folderBrowserDialog1.Description = "Set Watch Folder.";
            this.folderBrowserDialog1.SelectedPath = watchFolder;

            if (this.folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                watchFolder = this.folderBrowserDialog1.SelectedPath;
                this.textBoxWatch.Text = watchFolder;
            }
        }

        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {
            this.folderBrowserDialog1.Description = "Set Output Folder.";
            this.folderBrowserDialog1.SelectedPath = outputFolder;

            if (this.folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                outputFolder = this.folderBrowserDialog1.SelectedPath;
                this.textBoxOutput.Text = outputFolder;
            }
        }

        private void btnDangAmbigs_Click(object sender, RoutedEventArgs e)
        {
            this.openFileDialog1.Title = String.Format("Set Path to {0}.DangAmbigs.txt", curLangCode);
            this.openFileDialog1.InitialDirectory = dangAmbigsPath;

            Nullable<bool> result = openFileDialog1.ShowDialog();

            if (result.HasValue && result.Value)
            {
                dangAmbigsPath = System.IO.Path.GetDirectoryName(this.openFileDialog1.FileName);
                this.textBoxDangAmbigs.Text = dangAmbigsPath;
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
