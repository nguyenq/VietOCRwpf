﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VietOCR.NET.Utilities;

namespace VietOCR
{
    /// <summary>
    /// Interaction logic for DownloadDialog.xaml
    /// </summary>
    public partial class DownloadDialog : Window
    {
        Dictionary<string, string> availableLanguageCodes;
        Dictionary<string, string> availableDictionaries;

        public Dictionary<string, string> LookupISO_3_1_Codes
        {
            get;
            set;
        }

        public Dictionary<string, string> LookupISO639
        {
            get;
            set;
        }

        public ObservableCollection<string> InstalledLanguages
        {
            get;
            set;
        }

        List<WebClient> clients;
        Dictionary<string, long> downloadTracker;
        int numberOfDownloads, numOfConcurrentTasks;
        long contentLength;
        string baseDir;
        const string DICTIONARY_FOLDER = "dict";
        const string TESS_DATA = "tessdata";

        public DownloadDialog()
        {
            InitializeComponent();

            baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            clients = new List<WebClient>();
            downloadTracker = new Dictionary<string, long>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string xmlFilePath = Path.Combine(baseDir, "Data/TessLangDataURL.xml");
            availableLanguageCodes = new Dictionary<string, string>();
            VietOCR.NET.Utilities.Utilities.LoadFromXML(availableLanguageCodes, xmlFilePath);

            xmlFilePath = Path.Combine(baseDir, "Data/OO-SpellDictionaries.xml");
            availableDictionaries = new Dictionary<string, string>();
            VietOCR.NET.Utilities.Utilities.LoadFromXML(availableDictionaries, xmlFilePath);

            string[] available = new string[availableLanguageCodes.Count];
            availableLanguageCodes.Keys.CopyTo(available, 0);
            List<string> names = new List<string>();
            foreach (string key in available)
            {
                if (LookupISO639.ContainsKey(key))
                {
                    names.Add(LookupISO639[key]);
                } else
                {
                    names.Add(key);
                }
                
            }
            names.Sort();

            this.listBox.Items.Clear();
            this.listBox.ItemsSource = names.ConvertAll(x => new ListBoxItem()
            {
                Content = x
            });

            List<ListBoxItem> installed = this.listBox.Items.Cast<ListBoxItem>().Where(x => InstalledLanguages.Contains(x.Content)).ToList();
            installed.ForEach(x => x.IsEnabled = false);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (numberOfDownloads > 0)
            {
                string destFolder = Path.Combine(baseDir, TESS_DATA);
                try
                {
                    string downloadFolder = Path.Combine(destFolder, @"tesseract-ocr\tessdata");
                    if (Directory.Exists(downloadFolder))
                    {
                        //In Tesseract 3.02, data is packaged under tesseract-ocr/tessdata directory
                        //After extraction, move them up two levels
                        string[] files = Directory.GetFiles(downloadFolder);
                        foreach (string file in files)
                        {
                            string destFile = Path.Combine(destFolder, Path.GetFileName(file));
                            // Ensure that the target does not exist
                            if (File.Exists(destFile))
                            {
                                File.Delete(destFile);
                            }
                            File.Move(file, destFile);
                        }

                        //remove extraneous directories left by file extraction
                        Directory.Delete(Path.Combine(destFolder, "tesseract-ocr"), true);
                    }
                }
                catch
                {
                    // trap and ignore
                }
                MessageBox.Show(this, Properties.Resources.Please_restart, Gui.strProgName);
            }
        }

