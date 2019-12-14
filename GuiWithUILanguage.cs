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
using System.Globalization;
//using Vietpad.NET.Controls;
using System.Windows;
using System.Windows.Controls;

namespace VietOCR
{
    public class GuiWithUILanguage : GuiWithInputMethod
    {
        MenuItem miuilChecked;

        public GuiWithUILanguage()
        {

            //
            // Settings UI Language submenu
            //
            RoutedEventHandler eh = new RoutedEventHandler(MenuKeyboardUILangOnClick);

            List<MenuItem> ar = new List<MenuItem>();

            String[] uiLangs = { "bn-IN", "ca-ES", "cs-CZ", "en-US", "es-ES", "de-DE", "fa-IR", "hi-IN", "it-IT", "ja-JP", "kn-In", "lt-LT", "ne-NP", "nl-NL", "pl-PL", "ru-RU", "sd-Deva", "sk-SK", "tr-TR", "vi-VN" }; // "bn-IN" caused exception on WinXP .NET 2.0
            foreach (string uiLang in uiLangs)
            {
                MenuItem miuil = new MenuItem();
                CultureInfo ci = new CultureInfo(uiLang);
                miuil.Tag = ci.Name;
                miuil.Header = ci.Parent.DisplayName + " (" + ci.Parent.NativeName + ")";
                if (ci.Parent.DisplayName.StartsWith("Invariant Language"))
                {
                    miuil.Header = ci.EnglishName.Substring(0, ci.EnglishName.IndexOf("(") - 1) + " (" + ci.NativeName.Substring(0, ci.NativeName.IndexOf("(") - 1) + ")";
                }
                miuil.Click += eh;
                ar.Add(miuil);
                this.uiLanguageToolStripMenuItem.Items.Add(miuil);
            }
        }

        protected override void Window_Loaded(object sender, RoutedEventArgs e)
        {
            base.Window_Loaded(sender, e);

            for (int i = 0; i < this.uiLanguageToolStripMenuItem.Items.Count; i++)
            {
                if (((MenuItem)this.uiLanguageToolStripMenuItem.Items[i]).Tag.ToString() == selectedUILanguage)
                {
                    // Select UI Language last saved
                    miuilChecked = (MenuItem)uiLanguageToolStripMenuItem.Items[i];
                    miuilChecked.IsChecked = true;
                    break;
                }
            }
        }

        void MenuKeyboardUILangOnClick(object obj, EventArgs ea)
        {
            if (miuilChecked != null)
            {
                miuilChecked.IsChecked = false;
            }
            
            miuilChecked = (MenuItem)obj;
            miuilChecked.IsChecked = true;
            if (selectedUILanguage != miuilChecked.Tag.ToString())
            {
                selectedUILanguage = miuilChecked.Tag.ToString();
                //ChangeUILanguage(selectedUILanguage);
                App.ChangeCulture(new CultureInfo(selectedUILanguage));
            }
        }
    }
}
