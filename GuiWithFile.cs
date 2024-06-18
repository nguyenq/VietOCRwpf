/**
 * Copyright @ 2016 Quan Nguyen
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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VietOCR.NET.Utilities;

namespace VietOCR
{
    public class GuiWithFile : GuiWithRegistry
    {
        private int filterIndex;
        List<string> mruList = new List<string>();
        private string strClearRecentFiles;
        const string strFilterIndex = "FilterIndex";
        const string strMruList = "MruList";
        private BackgroundWorker backgroundWorkerLoad;

        protected System.Drawing.Image CurrentImage
        {
            get { return imageList[imageIndex]; }
        }

        public GuiWithFile()
        {
            // 
            // backgroundWorkerLoad
            // 
            this.backgroundWorkerLoad = new BackgroundWorker();
            this.backgroundWorkerLoad.WorkerReportsProgress = true;
            this.backgroundWorkerLoad.WorkerSupportsCancellation = true;
            this.backgroundWorkerLoad.DoWork += new DoWorkEventHandler(this.backgroundWorkerLoad_DoWork);
            this.backgroundWorkerLoad.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorkerLoad_RunWorkerCompleted);
        }

        /// <summary>
        /// Opens image or text file.
        /// </summary>
        /// <param name="selectedFile"></param>
        protected override void openFile(string selectedFile)
        {
            OpenFiles(new[] { selectedFile });
        }


        /// <summary>
        /// Opens images or text file.
        /// </summary>
        /// <param name="selectedFiles"></param>
        void OpenFiles(string[] selectedFiles)
        {
            string selectedFile = selectedFiles[0];

            if (!File.Exists(selectedFile))
            {
                MessageBox.Show(this, Properties.Resources.File_not_exist, strProgName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // if text file, load it into textbox
            if (selectedFile.EndsWith(".txt"))
            {
                if (!OkToTrash())
                    return;

                try
                {
                    using (StreamReader sr = new StreamReader(selectedFile, Encoding.UTF8, true))
                    {
                        textModified = false;
                        this.textBox1.Text = sr.ReadToEnd();
                        updateMRUList(selectedFile);
                        textFilename = selectedFile;
                        //this.textBox1.Modified = false;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(this, e.Message, strProgName, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }

            this.statusLabel.Content = Properties.Resources.Loading_image;
            this.Cursor = Cursors.Wait;
            //this.pictureBox1.UseWaitCursor = true;
            this.textBox1.Cursor = Cursors.Wait;
            this.buttonOCR.IsEnabled = false;
            this.oCRToolStripMenuItem.IsEnabled = false;
            this.oCRAllPagesToolStripMenuItem.IsEnabled = false;
            this.toolStripProgressBar1.IsEnabled = true;
            this.toolStripProgressBar1.Visibility = Visibility.Visible;
            //this.toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
            this.backgroundWorkerLoad.RunWorkerAsync(Tuple.Create(selectedFiles, Keyboard.Modifiers == ModifierKeys.Shift));
            updateMRUList(selectedFile);
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private void backgroundWorkerLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            var tuple = e.Argument as Tuple<string[], bool>;
            string[] fileNames = tuple.Item1;
            inputfilename = fileNames[0];

            if (!tuple.Item2)
            {
                imageList.Clear();
            }

            foreach (string fileName in fileNames)
            {
                FileInfo imageFile = new FileInfo(fileName);
                ((List<System.Drawing.Image>)imageList).AddRange(ImageIOHelper.GetImageList(imageFile));
            }

            e.Result = Tuple.Create(fileNames, tuple.Item2);
        }

        private void backgroundWorkerLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.toolStripProgressBar1.IsEnabled = false;
            this.toolStripProgressBar1.Visibility = Visibility.Hidden;

            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                this.statusLabel.Content = string.Empty;
                MessageBox.Show(e.Error.Message, Properties.Resources.Load_image, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled the operation.
                // Note that due to a race condition in the DoWork event handler, the Cancelled
                // flag may not have been set, even though CancelAsync was called.
                this.statusLabel.Content = "Image loading " + Properties.Resources.canceled;
            }
            else
            {
                // Finally, handle the case where the operation succeeded.
                var tuple = e.Result as Tuple<string[], bool>;
                loadImage(tuple.Item2);
                this.Title = tuple.Item1[0] + " - " + strProgName;
                this.statusLabel.Content = Properties.Resources.Loading_completed;
            }

            this.Cursor = null;
            //this.pictureBox1.UseWaitCursor = false;
            this.textBox1.Cursor = null;
            this.buttonOCR.IsEnabled = true;
            this.oCRToolStripMenuItem.IsEnabled = true;
            this.oCRAllPagesToolStripMenuItem.IsEnabled = true;
        }

        void loadImage(bool isShiftDown)
        {
            if (!imageList.Any())
            {
                MessageBox.Show(this, Properties.Resources.Cannotloadimage, strProgName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (isShiftDown)
            {
                imageIndex = imageTotal;
            }
            else
            {
                imageIndex = 0;
            }

            imageTotal = imageList.Count;

            //this.pictureBox1.Dock = DockStyle.None;
            //this.pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
            scaleX = scaleY = 1f;
            isFitImageSelected = false;

            this.comboBoxPageNum.IsEnabled = true;
            this.comboBoxPageNum.Items.Clear();
            for (int i = 0; i < imageTotal; i++)
            {
                this.comboBoxPageNum.Items.Add(i + 1);
            }
            this.comboBoxPageNum.IsEnabled = false;
            this.comboBoxPageNum.SelectedIndex = imageIndex;
            this.comboBoxPageNum.IsEnabled = true;

            this.labelTotalPages.Content = " / " + imageTotal.ToString();
            this.labelTotalPages.IsEnabled = true;

            displayImage();
            loadThumbnails();

            // clear undo buffer
            clearStack();

            this.imageCanvas.Deselect();

            this.buttonFitImage.IsEnabled = true;
            this.buttonActualSize.IsEnabled = false;
            this.buttonZoomIn.IsEnabled = true;
            this.buttonZoomOut.IsEnabled = true;
            this.buttonRotateCCW.IsEnabled = true;
            this.buttonRotateCW.IsEnabled = true;

            if (imageList.Count == 1)
            {
                this.buttonNext.IsEnabled = false;
                this.buttonPrev.IsEnabled = false;
            }
            else
            {
                this.buttonNext.IsEnabled = true;
                this.buttonPrev.IsEnabled = true;
            }

            setButton();
        }

        protected void displayImage()
        {
            //var uri = new Uri(inputfilename);
            //var bitmap = new BitmapImage(uri);
            //imageMain.Source = bitmap;
            this.imageMain.Source = ImageConverter.BitmapToImageSource(CurrentImage);
            this.imageCanvas.Width = CurrentImage.Width;
            this.imageCanvas.Height = CurrentImage.Height;
            this.statusLabelDimValue.Content = string.Format("{0} × {1}px  {2}bpp", CurrentImage.Width, CurrentImage.Height, System.Drawing.Bitmap.GetPixelFormatSize(CurrentImage.PixelFormat).ToString());

            if (this.isFitImageSelected)
            {
                System.Drawing.Size fitSize = fitImagetoContainer((int)this.imageCanvas.Width, (int)this.imageCanvas.Height, (int)this.scrollViewer.ActualWidth, (int)this.scrollViewer.ActualHeight);
                this.imageCanvas.Width = fitSize.Width;
                this.imageCanvas.Height = fitSize.Height;
                setScale();
            }
            else if (this.scaleX != 1f)
            {
                this.imageCanvas.Width = Convert.ToInt32(CurrentImage.Width / scaleX);
                this.imageCanvas.Height = Convert.ToInt32(CurrentImage.Height / scaleY);
            }
            //curScrollPos = Point.Empty;
            this.centerPicturebox();

            this.imageCanvas.Deselect();
            this.imageCanvas.SegmentedRegions = null;
            setSegmentedRegions();
        }

        protected void setScale()
        {
            scaleX = (float)CurrentImage.Width / (float)this.imageCanvas.Width;
            scaleY = (float)CurrentImage.Height / (float)this.imageCanvas.Height;
            if (scaleX > scaleY)
            {
                scaleY = scaleX;
            }
            else
            {
                scaleX = scaleY;
            }
        }

        protected void setButton()
        {
            this.buttonPrev.IsEnabled = imageIndex != 0;
            this.buttonNext.IsEnabled = imageIndex != (imageList.Count - 1);
        }
        /// <summary>
        /// Update MRU Submenu.
        /// </summary>
        private void updateMRUMenu()
        {
            this.recentFilesToolStripMenuItem.Items.Clear();

            if (mruList.Count == 0)
            {
                this.recentFilesToolStripMenuItem.Items.Add(Properties.Resources.No_Recent_Files);
            }
            else
            {
                RoutedEventHandler eh = new RoutedEventHandler(MenuRecentFilesOnClick);

                foreach (string fileName in mruList)
                {
                    MenuItem item = new MenuItem { Header = fileName };
                    this.recentFilesToolStripMenuItem.Items.Add(item);
                    item.Click += eh;
                }
                this.recentFilesToolStripMenuItem.Items.Add(new Separator());
                strClearRecentFiles = Properties.Resources.Clear_Recent_Files;
                MenuItem clearItem = new MenuItem { Header = strClearRecentFiles };
                this.recentFilesToolStripMenuItem.Items.Add(clearItem);
                clearItem.Click += eh;
            }
        }

        void MenuRecentFilesOnClick(object obj, EventArgs ea)
        {
            MenuItem item = (MenuItem)obj;
            string fileName = item.Header.ToString();

            if (fileName == strClearRecentFiles)
            {
                mruList.Clear();
                this.recentFilesToolStripMenuItem.Items.Clear();
                this.recentFilesToolStripMenuItem.Items.Add(Properties.Resources.No_Recent_Files);
            }
            else
            {
                openFile(fileName);
                if (!File.Exists(fileName))
                {
                    mruList.Remove(fileName);
                    this.recentFilesToolStripMenuItem.Items.Remove(item);
                }
            }
        }

        /// <summary>
        /// Update MRU List.
        /// </summary>
        /// <param name="fileName"></param>
        private void updateMRUList(string fileName)
        {
            if (mruList.Contains(fileName))
            {
                mruList.Remove(fileName);
            }

            mruList.Insert(0, fileName);

            if (mruList.Count > 10)
            {
                mruList.RemoveAt(10);
            }

            updateMRUMenu();
        }

        protected override bool saveAction()
        {
            if (string.IsNullOrEmpty(textFilename))
            {
                return SaveFileDlg();
            }
            else
            {
                return SaveTextFile(textFilename);
            }
        }

        protected override bool saveAsAction()
        {
            return SaveFileDlg();
        }

        bool SaveFileDlg()
        {
            SaveFileDialog dlg = new SaveFileDialog();

            // Set filter for file extension and default file extension 
            dlg.Filter = "UTF-8 Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            dlg.FilterIndex = 1;
            dlg.DefaultExt = ".txt";
            if (!string.IsNullOrEmpty(textFilename))
            {
                dlg.InitialDirectory = Path.GetDirectoryName(textFilename);
                dlg.FileName = Path.GetFileName(textFilename);
            }

            Nullable<bool> result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                // Save document 
                textFilename = dlg.FileName;
                return SaveTextFile(textFilename);
            }
            else
            {
                return false;
            }
        }

        bool SaveTextFile(string fileName)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                using (StreamWriter sw = new StreamWriter(fileName, false, new System.Text.UTF8Encoding()))
                {
                    sw.Write(this.textBox1.Text);
                    updateMRUList(fileName);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, strProgName, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            textModified = false;
            //this.textBox1.Modified = false;
            this.Cursor = null;

            return true;
        }

        protected override void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;

            // Set filter for file extension and default file extension 
            dlg.Filter = "All Image Files|*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.pdf|Image Files (*.bmp)|*.bmp|Image Files (*.gif)|*.gif|Image Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|Image Files (*.png)|*.png|Image Files (*.tif;*.tiff)|*.tif;*.tiff|PDF Files (*.pdf)|*.pdf|UTF-8 Text Files (*.txt)|*.txt";
            dlg.FilterIndex = filterIndex;
            dlg.DefaultExt = ".png";

            Nullable<bool> result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                OpenFiles(dlg.FileNames);
                filterIndex = dlg.FilterIndex;
            }
        }

        protected override void LoadRegistryInfo(RegistryKey regkey)
        {
            base.LoadRegistryInfo(regkey);
            filterIndex = (int)regkey.GetValue(strFilterIndex, 1);

            string[] fileNames = ((string)regkey.GetValue(strMruList, string.Empty)).Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string fileName in fileNames)
            {
                mruList.Add(fileName);
            }
            updateMRUMenu();
        }

        protected override void SaveRegistryInfo(RegistryKey regkey)
        {
            base.SaveRegistryInfo(regkey);
            regkey.SetValue(strFilterIndex, filterIndex);
            StringBuilder strB = new StringBuilder();
            foreach (string name in mruList)
            {
                strB.Append(name).Append(';');
            }
            regkey.SetValue(strMruList, strB.ToString());
        }
    }
}
