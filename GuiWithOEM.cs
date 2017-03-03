/**
 * Copyright @ 2017 Quan Nguyen
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
//using Vietpad.NET.Controls;
using Microsoft.Win32;
using Tesseract;
using System.Windows.Controls;
using System.Windows;

namespace VietOCR
{
    public class GuiWithOEM : GuiWithPSM
    {
        const string strOEM = "OcrEngineMode";
        MenuItem oemItemChecked;

        public GuiWithOEM()
        {
            Dictionary<string, string> oemDict = new Dictionary<string, string>();
            oemDict.Add("TesseractOnly", "0 - Tesseract only");
            oemDict.Add("CubeOnly", "1 - Cube only");
            oemDict.Add("TesseractAndCube", "2 - Tesseract & Cube");
            oemDict.Add("Default", "3 - Default");

            //
            // Settings EngineMode submenu
            //
            RoutedEventHandler eh = new RoutedEventHandler(MenuOEMOnClick);

            foreach (string mode in Enum.GetNames(typeof(EngineMode)))
            {
                MenuItem oemItem = new MenuItem();
                oemItem.Header = oemDict[mode];
                oemItem.Tag = mode;
                oemItem.Click += eh;
                this.oemToolStripMenuItem.Items.Add(oemItem);
            }
        }

        protected override void Window_Loaded(object sender, RoutedEventArgs e)
        {
            base.Window_Loaded(sender, e);

            for (int i = 0; i < this.oemToolStripMenuItem.Items.Count; i++)
            {
                if (((MenuItem)this.oemToolStripMenuItem.Items[i]).Tag.ToString() == selectedOEM)
                {
                    // Select PSM last saved
                    oemItemChecked = (MenuItem)oemToolStripMenuItem.Items[i];
                    oemItemChecked.IsChecked = true;
                    break;
                }
            }

            this.statusLabelOEMvalue.Content = selectedOEM;
        }

        void MenuOEMOnClick(object obj, EventArgs ea)
        {
            if (oemItemChecked != null)
            {
                oemItemChecked.IsChecked = false;
            }
            oemItemChecked = (MenuItem)obj;
            oemItemChecked.IsChecked = true;
            selectedOEM = oemItemChecked.Tag.ToString();
            this.statusLabelOEMvalue.Content = selectedOEM;
        }

        protected override void LoadRegistryInfo(RegistryKey regkey)
        {
            base.LoadRegistryInfo(regkey);
            selectedOEM = (string)regkey.GetValue(strOEM, Enum.GetName(typeof(EngineMode), Tesseract.EngineMode.Default));
            try
            {
                // validate OEM value
                Tesseract.EngineMode oem = (EngineMode)Enum.Parse(typeof(EngineMode), selectedOEM);
            }
            catch
            {
                selectedOEM = Enum.GetName(typeof(EngineMode), Tesseract.EngineMode.Default);
            }
        }

        protected override void SaveRegistryInfo(RegistryKey regkey)
        {
            base.SaveRegistryInfo(regkey);
            regkey.SetValue(strOEM, selectedOEM);
        }
    }
}
