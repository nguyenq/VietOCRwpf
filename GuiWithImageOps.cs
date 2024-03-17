using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace VietOCR
{
    public class GuiWithImageOps : GuiWithScan
    {
        //private bool isFitForZoomIn = false;
        private const float ZOOM_FACTOR = 1.25f;

        protected override void buttonPrev_Click(object sender, RoutedEventArgs e)
        {
            this.imageCanvas.Deselect();
            imageIndex--;
            if (imageIndex < 0)
            {
                imageIndex = 0;
            }
            else
            {
                this.statusLabel.Content = null;
                this.comboBoxPageNum.SelectedItem = (imageIndex + 1);
            }
        }

        protected override void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            this.imageCanvas.Deselect();
            imageIndex++;
            if (imageIndex > imageTotal - 1)
            {
                imageIndex = imageTotal - 1;
            }
            else
            {
                this.statusLabel.Content = null;
                this.comboBoxPageNum.SelectedItem = (imageIndex + 1);
            }
        }

        protected override void comboBoxPageNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.comboBoxPageNum.IsEnabled || this.comboBoxPageNum.SelectedIndex == -1) return;

            int pageNum = Int32.Parse(this.comboBoxPageNum.SelectedItem.ToString());
            this.imageCanvas.Deselect();
            imageIndex = pageNum - 1;
            this.statusLabel.Content = null;
            displayImage();
            clearStack();

            // recalculate scale factors if in Fit Image mode
            //if (this.imageMain.SizeMode == PictureBoxSizeMode.Zoom)
            //{
            //    scaleX = (float)this.imageMain.Source.Width / (float)this.imageMain.Width;
            //    scaleY = (float)this.imageMain.Source.Height / (float)this.imageMain.Height;
            //}

            setButton();
            selectThumbnail(imageIndex);
        }

        protected override void buttonFitImage_Click(object sender, RoutedEventArgs e)
        {
            this.buttonFitImage.IsEnabled = false;
            this.buttonActualSize.IsEnabled = true;
            this.buttonZoomIn.IsEnabled = false;
            this.buttonZoomOut.IsEnabled = false;
            //curScrollPos = this.splitContainerImage.Panel2.AutoScrollPosition;
            //this.splitContainerImage.Panel2.AutoScrollPosition = Point.Empty;
            this.imageCanvas.Deselect();
            setSegmentedRegions();

            //this.imageCanvas.Dock = DockStyle.None;
            //this.imageCanvas.SizeMode = PictureBoxSizeMode.Zoom;
            System.Drawing.Size fitSize = fitImagetoContainer((int)this.imageCanvas.Width, (int)this.imageCanvas.Height, (int)this.scrollViewer.ActualWidth, (int)this.scrollViewer.ActualHeight);
            this.imageCanvas.Width = fitSize.Width;
            this.imageCanvas.Height = fitSize.Height;
            setScale();
            this.centerPicturebox();
            isFitImageSelected = true;
        }

        protected override void buttonActualSize_Click(object sender, RoutedEventArgs e)
        {
            this.buttonFitImage.IsEnabled = true;
            this.buttonActualSize.IsEnabled = false;
            this.buttonZoomIn.IsEnabled = true;
            this.buttonZoomOut.IsEnabled = true;

            this.imageCanvas.Deselect();
            setSegmentedRegions();
            this.imageCanvas.Width = CurrentImage.Width;
            this.imageCanvas.Height = CurrentImage.Height;
            //this.imageCanvas.Dock = DockStyle.None;
            //this.imageCanvas.SizeMode = PictureBoxSizeMode.Normal;
            scaleX = scaleY = 1f;
            this.centerPicturebox();
            //this.splitContainerImage.Panel2.AutoScrollPosition = new System.Drawing.Point(Math.Abs(curScrollPos.X), Math.Abs(curScrollPos.Y));
            isFitImageSelected = false;
        }

        protected override void buttonRotateCCW_Click(object sender, RoutedEventArgs e)
        {
            this.centerPicturebox();
            this.imageCanvas.Deselect();
            // Rotating 270 degrees is equivalent to rotating -90 degrees.
            imageList[imageIndex].RotateFlip(RotateFlipType.Rotate270FlipNone);
            this.imageMain.Source = ImageConverter.BitmapToImageSource(imageList[imageIndex]);
            adjustPictureBoxAfterFlip();
            clearStack();
        }

        protected override void buttonRotateCW_Click(object sender, RoutedEventArgs e)
        {
            this.centerPicturebox();
            this.imageCanvas.Deselect();
            imageList[imageIndex].RotateFlip(RotateFlipType.Rotate90FlipNone);
            this.imageMain.Source = ImageConverter.BitmapToImageSource(imageList[imageIndex]);
            adjustPictureBoxAfterFlip();
            clearStack();
        }

        private void adjustPictureBoxAfterFlip()
        {
            this.imageCanvas.Width = CurrentImage.Width / scaleX;
            this.imageCanvas.Height = CurrentImage.Height / scaleY;
            // recalculate scale factors if in Fit Image mode
            if (this.isFitImageSelected)
            {
                setScale();
            }
            this.centerPicturebox();
        }

        protected override void buttonZoomIn_Click(object sender, RoutedEventArgs e)
        {
            this.buttonActualSize.IsEnabled = true;
            this.imageCanvas.Deselect();
            setSegmentedRegions();
            ////isFitForZoomIn = true;
            //this.imageMain.SizeMode = PictureBoxSizeMode.Zoom;

            // Zoom works best if you first fit the image according to its true aspect ratio.
            Fit();
            // Make the PictureBox dimensions larger by 25% to effect the Zoom.
            this.imageCanvas.Width = Convert.ToInt32(this.imageCanvas.Width * ZOOM_FACTOR);
            this.imageCanvas.Height = Convert.ToInt32(this.imageCanvas.Height * ZOOM_FACTOR);
            scaleX = (float)CurrentImage.Width / (float)this.imageCanvas.Width;
            scaleY = (float)CurrentImage.Height / (float)this.imageCanvas.Height;
            this.centerPicturebox();
            isFitImageSelected = false;
        }

        protected override void buttonZoomOut_Click(object sender, RoutedEventArgs e)
        {
            this.buttonActualSize.IsEnabled = true;
            this.imageCanvas.Deselect();
            setSegmentedRegions();
            //isFitForZoomIn = false;
            // StretchImage SizeMode works best for zooming.
            //this.imageMain.SizeMode = PictureBoxSizeMode.Zoom;
            // Zoom works best if you first fit the image according to its true aspect ratio.
            Fit();
            // Make the PictureBox dimensions smaller by 25% to effect the Zoom.
            this.imageCanvas.Width = Convert.ToInt32(this.imageCanvas.Width / ZOOM_FACTOR);
            this.imageCanvas.Height = Convert.ToInt32(this.imageCanvas.Height / ZOOM_FACTOR);
            scaleX = (float)CurrentImage.Width / (float)this.imageCanvas.Width;
            scaleY = (float)CurrentImage.Height / (float)this.imageCanvas.Height;
            this.centerPicturebox();
            isFitImageSelected = false;
        }

        // This method makes the image fit properly in the PictureBox. You might think 
        // that the AutoSize SizeMode enum would make the image appear in the PictureBox 
        // according to its true aspect ratio within the fixed bounds of the PictureBox.
        // However, it merely expands or shrinks the PictureBox.
        private void Fit()
        {
            // if Fit was called by the Zoom In button, then center the image. This is
            // only needed when working with images that are smaller than the PictureBox.
            // Feel free to uncomment the line that sets the SizeMode and then see how
            // it causes Zoom In for small images to show unexpected behavior.

            //if ((this.imageMain.Image.Width < this.imageMain.Width) &&
            //    (this.imageMain.Image.Height < this.imageMain.Height))
            //{
            //    if (!isFitForZoomIn)
            //    {
            //        this.imageMain.SizeMode = PictureBoxSizeMode.CenterImage;
            //    }
            //}
            //CalculateAspectRatioAndSetDimensions();
        }

        //// Calculates and returns the image's aspect ratio, and sets 
        //// its proper dimensions. This is used for Fit() and for saving thumbnails
        //// of images.
        //private double CalculateAspectRatioAndSetDimensions()
        //{
        //    // Calculate the proper aspect ratio and set the image's dimensions.
        //    double ratio;

        //    if (this.imageMain.Image.Width > this.imageMain.Image.Height)
        //    {
        //        ratio = this.imageMain.Image.Width / this.imageMain.Image.Height;
        //        this.imageMain.Height = Convert.ToInt32(Convert.ToDouble(this.imageMain.Width) / ratio);
        //    }
        //    else
        //    {
        //        ratio = this.imageMain.Image.Height / this.imageMain.Image.Width;
        //        this.imageMain.Width = Convert.ToInt32(Convert.ToDouble(this.imageMain.Height) / ratio);
        //    }
        //    return ratio;
        //}
    }
}
