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
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows;

using Net.SourceForge.Vietpad.InputMethod;

namespace VietOCR
{
    public partial class GuiWithInputMethod : GuiWithFormat
    {
        MenuItem miimChecked;

        private string selectedInputMethod;
        const string strInputMethod = "InputMethod";

        public GuiWithInputMethod()
        {
            //
            // Settings InputMethod submenu
            //
            RoutedEventHandler eh = new RoutedEventHandler(MenuKeyboardInputMethodOnClick);

            List<MenuItem> ar = new List<MenuItem>();

            foreach (string inputMethod in Enum.GetNames(typeof(InputMethods)))
            {
                MenuItem miim = new MenuItem();
                miim.Header = inputMethod;
                miim.IsCheckable = true;
                miim.Click += eh;
                ar.Add(miim);
            }

            this.vietInputMethodToolStripMenuItem.ItemsSource = ar;

            new VietKeyHandler(textBox1);
        }

        protected override void Window_Loaded(object sender, RoutedEventArgs e)
        {
            base.Window_Loaded(sender, e);

            for (int i = 0; i < this.vietInputMethodToolStripMenuItem.Items.Count; i++)
            {
                if (((MenuItem)this.vietInputMethodToolStripMenuItem.Items[i]).Header.ToString() == selectedInputMethod)
                {
                    // Select InputMethod last saved
                    miimChecked = (MenuItem)vietInputMethodToolStripMenuItem.Items[i];
                    miimChecked.IsChecked = true;
                    break;
                }
            }

            VietKeyHandler.InputMethod = (InputMethods)Enum.Parse(typeof(InputMethods), selectedInputMethod);
            VietKeyHandler.SmartMark = true;
            VietKeyHandler.ConsumeRepeatKey = true;
        }

        void MenuKeyboardInputMethodOnClick(object obj, EventArgs ea)
        {
            miimChecked.IsChecked = false;
            miimChecked = (MenuItem)obj;
            miimChecked.IsChecked = true;
            selectedInputMethod = miimChecked.Header.ToString();
            VietKeyHandler.InputMethod = (InputMethods)Enum.Parse(typeof(InputMethods), selectedInputMethod);
        }

        protected override void LoadRegistryInfo(RegistryKey regkey)
        {
            base.LoadRegistryInfo(regkey);
            selectedInputMethod = (string)regkey.GetValue(strInputMethod, Enum.GetName(typeof(InputMethods), InputMethods.Telex));
        }

        protected override void SaveRegistryInfo(RegistryKey regkey)
        {
            base.SaveRegistryInfo(regkey);
            regkey.SetValue(strInputMethod, selectedInputMethod);
        }
        
    }
}

