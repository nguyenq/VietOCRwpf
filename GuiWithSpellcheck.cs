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
using Net.SourceForge.Vietpad.Utilities;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Documents;

namespace VietOCR
{
    public class GuiWithSpellcheck : GuiWithOEM
    {
        private int start, end;
        private SpellCheckHelper speller;
        private string curWord;

        public GuiWithSpellcheck()
        {

        }

        protected override void textBox1_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                this.textBox1.ContextMenu.Items.Clear();
                if (this.buttonSpellcheck.IsChecked.Value)
                {
                    int offset = this.textBox1.CaretIndex;
                    BreakIterator boundary = BreakIterator.GetWordInstance();
                    boundary.Text = this.textBox1.Text;
                    end = boundary.Following(offset);

                    if (end != BreakIterator.DONE)
                    {
                        start = boundary.Previous();
                        curWord = this.textBox1.Text.Substring(start, end - start);
                        makeSuggestions(curWord);
                    }
                }
            }
            finally
            {
                // load standard menu items
                RepopulateDefaultContextMenu();
            }
        }

        void RepopulateDefaultContextMenu()
        {
            MenuItem item = new MenuItem();
            //item.Header = "Undo";
            item.Command = ApplicationCommands.Undo;
            this.textBox1.ContextMenu.Items.Add(item);
            item = new MenuItem();
            //item.Header = "Undo";
            item.Command = ApplicationCommands.Redo;
            this.textBox1.ContextMenu.Items.Add(item);
            this.textBox1.ContextMenu.Items.Add(new Separator());
            item = new MenuItem();
            //item.Header = "Undo";
            item.Command = ApplicationCommands.Cut;
            this.textBox1.ContextMenu.Items.Add(item);
            item = new MenuItem();
            //item.Header = "Undo";
            item.Command = ApplicationCommands.Copy;
            this.textBox1.ContextMenu.Items.Add(item);
            item = new MenuItem();
            //item.Header = "Undo";
            item.Command = ApplicationCommands.Paste;
            this.textBox1.ContextMenu.Items.Add(item);
            item = new MenuItem();
            //item.Header = "Undo";
            item.Command = ApplicationCommands.Delete;
            this.textBox1.ContextMenu.Items.Add(item);
            this.textBox1.ContextMenu.Items.Add(new Separator());
            item = new MenuItem();
            //item.Header = "Undo";
            item.Command = ApplicationCommands.SelectAll;
            this.textBox1.ContextMenu.Items.Add(item);
        }

        /// <summary>
        /// Populates suggestions at top of context menu.
        /// </summary>
        /// <param name="curWord"></param>
        void makeSuggestions(string curWord)
        {
            if (speller == null || curWord == null || curWord.Trim().Length == 0)
            {
                return;
            }

            List<String> suggests = speller.Suggest(curWord);
            if (suggests == null || suggests.Count == 0)
            {
                return;
            }

            foreach (string word in suggests)
            {
                MenuItem item = new MenuItem();
                item.Header = word;
                item.FontWeight = FontWeights.Bold;
                item.Tag = EditingCommands.CorrectSpellingError;
                item.Click += new RoutedEventHandler(item_Click);
                this.textBox1.ContextMenu.Items.Add(item);
            }
            this.textBox1.ContextMenu.Items.Add(new Separator());

            MenuItem item1 = new MenuItem();
            item1.Header = "Ignore All";
            item1.Tag = EditingCommands.IgnoreSpellingError;
            item1.Click += new RoutedEventHandler(item_Click);
            this.textBox1.ContextMenu.Items.Add(item1);

            item1 = new MenuItem();
            item1.Header = "Add to Dictionary";
            item1.Tag = "AddToDict";
            item1.Click += new RoutedEventHandler(item_Click);
            this.textBox1.ContextMenu.Items.Add(item1);
            this.textBox1.ContextMenu.Items.Add(new Separator());
        }

        void item_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;

            if (item.Tag == EditingCommands.CorrectSpellingError)
            {
                this.textBox1.Select(start, end - start);
                this.textBox1.SelectedText = item.Header.ToString();
                this.textBox1.SelectionStart = start + item.Header.ToString().Length;
                this.textBox1.SelectionLength = 0;
            }
            else if (item.Tag == EditingCommands.IgnoreSpellingError)
            {
                speller.IgnoreWord(curWord);
            }
            else if (item.Tag.ToString() == "AddToDict")
            {
                speller.AddWord(curWord);
            }

            speller.SpellCheck();
        }

        protected override void buttonSpellcheck_Click(object sender, RoutedEventArgs e)
        {
            string localeId = null;

            if (LookupISO_3_1_Codes.ContainsKey(curLangCode))
            {
                localeId = LookupISO_3_1_Codes[curLangCode];
            }
            else if (LookupISO_3_1_Codes.ContainsKey(curLangCode.Substring(0, 3)))
            {
                localeId = LookupISO_3_1_Codes[curLangCode.Substring(0, 3)];
            }

            if (localeId == null)
            {
                MessageBox.Show("Need to add an entry in Data/ISO639-1.xml file.");
                return;
            }

            speller = new SpellCheckHelper(this.textBox1, localeId);

            if (this.buttonSpellcheck.IsChecked.Value)
            {
                try
                {
                    speller.EnableSpellCheck();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                speller.DisableSpellCheck();
            }
        }
    }
}
