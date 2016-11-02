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
    /// Interaction logic for BulkDialog.xaml
    /// </summary>
    public partial class BulkDialog : Window
    {
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private string inputFolder;

        public string InputFolder
        {
            get { return inputFolder; }
            set { inputFolder = value; }
        }
        private string outputFolder;

        public string OutputFolder
        {
            get { return outputFolder; }
            set { outputFolder = value; }
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

        public BulkDialog()
        {
            InitializeComponent();

            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBoxInput.Text = inputFolder;
            this.textBoxOutput.Text = outputFolder;
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

            inputFolder = this.textBoxInput.Text;
            outputFolder = this.textBoxOutput.Text;
        }

        /// <summary>
        /// Changes localized text and messages
        /// </summary>
        /// <param name="locale"></param>
        public virtual void ChangeUILanguage(string locale)
        {
            FormLocalizer localizer = new FormLocalizer(this, typeof(BulkDialog));
            localizer.ApplyCulture(new CultureInfo(locale));
        }

        private void buttonRun_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            this.folderBrowserDialog1.Description = "Set Input Image Folder.";
            this.folderBrowserDialog1.SelectedPath = inputFolder;

            if (this.folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                inputFolder = this.folderBrowserDialog1.SelectedPath;
                this.textBoxInput.Text = inputFolder;
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
