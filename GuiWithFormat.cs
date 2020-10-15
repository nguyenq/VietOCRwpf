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
using System.Drawing;
using Microsoft.Win32;
using System.Globalization;
using Net.SourceForge.Vietpad.Utilities;
using VietOCR.NET.Utilities;
using System.Windows.Controls;
using System.Windows;

namespace VietOCR
{
    public class GuiWithFormat : GuiWithImage
    {
        const string strSelectedCase = "SelectedCase";

        private string selectedCase;

        public GuiWithFormat()
        {

        }
        /// <summary>
        /// Changes localized text and messages
        /// </summary>
        /// <param name="locale"></param>
        protected override void ChangeUILanguage(string locale)
        {
            base.ChangeUILanguage(locale);

            foreach (Window form in this.OwnedWindows)
            {
                ChangeCaseDialog changeCaseDlg = form as ChangeCaseDialog;
                if (changeCaseDlg != null)
                {
                    FormLocalizer localizer = new FormLocalizer(changeCaseDlg, typeof(ChangeCaseDialog));
                    localizer.ApplyCulture(new CultureInfo(locale));
                    break;
                }
            }
        }
        protected override void wordWrapToolStripMenuItem_Click(object sender,RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            mi.IsChecked ^= true;
            this.textBox1.TextWrapping = mi.IsChecked ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }

        protected override void fontToolStripMenuItem_Click(object sender,RoutedEventArgs e)
        {
            System.Windows.Forms.FontDialog fontdlg = new System.Windows.Forms.FontDialog();

            fontdlg.ShowColor = true;
            fontdlg.Font = new Font(this.textBox1.FontFamily.ToString(), (float) (this.textBox1.FontSize * 72.0 / 96.0), this.textBox1.FontWeight == FontWeights.Bold ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular);
            System.Windows.Media.Color textColor = ((System.Windows.Media.SolidColorBrush)this.textBox1.Foreground).Color;
            fontdlg.Color = Color.FromArgb(textColor.R, textColor.G, textColor.B);
            if (fontdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.textBox1.FontFamily = new System.Windows.Media.FontFamily(fontdlg.Font.Name);
                this.textBox1.FontSize = fontdlg.Font.Size * 96.0 / 72.0;
                this.textBox1.FontWeight = fontdlg.Font.Bold ? FontWeights.Bold : FontWeights.Regular;
                this.textBox1.FontStyle = fontdlg.Font.Italic ? FontStyles.Italic : FontStyles.Normal;
                this.textBox1.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(fontdlg.Color.A, fontdlg.Color.R, fontdlg.Color.G, fontdlg.Color.B));
            }
        }

        protected override void changeCaseToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (OwnedWindows.Count > 0)
            {
                foreach (Window form in this.OwnedWindows)
                {
                    ChangeCaseDialog changeCaseDlg1 = form as ChangeCaseDialog;
                    if (changeCaseDlg1 != null)
                    {
                        changeCaseDlg1.Show();
                        return;
                    }
                }
            }

            //textBox1.HideSelection = false;

            ChangeCaseDialog changeCaseDlg = new ChangeCaseDialog();
            changeCaseDlg.Owner = this;
            changeCaseDlg.SelectedCase = selectedCase;
            changeCaseDlg.ChangeCase += new RoutedEventHandler(ChangeCaseDialogChangeCase);
            changeCaseDlg.CloseDlg += new RoutedEventHandler(ChangeCaseDialogCloseDlg);

            if (textBox1.SelectedText == "")
            {
                textBox1.SelectAll();
            }
            changeCaseDlg.Show();
        }

        void ChangeCaseDialogChangeCase(object obj,RoutedEventArgs ea)
        {
            if (textBox1.SelectedText == "")
            {
                textBox1.SelectAll();
                return;
            }

            ChangeCaseDialog dlg = (ChangeCaseDialog)obj;
            selectedCase = dlg.SelectedCase;
            changeCase(selectedCase);
        }
        
        void ChangeCaseDialogCloseDlg(object obj,RoutedEventArgs ea)
        {
            //textBox1.HideSelection = true;
            this.Focus();
        }

        /// <summary>
        /// Changes letter case.
        /// </summary>
        /// <param name="typeOfCase"></param>
        private void changeCase(string typeOfCase)
        {
            int start = textBox1.SelectionStart;
            string result = TextUtilities.ChangeCase(textBox1.SelectedText, typeOfCase);
            textBox1.SelectedText = result;
            textBox1.Select(start, result.Length);
        }

        /// <summary>
        /// Removes line breaks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void removeLineBreaksToolStripMenuItem_Click(object sender,RoutedEventArgs e)
        {
            if (textBox1.SelectedText == "")
            {
                textBox1.SelectAll();
                if (textBox1.SelectedText == "") return;
            }

            int start = textBox1.SelectionStart;
            string result = TextUtilities.RemoveLineBreaks(textBox1.SelectedText, options.RemoveHyphens);
            textBox1.SelectedText = result;
            textBox1.Select(start, result.Length);
        }

        protected override void LoadRegistryInfo(RegistryKey regkey)
        {
            base.LoadRegistryInfo(regkey);
            selectedCase = (string)regkey.GetValue(strSelectedCase, String.Empty);
        }

        protected override void SaveRegistryInfo(RegistryKey regkey)
        {
            base.SaveRegistryInfo(regkey);
            regkey.SetValue(strSelectedCase, selectedCase);
        }
    }
}
