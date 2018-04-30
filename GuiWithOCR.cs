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
using System.Drawing;
using System.IO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace VietOCR
{
    public class GuiWithOCR : GuiWithImageOps
    {
        protected string selectedPSM = "Auto"; // 3 - Fully automatic page segmentation, but no OSD (default)
        protected string selectedOEM = "3"; // Default

        BackgroundWorker backgroundWorkerOcr;

        public GuiWithOCR()
        {
            backgroundWorkerOcr = new BackgroundWorker();
            backgroundWorkerOcr.DoWork += backgroundWorkerOcr_DoWork;
            backgroundWorkerOcr.ProgressChanged += backgroundWorkerOcr_ProgressChanged;
            backgroundWorkerOcr.RunWorkerCompleted += backgroundWorkerOcr_RunWorkerCompleted;
            backgroundWorkerOcr.WorkerReportsProgress = true;
            backgroundWorkerOcr.WorkerSupportsCancellation = true;
        }

        protected override void oCRToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.imageMain.Source == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }

            Rectangle rect = this.imageCanvas.GetROI();

            if (rect != Rectangle.Empty)
            {
                try
                {
                    rect = new Rectangle((int)(rect.X * scaleX), (int)(rect.Y * scaleY), (int)(rect.Width * scaleX), (int)(rect.Height * scaleY));
                    performOCR(imageList, inputfilename, imageIndex, rect);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
            else
            {
                performOCR(imageList, inputfilename, imageIndex, Rectangle.Empty);
            }
        }

        protected override void oCRAllPagesToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.imageMain.Source == null)
            {
                MessageBox.Show(this, Properties.Resources.LoadImage, strProgName);
                return;
            }

            this.buttonOCR.Visibility = Visibility.Collapsed;
            this.buttonCancelOCR.Visibility = Visibility.Visible;
            this.buttonCancelOCR.IsEnabled = true;
            performOCR(imageList, inputfilename , -1, Rectangle.Empty);
        }

        /// <summary>
        /// Perform OCR on pages of image.
        /// </summary>
        /// <param name="imageList"></param>
        /// <param name="index">-1 for all pages</param>
        /// <param name="rect">selection rectangle</param>
        void performOCR(IList<Image> imageList, string inputfilename, int index, Rectangle rect)
        {
            try
            {
                if (curLangCode.Trim().Length == 0)
                {
                    MessageBox.Show(this, Properties.Resources.selectLanguage, strProgName);
                    return;
                }

                this.statusLabel.Content = Properties.Resources.OCRrunning;
                this.Cursor = Cursors.Wait;
                
                this.textBox1.Cursor = Cursors.Wait;
                this.buttonOCR.IsEnabled = false;
                this.oCRToolStripMenuItem.IsEnabled = false;
                this.oCRAllPagesToolStripMenuItem.IsEnabled = false;
                this.toolStripProgressBar1.IsEnabled = true;
                this.toolStripProgressBar1.Visibility = Visibility.Visible;
                //this.toolStripProgressBar1.Style = ProgressBarStyle.Marquee;

                OCRImageEntity entity = new OCRImageEntity(imageList, inputfilename, index, rect, curLangCode);
                entity.ScreenshotMode = this.screenshotModeToolStripMenuItem.IsChecked;

                // Start the asynchronous operation.
                backgroundWorkerOcr.RunWorkerAsync(entity);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected override void buttonCancelOCR_Click(object sender, RoutedEventArgs e)
        {
            backgroundWorkerOcr.CancelAsync();
            this.buttonCancelOCR.IsEnabled = false;
        }

        //[System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private void backgroundWorkerOcr_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;

            OCRImageEntity entity = (OCRImageEntity)e.Argument;
            OCR<Image> ocrEngine = new OCRImages();
            ocrEngine.PageSegMode = selectedPSM;
            ocrEngine.OcrEngineMode = selectedOEM;
            ocrEngine.Language = entity.Language;

            // Assign the result of the computation to the Result property of the DoWorkEventArgs
            // object. This is will be available to the RunWorkerCompleted eventhandler.
            //e.Result = ocrEngine.RecognizeText(entity.ClonedImages, entity.Lang, entity.Rect, worker, e);
            IList<Image> images = entity.ClonedImages;

            for (int i = 0; i < images.Count; i++)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                string result = ocrEngine.RecognizeText(((List<Image>)images).GetRange(i, 1), entity.Inputfilename, entity.Rect, worker, e);
                worker.ReportProgress(i, result); // i is not really percentage
            }
        }

        private void backgroundWorkerOcr_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //this.toolStripProgressBar1.Value = e.ProgressPercentage;
            this.textBox1.AppendText((string)e.UserState);
        }

        private void backgroundWorkerOcr_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.toolStripProgressBar1.IsEnabled = false;
            this.toolStripProgressBar1.Visibility = Visibility.Hidden;

            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                this.statusLabel.Content = String.Empty;
                MessageBox.Show(e.Error.Message, Properties.Resources.OCROperation, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled the operation.
                // Note that due to a race condition in the DoWork event handler, the Cancelled
                // flag may not have been set, even though CancelAsync was called.
                this.statusLabel.Content = "OCR " + Properties.Resources.canceled;
            }
            else
            {
                // Finally, handle the case where the operation succeeded.
                this.statusLabel.Content = Properties.Resources.OCRcompleted;
                this.textBox1.Focus();
            }

            Mouse.OverrideCursor = null;
            this.Cursor = null;
            this.textBox1.Cursor = null;
            this.buttonOCR.Visibility = Visibility.Visible;
            this.buttonOCR.IsEnabled = true;
            this.oCRToolStripMenuItem.IsEnabled = true;
            this.oCRAllPagesToolStripMenuItem.IsEnabled = true;
            this.buttonCancelOCR.Visibility = Visibility.Collapsed;
        }
    }
}
