/**
 * Copyright @ 2008 Quan Nguyen
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using Microsoft.Win32;
using VietOCR.NET.Utilities;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace VietOCR
{
    public class GuiWithImage : GuiWithBulkOCR
    {
        const string strScreenshotMode = "ScreenshotMode";
        const double MINIMUM_DESKEW_THRESHOLD = 0.05d;
        FixedSizeStack<System.Drawing.Image> stack = new FixedSizeStack<System.Drawing.Image>(10);
        System.Drawing.Image originalImage;

        public GuiWithImage()
        {
            InitializeComponent();
        }

        protected override void Window_Loaded(object sender, RoutedEventArgs e)
        {
            base.Window_Loaded(sender, e);
            statusLabelSMvalue.Content = this.screenshotModeToolStripMenuItem.IsChecked ? "On" : "Off";
        }

        protected override void metadataToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }

            ImageInfoDialog dialog = new ImageInfoDialog();
            dialog.Image = imageList[imageIndex];

            Nullable<bool> dialogResult = dialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                // Do nothing for now. 
                // Initial plan was to implement various image manipulation operations 
                // (rotate, flip, sharpen, brighten, threshold, clean up,...) here.
            }
        }

        protected override void brightenToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }
            SliderDialog dialog = new SliderDialog();
            dialog.LabelText = Properties.Resources.Brightness;
            dialog.ValueUpdated += new SliderDialog.HandleValueChange(UpdatedBrightness);

            originalImage = imageList[imageIndex];
            stack.Push(originalImage);
            Nullable<bool> dialogResult = dialog.ShowDialog();
            if (dialogResult.HasValue && !dialogResult.Value)
            {
                // restore original image
                imageList[imageIndex] = originalImage;
                this.imageMain.Source = ImageConverter.BitmapToImageSource(originalImage);
            }
        }

        protected override void contrastToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }
            SliderDialog dialog = new SliderDialog();
            dialog.LabelText = Properties.Resources.Contrast;
            dialog.SetForContrast();
            dialog.ValueUpdated += new SliderDialog.HandleValueChange(UpdatedContrast);

            originalImage = imageList[imageIndex];
            stack.Push(originalImage);

            Nullable<bool> dialogResult = dialog.ShowDialog();
            if (dialogResult.HasValue && !dialogResult.Value)
            {
                // restore original image
                imageList[imageIndex] = originalImage;
                this.imageMain.Source = ImageConverter.BitmapToImageSource(originalImage);
            }
        }

        private void UpdatedBrightness(object sender, SliderDialog.ValueChangedEventArgs e)
        {
            System.Drawing.Image image = ImageHelper.Brighten(originalImage, (float) e.NewValue * 0.005f);
            if (image != null)
            {
                imageList[imageIndex] = image;
                this.imageMain.Source = ImageConverter.BitmapToImageSource(image);
            }
        }

        private void UpdatedContrast(object sender, SliderDialog.ValueChangedEventArgs e)
        {
            System.Drawing.Image image = ImageHelper.Contrast(originalImage, (float)e.NewValue * 0.04f);
            if (image != null)
            {
                imageList[imageIndex] = image;
                this.imageMain.Source = ImageConverter.BitmapToImageSource(image);
            }
        }

        protected override void deskewToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }
            this.imageCanvas.Deselect();
            this.Cursor = Cursors.Wait;

            gmseDeskew deskew = new gmseDeskew((Bitmap)imageList[imageIndex]);
            double imageSkewAngle = deskew.GetSkewAngle();

            if ((imageSkewAngle > MINIMUM_DESKEW_THRESHOLD || imageSkewAngle < -(MINIMUM_DESKEW_THRESHOLD)))
            {
                originalImage = imageList[imageIndex];
                stack.Push(originalImage);
                imageList[imageIndex] = ImageHelper.Rotate((Bitmap)originalImage, -imageSkewAngle);
                this.imageMain.Source = ImageConverter.BitmapToImageSource(imageList[imageIndex]);
            }
            this.Cursor = null;
        }

        protected override void autocropToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }
            this.Cursor = Cursors.Wait;
            originalImage = imageList[imageIndex];
            imageList[imageIndex] = ImageHelper.AutoCrop((Bitmap)originalImage, 0.1);

            // if same image, skip
            if (originalImage != imageList[imageIndex])
            {
                stack.Push(originalImage);
                displayImage();
            }

            this.Cursor = null;
        }

        protected override void cropToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }
            Rectangle rect = this.imageCanvas.GetROI();

            if (rect == Rectangle.Empty)
            {
                return;
            }

            rect = new Rectangle((int)(rect.X * scaleX), (int)(rect.Y * scaleY), (int)(rect.Width * scaleX), (int)(rect.Height * scaleY));

            this.Cursor = Cursors.Wait;
            originalImage = imageList[imageIndex];
            System.Drawing.Image croppedImage = ImageHelper.Crop(originalImage, rect);
            if (originalImage.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                croppedImage = ImageHelper.ConvertGrayscale(croppedImage);
            }
            imageList[imageIndex] = croppedImage;
            stack.Push(originalImage);
            displayImage();
            this.Cursor = null;
        }

        protected override void removeLinesToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }

            try
            {
                originalImage = imageList[imageIndex];
                imageList[imageIndex] = ImageHelper.RemoveLines((Bitmap)originalImage);
                this.imageMain.Source = ImageConverter.BitmapToImageSource(imageList[imageIndex]);
                stack.Push(originalImage);
            }
            catch
            {
                MessageBox.Show(this, Properties.Resources.Require_grayscale, strProgName, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        protected override void grayscaleToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }
            originalImage = imageList[imageIndex];
            stack.Push(originalImage);
            imageList[imageIndex] = ImageHelper.ConvertGrayscale((Bitmap)originalImage);
            this.imageMain.Source = ImageConverter.BitmapToImageSource(imageList[imageIndex]);
        }

        protected override void monochromeToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }
            originalImage = imageList[imageIndex];
            stack.Push(originalImage);
            imageList[imageIndex] = ImageHelper.ConvertMonochrome((Bitmap)originalImage);
            this.imageMain.Source = ImageConverter.BitmapToImageSource(imageList[imageIndex]);
        }

        protected override void gammaMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }
            SliderDialog dialog = new SliderDialog();
            dialog.SetForGamma();
            dialog.LabelText = Properties.Resources.Gamma;
            dialog.ValueUpdated += new SliderDialog.HandleValueChange(UpdatedGamma);

            originalImage = imageList[imageIndex];
            stack.Push(originalImage);
            Nullable<bool> dialogResult = dialog.ShowDialog();
            if (dialogResult.HasValue && !dialogResult.Value)
            {
                // restore original image
                imageList[imageIndex] = originalImage;
                this.imageMain.Source = ImageConverter.BitmapToImageSource(originalImage);
            }
        }

        private void UpdatedGamma(object sender, SliderDialog.ValueChangedEventArgs e)
        {
            System.Drawing.Image image = ImageHelper.AdjustGamma(originalImage, (float) e.NewValue * 0.005f);
            if (image != null)
            {
                imageList[imageIndex] = image;
                this.imageMain.Source = ImageConverter.BitmapToImageSource(image);
            }
        }

        protected override void thresholdMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }
            SliderDialog dialog = new SliderDialog();
            dialog.SetForThreshold();
            dialog.LabelText = Properties.Resources.Threshold;
            dialog.ValueUpdated += new SliderDialog.HandleValueChange(UpdatedThreshold);

            originalImage = imageList[imageIndex];
            stack.Push(originalImage);
            Nullable<bool> dialogResult = dialog.ShowDialog();
            if (dialogResult.HasValue && !dialogResult.Value)
            {
                // restore original image
                imageList[imageIndex] = originalImage;
                this.imageMain.Source = ImageConverter.BitmapToImageSource(originalImage);
            }
        }

        private void UpdatedThreshold(object sender, SliderDialog.ValueChangedEventArgs e)
        {
            System.Drawing.Image image = ImageHelper.AdjustThreshold(originalImage, (float) e.NewValue * 0.01f);
            if (image != null)
            {
                imageList[imageIndex] = image;
                this.imageMain.Source = ImageConverter.BitmapToImageSource(image);
            }
        }

        protected override void bilateralFilterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }
            originalImage = imageList[imageIndex];
            stack.Push(originalImage);
            imageList[imageIndex] = ImageHelper.BilateralFilter((Bitmap)originalImage);
            this.imageMain.Source = ImageConverter.BitmapToImageSource(imageList[imageIndex]);
        }

        protected override void invertToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }
            originalImage = imageList[imageIndex];
            stack.Push(originalImage);
            imageList[imageIndex] = ImageHelper.InvertColor((Bitmap)originalImage);
            this.imageMain.Source = ImageConverter.BitmapToImageSource(imageList[imageIndex]);
        }

        protected override void sharpenToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }
            originalImage = imageList[imageIndex];
            stack.Push(originalImage);
            imageList[imageIndex] = ImageHelper.Sharpen((Bitmap)originalImage);
            this.imageMain.Source = ImageConverter.BitmapToImageSource(imageList[imageIndex]);
        }

        protected override void smoothToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageList == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }
            originalImage = imageList[imageIndex];
            stack.Push(originalImage);
            imageList[imageIndex] = ImageHelper.GaussianBlur((Bitmap)originalImage);
            this.imageMain.Source = ImageConverter.BitmapToImageSource(imageList[imageIndex]);
        }

        protected override void screenshotModeToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            mi.IsChecked ^= true;
            statusLabelSMvalue.Content = mi.IsChecked ? "On" : "Off";
        }

        protected override void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (stack.Count == 0)
            {
                return;
            }

            System.Drawing.Image image = stack.Pop();
            imageList[imageIndex] = image;
            displayImage();
        }

        protected override void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = stack.Count > 0;
        }

        protected override void clearStack()
        {
            stack.Clear();
        }

        protected override void LoadRegistryInfo(RegistryKey regkey)
        {
            base.LoadRegistryInfo(regkey);

            this.screenshotModeToolStripMenuItem.IsChecked = Convert.ToBoolean(
                (int)regkey.GetValue(strScreenshotMode, Convert.ToInt32(false)));
        }

        protected override void SaveRegistryInfo(RegistryKey regkey)
        {
            base.SaveRegistryInfo(regkey);

            regkey.SetValue(strScreenshotMode, Convert.ToInt32(this.screenshotModeToolStripMenuItem.IsChecked));
        }
    }
}
