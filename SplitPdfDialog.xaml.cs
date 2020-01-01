using Microsoft.Win32;
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
    /// Interaction logic for SplitPdfDialog.xaml
    /// </summary>
    public partial class SplitPdfDialog : Window
    {
        SplitPdfArgs args;
        string pdfFolder = null;

        internal SplitPdfArgs Args
        {
            get { return args; }
        }

        public SplitPdfDialog()
        {
            InitializeComponent();

            disableBoxes(!this.radioButtonPages.IsChecked.Value);
            this.textBoxNumOfPages.IsEnabled = false;
            this.textBoxNumOfPages.Text = "20";
            //this.toolTip1.SetToolTip(this.buttonBrowseInput, Properties.Resources.Browse);
            //this.toolTip1.SetToolTip(this.buttonBrowseOutput, Properties.Resources.Browse);
        }

        private void buttonBrowseInput_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = Properties.Resources.Open;
            dialog.InitialDirectory = pdfFolder;
            dialog.Filter = "PDF (*.pdf)|*.pdf";
            dialog.RestoreDirectory = true;

            Nullable<bool> result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                this.textBoxInput.Text = dialog.FileName;
                pdfFolder = System.IO.Path.GetDirectoryName(dialog.FileName);
            }
        }

        private void buttonBrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = Properties.Resources.Save;
            dialog.InitialDirectory = pdfFolder;
            dialog.Filter = "PDF (*.pdf)|*.pdf";
            dialog.RestoreDirectory = true;

            Nullable<bool> result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                this.textBoxOutput.Text = dialog.FileName;

                if (!this.textBoxOutput.Text.EndsWith(".pdf"))
                {
                    this.textBoxOutput.Text += ".pdf"; // seems not needed
                }
            }
        }

        private void buttonSplit_Click(object sender, RoutedEventArgs e)
        {
            SplitPdfArgs args = new SplitPdfArgs();
            args.InputFilename = this.textBoxInput.Text;
            args.OutputFilename = this.textBoxOutput.Text;
            args.FromPage = this.textBoxFrom.Text;
            args.ToPage = this.textBoxTo.Text;
            args.NumOfPages = this.textBoxNumOfPages.Text;
            args.Pages = this.radioButtonPages.IsChecked.Value;

            if (args.InputFilename.Length > 0 && args.OutputFilename.Length > 0 &&
                ((this.radioButtonPages.IsChecked.Value && args.FromPage.Length > 0) ||
                (this.radioButtonFiles.IsChecked.Value && args.NumOfPages.Length > 0)))
            {
                Regex regexNums = new Regex(@"^\d+$");

                if ((this.radioButtonPages.IsChecked.Value && regexNums.IsMatch(args.FromPage) &&
                    (args.ToPage.Length > 0 ? regexNums.IsMatch(args.ToPage) : true)) ||
                    (this.radioButtonFiles.IsChecked.Value && regexNums.IsMatch(args.NumOfPages)))
                {
                    this.args = args;
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show(this, "Input invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(this, "Input incomplete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void disableBoxes(bool enabled)
        {
            this.textBoxNumOfPages.IsEnabled = enabled;
            this.textBoxFrom.IsEnabled = !enabled;
            this.textBoxTo.IsEnabled = !enabled;
        }

        private void radioButtonPages_Checked(object sender, RoutedEventArgs e)
        {
            disableBoxes(false);
        }

        private void radioButtonFiles_Checked(object sender, RoutedEventArgs e)
        {
            disableBoxes(true);
        }
    }
}
