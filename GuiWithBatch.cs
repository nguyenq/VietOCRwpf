﻿/**
 * Copyright @ 2012 Quan Nguyen
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
using VietOCR.NET.Utilities;
using System.Threading;
using System.IO;
using VietOCR.NET.Postprocessing;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using VietOCR.NET;

namespace VietOCR
{
    public class GuiWithBatch : GuiWithSettings
    {
        private Queue<String> queue;
        private Watcher watcher;
        private DispatcherTimer aTimer;
        private StatusForm statusForm;

        delegate void UpdateStatusEvent(string message);

        public GuiWithBatch()
        {
            statusForm = new StatusForm();
            statusForm.Title = Properties.Resources.BatchProcessStatus;
        }

        protected override void Window_Loaded(object sender, RoutedEventArgs e)
        {
            base.Window_Loaded(sender, e);

            queue = new Queue<String>();
            watcher = new Watcher(queue, watchFolder);
            watcher.Enabled = watchEnabled;

            aTimer = new DispatcherTimer();
            aTimer.Interval = new TimeSpan(10000);
            aTimer.Tick += new EventHandler(OnTimedEvent);
            if (watchEnabled)
            {
                aTimer.Start();
            }
        }

        private void OnTimedEvent(Object sender, EventArgs e)
        {
            if (queue.Count > 0)
            {
                if (!this.statusForm.IsVisible)
                {
                    this.statusForm.Show();
                }
                else if (this.statusForm.WindowState == WindowState.Minimized)
                {
                    this.statusForm.WindowState = WindowState.Normal;
                }

                Thread t = new Thread(new ThreadStart(AutoOCR));
                t.Start();
            }
        }

        private void AutoOCR()
        {
            FileInfo imageFile;
            try
            {
                imageFile = new FileInfo(queue.Dequeue());
                if (imageFile == null || !imageFile.Exists)
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            Dispatcher.BeginInvoke(new Action<string>(WorkerUpdate), DispatcherPriority.Normal, imageFile.FullName);

            if (curLangCode == null)
            {
                Dispatcher.BeginInvoke(new Action<string>(WorkerUpdate), DispatcherPriority.Normal, "\t** " + Properties.Resources.selectLanguage + " **");
                //queue.Clear();
                return;
            }

            try
            {
                OCRHelper.PerformOCR(imageFile.FullName, Path.Combine(outputFolder, imageFile.Name), curLangCode, selectedPSM, outputFormat, options);
            }
            catch
            {
                // Sets the UI culture to the selected language.
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(selectedUILanguage);
                Dispatcher.BeginInvoke(new Action<string>(WorkerUpdate), DispatcherPriority.Normal, "\t** " + Properties.Resources.Cannotprocess + imageFile.Name + " **");
            }
        }

        void WorkerUpdate(string message)
        {
            this.statusForm.TextBox.AppendText(message + Environment.NewLine);
        }

        protected override void updateWatch()
        {
            watcher.Path = watchFolder;
            watcher.Enabled = watchEnabled;
            if (watchEnabled)
            {
                aTimer.Start();
            }
            else
            {
                aTimer.Stop();
            }
        }
    }
}
