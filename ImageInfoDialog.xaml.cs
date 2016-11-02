using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace VietOCR
{
    /// <summary>
    /// Interaction logic for ImageInfoDialog.xaml
    /// </summary>
    public partial class ImageInfoDialog : Window
    {
        System.Drawing.Image image;
        bool isProgrammatic;

        public System.Drawing.Image Image
        {
            get { return image; }
            set { image = value; }
        }

        public ImageInfoDialog()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ConvertUnits(int unit)
        {
            switch (unit)
            {
                case 1: // "inches"
                    this.textBoxWidth.Text = Math.Round(this.image.Width / this.image.HorizontalResolution, 1).ToString();
                    this.textBoxHeight.Text = Math.Round(this.image.Height / this.image.VerticalResolution, 1).ToString();
                    break;

                case 2: //"cm"
                    this.textBoxWidth.Text = Math.Round(this.image.Width / this.image.HorizontalResolution * 2.54, 2).ToString();
                    this.textBoxHeight.Text = Math.Round(this.image.Height / this.image.VerticalResolution * 2.54, 2).ToString();
                    break;

                default: // "pixel"
                    this.textBoxWidth.Text = this.image.Width.ToString();
                    this.textBoxHeight.Text = this.image.Height.ToString();
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBoxXRes.Text = Math.Round(this.image.HorizontalResolution).ToString();
            this.textBoxYRes.Text = Math.Round(this.image.VerticalResolution).ToString();
            this.textBoxWidth.Text = this.image.Width.ToString();
            this.textBoxHeight.Text = this.image.Height.ToString();
            this.textBoxBitDepth.Text = Bitmap.GetPixelFormatSize(this.image.PixelFormat).ToString();
            this.comboBoxW.SelectedIndex = 0;
            this.comboBoxH.SelectedIndex = 0;
        }

        private void comboBoxW_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isProgrammatic)
            {
                isProgrammatic = true;
                this.comboBoxH.SelectedIndex = this.comboBoxW.SelectedIndex;
                ConvertUnits(this.comboBoxW.SelectedIndex);
                isProgrammatic = false;
            }
        }

        private void comboBoxH_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isProgrammatic)
            {
                isProgrammatic = true;
                this.comboBoxW.SelectedIndex = this.comboBoxH.SelectedIndex;
                ConvertUnits(this.comboBoxH.SelectedIndex);
                isProgrammatic = false;
            }
        }
    }
}
