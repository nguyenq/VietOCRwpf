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
using System.IO;
using System.Collections;
using VietOCR.NET.Utilities;
using System.Windows;
using System.Windows.Input;

namespace VietOCR
{
    public class GuiWithTools : GuiWithSpellcheck
    {
        const string strImageFolder = "ImageFolder";

        string imageFolder;
        int filterIndex;

        private System.ComponentModel.BackgroundWorker backgroundWorkerSplitPdf;
        private System.ComponentModel.BackgroundWorker backgroundWorkerMergeTiff;
        private System.ComponentModel.BackgroundWorker backgroundWorkerMergePdf;
        private System.ComponentModel.BackgroundWorker backgroundWorkerSplitTiff;
        private System.ComponentModel.BackgroundWorker backgroundWorkerConvertPdf;

        public GuiWithTools()
        {
            this.backgroundWorkerSplitPdf = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorkerMergeTiff = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorkerMergePdf = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorkerSplitTiff = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorkerConvertPdf = new System.ComponentModel.BackgroundWorker();
            // 
            // backgroundWorkerSplitPdf
            // 
            this.backgroundWorkerSplitPdf.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerSplitPdf_DoWork);
            this.backgroundWorkerSplitPdf.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerSplitPdf_ProgressChanged);
            this.backgroundWorkerSplitPdf.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerSplitPdf_RunWorkerCompleted);
            // 
            // backgroundWorkerMergeTiff
            // 
            this.backgroundWorkerMergeTiff.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerMergeTiff_DoWork);
            this.backgroundWorkerMergeTiff.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerMergeTiff_ProgressChanged);
            this.backgroundWorkerMergeTiff.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerMergeTiff_RunWorkerCompleted);
            // 
            // backgroundWorkerMergePdf
            // 
            this.backgroundWorkerMergePdf.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerMergePdf_DoWork);
            this.backgroundWorkerMergePdf.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerMergePdf_ProgressChanged);
            this.backgroundWorkerMergePdf.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerMergePdf_RunWorkerCompleted);
            // 
            // backgroundWorkerSplitTiff
            // 
            this.backgroundWorkerSplitTiff.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerSplitTiff_DoWork);
            this.backgroundWorkerSplitTiff.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerSplitTiff_ProgressChanged);
            this.backgroundWorkerSplitTiff.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerSplitTiff_RunWorkerCompleted);
            // 
            // backgroundWorkerConvertPdf
            // 
            this.backgroundWorkerConvertPdf.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerConvertPdf_DoWork);
            this.backgroundWorkerConvertPdf.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerConvertPdf_ProgressChanged);
            this.backgroundWorkerConvertPdf.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerConvertPdf_RunWorkerCompleted);

        }

        protected override void mergeTiffToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = imageFolder;
            openFileDialog1.Title = Properties.Resources.Select_Input_Images;
            openFileDialog1.Filter = "Image Files (*.tif;*.tiff)|*.tif;*.tiff|Image Files (*.bmp)|*.bmp|Image Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|Image Files (*.png)|*.png|All Image Files|*.tif;*.tiff;*.bmp;*.jpg;*.jpeg;*.png";
            openFileDialog1.FilterIndex = filterIndex;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Multiselect = true;

            Nullable<bool> result = openFileDialog1.ShowDialog();

            if (result.HasValue && result.Value)
            {
                filterIndex = openFileDialog1.FilterIndex;
                imageFolder = Path.GetDirectoryName(openFileDialog1.FileName);
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.InitialDirectory = imageFolder;
                saveFileDialog1.Title = Properties.Resources.Save_Multipage_TIFF_Image;
                saveFileDialog1.Filter = "Image Files (*.tif;*.tiff)|*.tif;*.tiff";
                saveFileDialog1.RestoreDirectory = true;

                result = saveFileDialog1.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    File.Delete(saveFileDialog1.FileName);

                    ArrayList args = new ArrayList();
                    args.Add(openFileDialog1.FileNames);
                    args.Add(saveFileDialog1.FileName);

                    this.Cursor = Cursors.Wait;
                    this.statusLabel.Content = Properties.Resources.MergeTIFF_running;

                    this.textBox1.Cursor = Cursors.Wait;
                    this.toolStripProgressBar1.IsEnabled = true;
                    this.toolStripProgressBar1.Visibility = Visibility.Visible;
                    //this.toolStripProgressBar1.Style = ProgressBarStyle.Marquee;

                    // Start the asynchronous operation.
                    backgroundWorkerMergeTiff.RunWorkerAsync(args);
                }
            }
        }

        private void backgroundWorkerMergeTiff_DoWork(object sender, DoWorkEventArgs e)
        {
            ArrayList args = (ArrayList)e.Argument;
            string[] inputFiles = (string[])args[0];
            string outputFile = (string)args[1];
            ImageIOHelper.MergeTiff(inputFiles, outputFile);
            e.Result = outputFile;
        }

        private void backgroundWorkerMergeTiff_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void backgroundWorkerMergeTiff_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.toolStripProgressBar1.IsEnabled = false;
            this.toolStripProgressBar1.Visibility = Visibility.Hidden;

            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                this.statusLabel.Content = String.Empty;
                MessageBox.Show(this, e.Error.Message, strProgName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled the operation.
                // Note that due to a race condition in the DoWork event handler, the Cancelled
                // flag may not have been set, even though CancelAsync was called.
                this.statusLabel.Content = Properties.Resources.canceled;
            }
            else
            {
                // Finally, handle the case where the operation succeeded.
                this.statusLabel.Content = Properties.Resources.MergeTIFFcompleted;
                MessageBox.Show(this, Properties.Resources.MergeTIFFcompleted + Path.GetFileName(e.Result.ToString()) + Properties.Resources.created, strProgName, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            this.statusLabel.Content = String.Empty;
            this.textBox1.Cursor = null;
            this.Cursor = null;
        }

        protected override void splitTiffToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = imageFolder;
            openFileDialog1.Title = Properties.Resources.Select_Input_TIFF;
            openFileDialog1.Filter = "Image Files (*.tif;*.tiff)|*.tif;*.tiff";
            //openFileDialog1.FilterIndex = filterIndex;
            openFileDialog1.RestoreDirectory = true;

            Nullable<bool> result = openFileDialog1.ShowDialog();

            if (result.HasValue && result.Value)
            {
                //filterIndex = openFileDialog1.FilterIndex;
                imageFolder = Path.GetDirectoryName(openFileDialog1.FileName);

                this.Cursor = Cursors.Wait;
                this.statusLabel.Content = Properties.Resources.SplitTIFF_running;

                this.textBox1.Cursor = Cursors.Wait;
                this.toolStripProgressBar1.IsEnabled = true;
                this.toolStripProgressBar1.Visibility = Visibility.Visible;
                //this.toolStripProgressBar1.Style = ProgressBarStyle.Marquee;

                // Start the asynchronous operation.
                backgroundWorkerSplitTiff.RunWorkerAsync(openFileDialog1.FileName);
            }
        }

        private void backgroundWorkerSplitTiff_DoWork(object sender, DoWorkEventArgs e)
        {
            string infilename = (string)e.Argument;
            string basefilename = Path.Combine(Path.GetDirectoryName(infilename), Path.GetFileNameWithoutExtension(infilename));

            IList<string> filenames = ImageIOHelper.SplitMultipageTiff(new FileInfo(infilename));

            // move temp TIFF files to selected folder
            for (int i = 0; i < filenames.Count; i++)
            {
                string outfilename = String.Format("{0}-{1:000}.tif", basefilename, i + 1);
                File.Delete(outfilename);
                File.Move(filenames[i], outfilename);
            }
            //e.Result = filenames;
        }

        private void backgroundWorkerSplitTiff_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void backgroundWorkerSplitTiff_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.toolStripProgressBar1.IsEnabled = false;
            this.toolStripProgressBar1.Visibility = Visibility.Hidden;

            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                this.statusLabel.Content = String.Empty;
                MessageBox.Show(this, e.Error.Message, strProgName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled the operation.
                // Note that due to a race condition in the DoWork event handler, the Cancelled
                // flag may not have been set, even though CancelAsync was called.
                this.statusLabel.Content = Properties.Resources.canceled;
            }
            else
            {
                // Finally, handle the case where the operation succeeded.
                this.statusLabel.Content = Properties.Resources.SplitTIFFcompleted;
                MessageBox.Show(this, Properties.Resources.SplitTIFFcompleted, strProgName, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            this.statusLabel.Content = String.Empty;
            this.textBox1.Cursor = null;
            this.Cursor = null;
        }

        protected override void splitPdfToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SplitPdfDialog dialog = new SplitPdfDialog();
            dialog.Owner = this;

            Nullable<bool> dialogResult = dialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                this.Cursor = Cursors.Wait;
                this.statusLabel.Content = Properties.Resources.SplitPDF_running;

                this.textBox1.Cursor = Cursors.Wait;
                this.toolStripProgressBar1.IsEnabled = true;
                this.toolStripProgressBar1.Visibility = Visibility.Visible;
                //this.toolStripProgressBar1.Style = ProgressBarStyle.Marquee;

                // Start the asynchronous operation.
                backgroundWorkerSplitPdf.RunWorkerAsync(dialog.Args);
            }
        }

        private void backgroundWorkerSplitPdf_DoWork(object sender, DoWorkEventArgs e)
        {
            SplitPdfArgs args = (SplitPdfArgs)e.Argument;

            if (args.Pages)
            {
                PdfUtilities.SplitPdf(args.InputFilename, args.OutputFilename, args.FromPage, args.ToPage);
            }
            else
            {
                string outputFilename = String.Empty;

                if (args.OutputFilename.EndsWith(".pdf"))
                {
                    outputFilename = args.OutputFilename.Substring(0, args.OutputFilename.LastIndexOf(".pdf"));
                }

                int pageCount = PdfUtilities.GetPdfPageCount(args.InputFilename);
                if (pageCount == 0)
                {
                    throw new ApplicationException("Split PDF failed.");
                }

                int pageRange = Int32.Parse(args.NumOfPages);
                int startPage = 1;

                while (startPage <= pageCount)
                {
                    int endPage = startPage + pageRange - 1;
                    string outputFile = outputFilename + startPage + ".pdf";
                    PdfUtilities.SplitPdf(args.InputFilename, outputFile, startPage.ToString(), endPage.ToString());
                    startPage = endPage + 1;
                }
            }

            e.Result = args.OutputFilename;
        }
        private void backgroundWorkerSplitPdf_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }
        private void backgroundWorkerSplitPdf_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.toolStripProgressBar1.IsEnabled = false;
            this.toolStripProgressBar1.Visibility = Visibility.Hidden;

            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                this.statusLabel.Content = String.Empty;
                MessageBox.Show(this, e.Error.Message, strProgName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled the operation.
                // Note that due to a race condition in the DoWork event handler, the Cancelled
                // flag may not have been set, even though CancelAsync was called.
                this.statusLabel.Content = Properties.Resources.canceled;
            }
            else
            {
                // Finally, handle the case where the operation succeeded.
                this.statusLabel.Content = Properties.Resources.SplitPDF_completed;
                MessageBox.Show(this, Properties.Resources.SplitPDF_completed + "\n" + Properties.Resources.check_output_in + Path.GetDirectoryName(e.Result.ToString()), strProgName, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            this.statusLabel.Content = String.Empty;
            this.textBox1.Cursor = null;
            this.Cursor = null;
        }

        protected override void mergePdfToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = imageFolder;
            openFileDialog1.Title = Properties.Resources.Select_Input_PDFs;
            openFileDialog1.Filter = "PDF Files (*.pdf)|*.pdf";
            openFileDialog1.FilterIndex = filterIndex;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Multiselect = true;

            Nullable<bool> result = openFileDialog1.ShowDialog();

            if (result.HasValue && result.Value)
            {
                filterIndex = openFileDialog1.FilterIndex;
                imageFolder = Path.GetDirectoryName(openFileDialog1.FileName);
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.InitialDirectory = imageFolder;
                saveFileDialog1.Title = Properties.Resources.Save_Merged_PDF;
                saveFileDialog1.Filter = "PDF Files (*.pdf)|*.pdf";
                saveFileDialog1.RestoreDirectory = true;

                result = saveFileDialog1.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    ArrayList args = new ArrayList();
                    args.Add(openFileDialog1.FileNames);
                    args.Add(saveFileDialog1.FileName);

                    this.Cursor = Cursors.Wait;
                    this.statusLabel.Content = Properties.Resources.MergePDF_running;

                    this.textBox1.Cursor = Cursors.Wait;
                    this.toolStripProgressBar1.IsEnabled = true;
                    this.toolStripProgressBar1.Visibility = Visibility.Visible;
                    //this.toolStripProgressBar1.Style = ProgressBarStyle.Marquee;

                    // Start the asynchronous operation.
                    backgroundWorkerMergePdf.RunWorkerAsync(args);
                }
            }
        }

        private void backgroundWorkerMergePdf_DoWork(object sender, DoWorkEventArgs e)
        {
            ArrayList args = (ArrayList)e.Argument;
            string[] inputFiles = (string[])args[0];
            string outputFile = (string)args[1];
            PdfUtilities.MergePdf(inputFiles, outputFile);
            e.Result = outputFile;
        }

        private void backgroundWorkerMergePdf_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorkerMergePdf_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.toolStripProgressBar1.IsEnabled = false;
            this.toolStripProgressBar1.Visibility = Visibility.Hidden;

            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                this.statusLabel.Content = String.Empty;
                MessageBox.Show(this, e.Error.Message, strProgName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled the operation.
                // Note that due to a race condition in the DoWork event handler, the Cancelled
                // flag may not have been set, even though CancelAsync was called.
                this.statusLabel.Content = Properties.Resources.canceled;
            }
            else
            {
                // Finally, handle the case where the operation succeeded.
                this.statusLabel.Content = Properties.Resources.MergePDFcompleted;
                MessageBox.Show(this, Properties.Resources.MergePDFcompleted + Path.GetFileName(e.Result.ToString()) + Properties.Resources.created, strProgName, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            this.statusLabel.Content = String.Empty;
            this.textBox1.Cursor = null;
            this.Cursor = null;
        }

        protected override void convertPdfToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = imageFolder;
            openFileDialog1.Title = Properties.Resources.Select_Input_PDF;
            openFileDialog1.Filter = "PDF Files (*.pdf)|*.pdf";
            openFileDialog1.FilterIndex = filterIndex;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Multiselect = true;

            Nullable<bool> result = openFileDialog1.ShowDialog();

            if (result.HasValue && result.Value)
            {
                filterIndex = openFileDialog1.FilterIndex;
                imageFolder = Path.GetDirectoryName(openFileDialog1.FileName);
                this.Cursor = Cursors.Wait;
                this.statusLabel.Content = Properties.Resources.ConvertPDF_running;

                this.textBox1.Cursor = Cursors.Wait;
                this.toolStripProgressBar1.IsEnabled = true;
                this.toolStripProgressBar1.Visibility = Visibility.Visible;
                //this.toolStripProgressBar1.Style = ProgressBarStyle.Marquee;

                // Start the asynchronous operation.
                backgroundWorkerConvertPdf.RunWorkerAsync(openFileDialog1.FileNames);
            }
        }

        private void backgroundWorkerConvertPdf_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] inputFiles = (string[])e.Argument;
            foreach (string inputFile in inputFiles)
            {
                string outputTiffFile = PdfUtilities.ConvertPdf2TiffGS(inputFile);
                string targetFile = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile) + ".tif");
                File.Delete(targetFile);
                File.Move(outputTiffFile, targetFile);
                e.Result = targetFile;
            }
        }

        private void backgroundWorkerConvertPdf_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorkerConvertPdf_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.toolStripProgressBar1.IsEnabled = false;
            this.toolStripProgressBar1.Visibility = Visibility.Hidden;

            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                this.statusLabel.Content = String.Empty;
                MessageBox.Show(this, e.Error.Message, strProgName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled the operation.
                // Note that due to a race condition in the DoWork event handler, the Cancelled
                // flag may not have been set, even though CancelAsync was called.
                this.statusLabel.Content = Properties.Resources.canceled;
            }
            else
            {
                // Finally, handle the case where the operation succeeded.
                this.statusLabel.Content = Properties.Resources.ConvertPDFcompleted;
                MessageBox.Show(this, Properties.Resources.ConvertPDFcompleted + "\n" + Properties.Resources.check_output_in + Path.GetDirectoryName(e.Result.ToString()), strProgName, MessageBoxButton.OK, MessageBoxImage.Information);

            }
            this.statusLabel.Content = String.Empty;
            this.textBox1.Cursor = null;
            this.Cursor = null;
        }

        protected override void LoadRegistryInfo(RegistryKey regkey)
        {
            base.LoadRegistryInfo(regkey);
            imageFolder = (string)regkey.GetValue(strImageFolder, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        protected override void SaveRegistryInfo(RegistryKey regkey)
        {
            base.SaveRegistryInfo(regkey);
            regkey.SetValue(strImageFolder, imageFolder);
        }
    }
}
