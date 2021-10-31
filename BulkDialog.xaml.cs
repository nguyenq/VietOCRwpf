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

        public string InputFolder
        {
            get;
            set;
        }

        public string OutputFolder
        {
            get;
            set;
        }

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

        public BulkDialog()
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
            this.textBoxInput.Text = InputFolder;
            this.textBoxOutput.Text = OutputFolder;
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
            if (OutputFormat.Length == 0)
            {
                MessageBox.Show(this, Properties.Resources.Please_select_output_format, this.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.DialogResult = true;
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            this.folderBrowserDialog1.Description = "Set Input Image Folder.";
            this.folderBrowserDialog1.SelectedPath = InputFolder;

            if (this.folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InputFolder = this.folderBrowserDialog1.SelectedPath;
                this.textBoxInput.Text = InputFolder;
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

        private void buttonOptions_Click(object sender, RoutedEventArgs e)
        {
            ((GuiWithBulkOCR)this.Owner).ButtonOptions_Click(sender, e);
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
