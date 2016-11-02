using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Drawing.Imaging;
using VietOCR.NET.WIA;
using System.Windows;
using System.Windows.Input;

namespace VietOCR
{
    public class GuiWithScan : GuiWithThumbnail
    {
        private System.ComponentModel.BackgroundWorker backgroundWorkerScan;

        public GuiWithScan()
        {
            this.backgroundWorkerScan = new System.ComponentModel.BackgroundWorker();
            // 
            // backgroundWorkerScan
            // 
            this.backgroundWorkerScan.WorkerReportsProgress = true;
            this.backgroundWorkerScan.WorkerSupportsCancellation = true;
            this.backgroundWorkerScan.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerScan_DoWork);
            this.backgroundWorkerScan.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerScan_RunWorkerCompleted);

        }

        protected override void scanToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            scaleX = scaleY = 1f;
            performScan();
        }

        /// <summary>
        /// Access scanner and scan documents via WIA.
        /// </summary>
        void performScan()
        {
            try
            {
                this.statusLabel.Content = Properties.Resources.Scanning;
                this.Cursor = Cursors.Wait;
                
                this.textBox1.Cursor = Cursors.Wait;
                this.buttonScan.IsEnabled = false;
                this.scanToolStripMenuItem.IsEnabled = false;
                this.toolStripProgressBar1.IsEnabled = true;
                this.toolStripProgressBar1.Visibility = Visibility.Visible;
                //this.toolStripProgressBar1.Style = ProgressBarStyle.Marquee;

                // Start the asynchronous operation.
                backgroundWorkerScan.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private void backgroundWorkerScan_DoWork(object sender, DoWorkEventArgs e)
        {
            using (WiaScannerAdapter adapter = new WiaScannerAdapter())
            {
                try
                {
                    string tempFileName = Path.GetTempFileName();
                    File.Delete(tempFileName);
                    tempFileName = Path.ChangeExtension(tempFileName, ".png");
                    tempFileCollection.AddFile(tempFileName, false);
                    FileInfo imageFile = new FileInfo(tempFileName);
                    if (imageFile.Exists)
                    {
                        imageFile.Delete();
                    }
                    adapter.ScanImage(ImageFormat.Png, imageFile.FullName);
                    e.Result = tempFileName;
                }
                catch (WiaOperationException ex)
                {
                    throw new Exception(System.Text.RegularExpressions.Regex.Replace(ex.ErrorCode.ToString(), "(?=\\p{Lu}+)", " ").Trim() + ".");
                }
            }
        }

        private void backgroundWorkerScan_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.toolStripProgressBar1.IsEnabled = false;
            this.toolStripProgressBar1.Visibility = Visibility.Hidden;

            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                this.statusLabel.Content = String.Empty;
                MessageBox.Show(e.Error.Message, Properties.Resources.ScanningOperation, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled the operation.
                // Note that due to a race condition in the DoWork event handler, the Cancelled
                // flag may not have been set, even though CancelAsync was called.
                this.statusLabel.Content = "Scanning " + Properties.Resources.canceled;
            }
            else
            {
                // Finally, handle the case where the operation succeeded.
                openFile(e.Result.ToString());
                this.statusLabel.Content = Properties.Resources.Scancompleted;
            }

            this.Cursor = null;
            this.textBox1.Cursor = null;
            this.buttonScan.IsEnabled = true;
            this.scanToolStripMenuItem.IsEnabled = true;
        }
    }
}
