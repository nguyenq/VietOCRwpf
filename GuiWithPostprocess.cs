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
using System.ComponentModel;
using VietOCR.NET.Postprocessing;
using System.IO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace VietOCR
{
    public class GuiWithPostprocess : GuiWithOCR
    {
        const string strDangAmbigsPath = "DangAmbigsPath";
        const string strDangAmbigsOn = "DangAmbigsOn";
        const string strReplaceHyphensEnabled = "ReplaceHyphensEnabled";
        const string strRemoveHyphensEnabled = "RemoveHyphensEnabled";

        protected string dangAmbigsPath;
        protected bool dangAmbigsOn;
        protected ProcessingOptions options;

        private System.ComponentModel.BackgroundWorker backgroundWorkerCorrect;

        public GuiWithPostprocess()
        {
            options = new ProcessingOptions();
            this.backgroundWorkerCorrect = new System.ComponentModel.BackgroundWorker();
            // 
            // backgroundWorkerCorrect
            // 
            this.backgroundWorkerCorrect.WorkerReportsProgress = true;
            this.backgroundWorkerCorrect.WorkerSupportsCancellation = true;
            this.backgroundWorkerCorrect.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerCorrect_DoWork);
            this.backgroundWorkerCorrect.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerCorrect_RunWorkerCompleted);
        }

        protected override void buttonPostProcess_Click(object sender, RoutedEventArgs e)
        {
            postprocessToolStripMenuItem_Click(sender, e);
        }

        protected override void postprocessToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (curLangCode == null) return;

            this.statusLabel.Content = Properties.Resources.Correcting_errors;
            this.Cursor = Cursors.Wait;
            
            this.textBox1.Cursor = Cursors.Wait;
            this.postprocessToolStripMenuItem.IsEnabled = false;
            this.toolStripProgressBar1.IsEnabled = true;
            this.toolStripProgressBar1.Visibility = Visibility.Visible;

            this.backgroundWorkerCorrect.RunWorkerAsync(this.textBox1.SelectionLength > 0 ? this.textBox1.SelectedText : this.textBox1.Text);
        }

        private void backgroundWorkerCorrect_DoWork(object sender, DoWorkEventArgs e)
        {
            // Perform post-OCR corrections
            string text = (string)e.Argument;
            e.Result = Processor.PostProcess(text, curLangCode, dangAmbigsPath, dangAmbigsOn, options.ReplaceHyphens);
        }

        private void backgroundWorkerCorrect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.toolStripProgressBar1.IsEnabled = false;
            this.toolStripProgressBar1.Visibility = Visibility.Hidden;

            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                Console.WriteLine(e.Error.StackTrace);
                string why;

                if (e.Error.GetBaseException() is NotSupportedException)
                {
                    why = string.Format("Post-processing not supported for {0} language.\nYou can provide one via a \"{1}.DangAmbigs.txt\" file.", this.dataSource.SelectedLanguagesText, curLangCode);
                }
                else
                {
                    why = e.Error.Message;
                }
                MessageBox.Show(this, why, strProgName);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled 
                // the operation.
                // Note that due to a race condition in 
                // the DoWork event handler, the Cancelled
                // flag may not have been set, even though
                // CancelAsync was called.
                this.statusLabel.Content = "Post-OCR correction " + Properties.Resources.canceled;
            }
            else
            {
                // Finally, handle the case where the operation 
                // succeeded.
                string result = e.Result.ToString();

                if (this.textBox1.SelectionLength > 0)
                {
                    int start = this.textBox1.SelectionStart;
                    this.textBox1.SelectedText = result;
                    this.textBox1.Select(start, result.Length);
                }
                else
                {
                    this.textBox1.Text = result;
                }
                this.statusLabel.Content = Properties.Resources.Correcting_completed;
            }

            this.Cursor = null;
            this.textBox1.Cursor = null;
            this.postprocessToolStripMenuItem.IsEnabled = true;
        }

        protected override void LoadRegistryInfo(RegistryKey regkey)
        {
            base.LoadRegistryInfo(regkey);

            dangAmbigsPath = (string)regkey.GetValue(strDangAmbigsPath, Path.Combine(baseDir, "Data"));
            dangAmbigsOn = Convert.ToBoolean(
                (int)regkey.GetValue(strDangAmbigsOn, Convert.ToInt32(true)));
            options.ReplaceHyphens = Convert.ToBoolean(
                (int)regkey.GetValue(strReplaceHyphensEnabled, Convert.ToInt32(true)));
            options.RemoveHyphens = Convert.ToBoolean(
                (int)regkey.GetValue(strRemoveHyphensEnabled, Convert.ToInt32(true)));
        }

        protected override void SaveRegistryInfo(RegistryKey regkey)
        {
            base.SaveRegistryInfo(regkey);

            regkey.SetValue(strDangAmbigsPath, dangAmbigsPath);
            regkey.SetValue(strDangAmbigsOn, Convert.ToInt32(dangAmbigsOn));
            regkey.SetValue(strReplaceHyphensEnabled, Convert.ToInt32(options.ReplaceHyphens));
            regkey.SetValue(strRemoveHyphensEnabled, Convert.ToInt32(options.RemoveHyphens));
        }
    }
}
