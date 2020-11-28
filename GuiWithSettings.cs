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
using System.Globalization;
using System.Windows;
using System.IO;

namespace VietOCR
{
    public class GuiWithSettings : GuiWithUILanguage
    {
        const string strWatchEnabled = "WatchEnabled";
        const string strDeskewEnabled = "DeskewEnabled";
        const string strWatchFolder = "WatchFolder";
        const string strOutputFolder = "OutputFolder";
        const string strBatchOutputFormat = "BatchOutputFormat";
        const string strPostProcessingEnabled = "PostProcessingEnabled";
        const string strCorrectLetterCasesEnabled = "CorrectLetterCasesEnabled";
        const string strTextOnlyPdfEnabled = "TextOnlyPdfEnabled";
        const string strRemoveLinesEnabled = "RemoveLinesEnabled";
        const string strRemoveLineBreaksEnabled = "RemoveLineBreaksEnabled";

        protected string watchFolder;
        protected string outputFolder;
        protected bool watchEnabled;
        protected string outputFormat;

        public GuiWithSettings()
        {

        }
        
        protected override void optionsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OptionsDialog optionsDialog = new OptionsDialog { Owner = this };
            optionsDialog.WatchFolder = watchFolder;
            optionsDialog.OutputFolder = outputFolder;
            optionsDialog.WatchEnabled = watchEnabled;
            optionsDialog.DangAmbigsPath = dangAmbigsPath;
            optionsDialog.DangAmbigsEnabled = dangAmbigsOn;
            optionsDialog.CurLangCode = curLangCode;
            optionsDialog.ProcessingOptions = options;
            optionsDialog.OutputFormat = outputFormat;
            optionsDialog.SelectedTab = e.Source.GetType().Name != "Button" ? 0 : 2;

            Nullable<bool> dialogResult = optionsDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                watchFolder = optionsDialog.WatchFolder;
                outputFolder = optionsDialog.OutputFolder;
                watchEnabled = optionsDialog.WatchEnabled;
                dangAmbigsPath = optionsDialog.DangAmbigsPath;
                dangAmbigsOn = optionsDialog.DangAmbigsEnabled;
                curLangCode = optionsDialog.CurLangCode;
                options = optionsDialog.ProcessingOptions;
                outputFormat = optionsDialog.OutputFormat;

                updateWatch();
            }
        }

        protected virtual void updateWatch()
        {
        }

        protected override void downloadLangDataToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DownloadDialog downloadDialog = new DownloadDialog();
            downloadDialog.Owner = this;
            downloadDialog.LookupISO639 = LookupISO639;
            downloadDialog.LookupISO_3_1_Codes = LookupISO_3_1_Codes;
            downloadDialog.InstalledLanguages = dataSource.InstalledLanguages;
            downloadDialog.ShowDialog();
        }

        protected override void LoadRegistryInfo(RegistryKey regkey)
        {
            base.LoadRegistryInfo(regkey);
            watchEnabled = Convert.ToBoolean((int)regkey.GetValue(strWatchEnabled, Convert.ToInt32(false)));
            options.Deskew = Convert.ToBoolean((int)regkey.GetValue(strDeskewEnabled, Convert.ToInt32(false)));
            options.PostProcessing = Convert.ToBoolean((int)regkey.GetValue(strPostProcessingEnabled, Convert.ToInt32(false)));
            options.CorrectLetterCases = Convert.ToBoolean((int)regkey.GetValue(strCorrectLetterCasesEnabled, Convert.ToInt32(false)));
            options.TextOnlyPdf = Convert.ToBoolean((int)regkey.GetValue(strTextOnlyPdfEnabled, Convert.ToInt32(false)));
            options.RemoveLines = Convert.ToBoolean((int)regkey.GetValue(strRemoveLinesEnabled, Convert.ToInt32(false)));
            options.RemoveLineBreaks = Convert.ToBoolean((int)regkey.GetValue(strRemoveLineBreaksEnabled, Convert.ToInt32(false)));
            watchFolder = (string)regkey.GetValue(strWatchFolder, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            if (!Directory.Exists(watchFolder)) watchFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            outputFolder = (string)regkey.GetValue(strOutputFolder, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            if (!Directory.Exists(outputFolder)) outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            outputFormat = (string)regkey.GetValue(strBatchOutputFormat, "text");
        }

        protected override void SaveRegistryInfo(RegistryKey regkey)
        {
            base.SaveRegistryInfo(regkey);
            regkey.SetValue(strWatchEnabled, Convert.ToInt32(watchEnabled));
            regkey.SetValue(strDeskewEnabled, Convert.ToInt32(options.Deskew));
            regkey.SetValue(strPostProcessingEnabled, Convert.ToInt32(options.PostProcessing));
            regkey.SetValue(strCorrectLetterCasesEnabled, Convert.ToInt32(options.CorrectLetterCases));
            regkey.SetValue(strTextOnlyPdfEnabled, Convert.ToInt32(options.TextOnlyPdf));
            regkey.SetValue(strRemoveLinesEnabled, Convert.ToInt32(options.RemoveLines));
            regkey.SetValue(strRemoveLineBreaksEnabled, Convert.ToInt32(options.RemoveLineBreaks));
            regkey.SetValue(strWatchFolder, watchFolder);
            regkey.SetValue(strOutputFolder, outputFolder);
            regkey.SetValue(strBatchOutputFormat, outputFormat);
        }
    }
}
