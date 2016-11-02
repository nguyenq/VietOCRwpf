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

namespace VietOCR
{
    public class GuiWithSpellcheck : GuiWithPSM
    {
        private int start, end;
        private SpellCheckHelper speller;
        private string curWord;

        public GuiWithSpellcheck()
        {

        }

        //protected override void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        //{
        //    try
        //    {
        //        this.contextMenuStrip1.Items.Clear();
        //        if (this.buttonSpellCheck.Checked)
        //        {
        //            int offset = this.textBox1.GetCharIndexFromPosition(pointClicked);
        //            BreakIterator boundary = BreakIterator.GetWordInstance();
        //            boundary.Text = this.textBox1.Text;
        //            end = boundary.Following(offset);

        //            if (end != BreakIterator.DONE)
        //            {
        //                start = boundary.Previous();
        //                curWord = this.textBox1.Text.Substring(start, end - start);
        //                makeSuggestions(curWord);
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        // load standard menu items
        //        this.contextMenuStrip1.RepopulateContextMenu();
        //    }
        //}
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

            //foreach (string word in suggests)
            //{
            //    MenuItem item = new MenuItem(word);
            //    item.Font = new Font(item.Font, FontStyle.Bold);
            //    item.Click += new EventHandler(item_Click);
            //    this.contextMenuStrip1.Items.Add(item);
            //}
            //this.contextMenuStrip1.Items.Add("-");

            //MenuItem item1 = new MenuItem(Properties.Resources.Ignore_All);
            //item1.Tag = "ignore.word";
            //item1.Click += new EventHandler(item_Click);
            //this.contextMenuStrip1.Items.Add(item1);

            //item1 = new MenuItem(Properties.Resources.Add_to_Dictionary);
            //item1.Tag = "add.word";
            //item1.Click += new EventHandler(item_Click);
            //this.contextMenuStrip1.Items.Add(item1);
            //this.contextMenuStrip1.Items.Add("-");
        }

        void item_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            object command = item.Tag;

            if (command == null)
            {
                this.textBox1.Select(start, end - start);
                this.textBox1.SelectedText = item.Header.ToString();
            }
            else if (command.ToString() == "ignore.word")
            {
                speller.IgnoreWord(curWord);
            }
            else if (command.ToString() == "add.word")
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

            System.Collections.IList dictionaries = SpellCheck.GetCustomDictionaries(textBox1);
            try
            {            
                // *.dic is included as content files
                Uri uri = new Uri(String.Format(@"pack://application:,,,/dict/{0}.dic", localeId));
                dictionaries.Add(uri);
                this.textBox1.SpellCheck.IsEnabled = this.buttonSpellcheck.IsChecked.HasValue ? this.buttonSpellcheck.IsChecked.Value : false;
            }
            catch (System.IO.IOException exc)
            {
                MessageBox.Show(exc.Message);
            }
        }
    }
}