        private void buttonDownload_Click(object sender, RoutedEventArgs e)
        {
            if (this.listBox.SelectedIndex == -1)
            {
                return;
            }

            bool isWriteAccess = CheckDirectoryWriteAccess(Path.Combine(baseDir, TESS_DATA));

            if (!isWriteAccess)
            {
                string msg = string.Format(Properties.Resources.Access_denied, Path.Combine(baseDir, TESS_DATA).ToString());
                MessageBox.Show(msg, Gui.strProgName, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.buttonDownload.IsEnabled = false;
            this.buttonCancel.IsEnabled = true;
            this.toolStripProgressBar1.Value = 0;
            this.toolStripProgressBar1.Visibility = Visibility.Visible;
            this.buttonDownload.IsEnabled = false;
            this.toolStripStatusLabel1.Content = Properties.Resources.Downloading;
            this.Cursor = Cursors.Wait;

            clients.Clear();
            downloadTracker.Clear();
            contentLength = 0;
            numOfConcurrentTasks = this.listBox.SelectedItems.Count;

            foreach (ListBoxItem item in this.listBox.SelectedItems)
            {
                string key = FindKey(LookupISO639, item.Content.ToString()); // Vietnamese -> vie
                if (key != null)
                {
                    try
                    {
                        Uri uri = new Uri(availableLanguageCodes[key]);
                        DownloadDataFile(uri, TESS_DATA);  // download language data pack. In Tesseract 3.02, data is packaged under tesseract-ocr/tessdata directory

                        if (LookupISO_3_1_Codes.ContainsKey(key))
                        {
                            string iso_3_1_Code = LookupISO_3_1_Codes[key]; // vie -> vi_VN
                            if (availableDictionaries.ContainsKey(iso_3_1_Code))
                            {
                                uri = new Uri(availableDictionaries[iso_3_1_Code]);
                                ++numOfConcurrentTasks;
                                DownloadDataFile(uri, DICTIONARY_FOLDER); // download dictionary
                            }
                        }
                    }
                    catch (Exception)
                    {
                        if (--numOfConcurrentTasks <= 0)
                        {
                            this.toolStripStatusLabel1.Content = Properties.Resources.Downloaderror;
                            this.toolStripProgressBar1.Visibility = Visibility.Hidden;
                            resetUI();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a folder is writable by a user.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private bool CheckDirectoryWriteAccess(string directory)
        {
            bool writeAccess = false;

            if (Directory.Exists(directory))
            {
                try
                {
                    string tempFile = Path.Combine(directory, Path.GetRandomFileName());
                    using (FileStream fs = File.Create(tempFile)) { }

                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                        writeAccess = true;
                    }
                }
                catch (Exception)
                {
                    writeAccess = false;
                }
            }

            return writeAccess;
        }

        string FindKey(IDictionary<string, string> lookup, string value)
        {
            foreach (var pair in lookup)
            {
                if (pair.Value == value)
                {
                    return pair.Key;
                }
            }
            return null;
        }

        void DownloadDataFile(Uri uri, string destFolder)
        {
            try
            {
                // WebClient can only handle one download at a time.
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(Client_DownloadFileCompleted);
                clients.Add(client);
                WebRequest request = WebRequest.Create(uri);
                request.Timeout = 15000;
                WebResponse response = request.GetResponse();
                contentLength += response.ContentLength;
                response.Close();
                string filePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(uri.AbsolutePath));
                client.DownloadFileAsync(uri, filePath, destFolder + filePath);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("404"))
                {
                    MessageBox.Show(Properties.Resources.Resource_not_exist); //url does not exist
                }
                else
                {
                    MessageBox.Show(e.Message);
                }
                throw e;
            }
        }

        void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            string filePath = e.UserState.ToString();

            if (!downloadTracker.ContainsKey(filePath))
            {
                downloadTracker.Add(filePath, e.BytesReceived);
            }
            else
            {
                downloadTracker[filePath] = e.BytesReceived;
            }

            long totalBytesReceived = 0;
            foreach (int bytesReceived in downloadTracker.Values)
            {
                totalBytesReceived += bytesReceived;
            }

            if (contentLength != 0)
            {
                this.toolStripProgressBar1.Value = (int)(100 * totalBytesReceived / contentLength);
            }
        }

        void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                this.toolStripStatusLabel1.Content = Properties.Resources.Downloadcanceled;
                resetUI();
            }
            else if (e.Error != null)
            {
                this.toolStripProgressBar1.Visibility = Visibility.Hidden;
                this.toolStripStatusLabel1.Content = e.Error.Message;
                resetUI();
            }
            else
            {
                string fileName = e.UserState.ToString();
                if (fileName.StartsWith(DICTIONARY_FOLDER))
                {
                    FileExtractor.ExtractCompressedFile(fileName.Substring(DICTIONARY_FOLDER.Length), Path.Combine(baseDir, DICTIONARY_FOLDER));
                }
                else
                {
                    FileExtractor.ExtractCompressedFile(fileName.Substring(TESS_DATA.Length), Path.Combine(baseDir, TESS_DATA));
                    numberOfDownloads++;
                }

                if (--numOfConcurrentTasks <= 0)
                {
                    this.toolStripStatusLabel1.Content = Properties.Resources.Downloadcompleted;
                    this.toolStripProgressBar1.Visibility = Visibility.Hidden;
                    resetUI();
                }
            }
        }

        void resetUI()
        {
            this.buttonDownload.IsEnabled = true;
            this.buttonCancel.IsEnabled = false;
            this.Cursor = null;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            foreach (WebClient client in clients)
            {
                if (client != null && client.IsBusy)
                {
                    client.CancelAsync();
                    client.Dispose();
                }
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.buttonDownload.IsEnabled = this.listBox.SelectedIndex != -1;
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
