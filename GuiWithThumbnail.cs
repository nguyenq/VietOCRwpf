using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace VietOCR
{
    public class GuiWithThumbnail : GuiWithFile
    {
        private BackgroundWorker backgroundWorkerLoadThumbnail;
        private Style toggleButtonStyle;

        public GuiWithThumbnail()
        {
            toggleButtonStyle = new Style(typeof(RadioButton), (Style)FindResource(typeof(ToggleButton)));
            this.backgroundWorkerLoadThumbnail = new BackgroundWorker();
            this.backgroundWorkerLoadThumbnail.WorkerReportsProgress = true;
            this.backgroundWorkerLoadThumbnail.DoWork += new DoWorkEventHandler(this.backgroundWorkerLoadThumbnail_DoWork);
            this.backgroundWorkerLoadThumbnail.ProgressChanged += new ProgressChangedEventHandler(this.backgroundWorkerLoadThumbnail_ProgressChanged);
            this.backgroundWorkerLoadThumbnail.RunWorkerCompleted += BackgroundWorkerLoadThumbnail_RunWorkerCompleted;
        }

        protected override void loadThumbnails()
        {
            this.panelThumbnail.Children.Clear();
            this.backgroundWorkerLoadThumbnail.RunWorkerAsync();
        }

        protected override void selectThumbnail(int index)
        {
            this.panelThumbnail.Children.OfType<RadioButton>().Where(x => (int) x.Tag == index).First().IsChecked = true;
        }

        private void backgroundWorkerLoadThumbnail_DoWork(object sender, DoWorkEventArgs e)
        {
            // Create thumbnails
            for (int i = 0; i < imageList.Count; i++)
            {
                System.Drawing.Image thumbnail = imageList[i].GetThumbnailImage(85, 110, null, IntPtr.Zero);
                this.backgroundWorkerLoadThumbnail.ReportProgress(i, thumbnail);
            }
        }

        private void backgroundWorkerLoadThumbnail_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Load thumbnails & associated labels into panel
            System.Drawing.Image thumbnail = (System.Drawing.Image)e.UserState;
            RadioButton rb = new RadioButton();
            rb.Width = thumbnail.Width;
            rb.Height = thumbnail.Height;
            rb.Style = toggleButtonStyle;
            BitmapImage bmimage = ImageConverter.BitmapToImageSource(thumbnail);
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = bmimage;
            rb.Content = image;
            rb.HorizontalAlignment = HorizontalAlignment.Center;
            rb.Tag = e.ProgressPercentage;
            rb.Click += new RoutedEventHandler(this.radioButton_Click);
            this.panelThumbnail.Children.Add(rb);
            Label label = new Label();
            label.Content = e.ProgressPercentage + 1;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.Margin = new Thickness(0, -5, 0, 0);
            this.panelThumbnail.Children.Add(label);
        }

        private void BackgroundWorkerLoadThumbnail_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                //cancelled
            }
            else
            {
                selectThumbnail(imageIndex);
            }
        }

        private void radioButton_Click(object sender, RoutedEventArgs e)
        {
            int index = (int)((RadioButton)sender).Tag;
            if (imageIndex == index)
            {
                return;
            }
            imageIndex = index;
            this.comboBoxPageNum.SelectedItem = (imageIndex + 1);
        }

        //protected override void splitContainerImage_SplitterMoved(object sender, SplitterEventArgs e)
        //{
        //    foreach (Control con in this.panelThumbnail.Children)
        //    {
        //        int horizontalMargin = (int)(this.panelThumbnail.Width - con.Width) / 2;
        //        con.Margin = new Thickness(horizontalMargin, 0, horizontalMargin, 2);
        //    }
        //}
    }
}
