using Microsoft.Win32;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

using Net.SourceForge.Vietpad.Utilities;

namespace VietOCR
{
    public class GuiWithFindReplace : GuiWithPostprocess
    {
        string strFind = String.Empty, strReplace = String.Empty;
        string textFind = String.Empty, textReplace = String.Empty;
        //ComboBox.ObjectCollection itemsFind, itemsReplace;

        bool bMatchCase = false, bMatchDiacritics = false, bMatchRegex = false, bSearchDown = true;
        bool? bMatchWholeWord = false;

        const string strMatchCase = "MatchCase";
        const string strMatchWholeWord = "MatchWholeWord";
        const string strMatchDiacritics = "MatchDiacritics";
        const string strMatchRegex = "MatchRegex";

        string searchData;
        string selectedText;

        public GuiWithFindReplace()
        {

        }

        protected override void buttonFind_Click(object sender, RoutedEventArgs e)
        {
            if (OwnedWindows.Count > 0)
            {
                foreach (Window form in this.OwnedWindows)
                {
                    FindReplaceDialog findDlg1 = form as FindReplaceDialog;
                    if (findDlg1 != null)
                    {
                        findDlg1.Show();
                        return;
                    }
                }
            }

            FindReplaceDialog frDlg = new FindReplaceDialog();
            frDlg.Owner = this;

            //textBox1.HideSelection = false;
            frDlg.FindText = textFind;
            //if (itemsFind != null)
            //    frDlg.FindCollection = itemsFind; // restore memorized find strings
            frDlg.ReplaceText = textReplace;
            //if (itemsReplace != null)
            //    frDlg.ReplaceCollection = itemsReplace; // restore memorized replace strings
            frDlg.MatchCase = bMatchCase;
            frDlg.MatchWholeWord = bMatchWholeWord;
            frDlg.MatchDiacritics = bMatchDiacritics;
            frDlg.MatchRegex = bMatchRegex;
            frDlg.SearchDown = bSearchDown;
            frDlg.FindNext += new RoutedEventHandler(FindDialogOnFindNext);
            frDlg.Replace += new RoutedEventHandler(ReplaceDialogOnReplace);
            frDlg.ReplaceAll += new RoutedEventHandler(ReplaceDialogOnReplaceAll);
            frDlg.CloseDlg += new RoutedEventHandler(FindReplaceDialogOnCloseDlg);

            frDlg.Show();
        }

        void FindDialogOnFindNext(object obj, RoutedEventArgs ea)
        {
            FindReplaceDialog dlg = (FindReplaceDialog)obj;
            textFind = dlg.FindText;
            //itemsFind = dlg.FindCollection; // memorize find strings
            strFind = textFind;

            bMatchCase = dlg.MatchCase;
            bMatchWholeWord = dlg.MatchWholeWord;
            bMatchRegex = dlg.MatchRegex;
            bMatchDiacritics = dlg.MatchDiacritics;
            bSearchDown = dlg.SearchDown;

            FindNext();
        }

        bool FindNext()
        {
            if (!bMatchDiacritics)
            {
                searchData = VietUtilities.StripDiacritics(textBox1.Text);
                strFind = VietUtilities.StripDiacritics(strFind);
            }
            else
            {
                searchData = textBox1.Text;
            }

            if (bSearchDown)
            {
                int iStart = textBox1.SelectionStart + textBox1.SelectionLength;

                if (bMatchRegex || (bMatchWholeWord.HasValue && bMatchWholeWord.Value))
                {
                    try
                    {
                        if (bMatchWholeWord.HasValue && bMatchWholeWord.Value)
                        {
                            strFind = "\\b" + strFind + "\\b";
                        }
                        Regex regex = new Regex((bMatchCase ? string.Empty : "(?i)") + strFind, RegexOptions.Multiline);
                        Match m = regex.Match(searchData, iStart);
                        if (m.Success)
                        {
                            textBox1.SelectionStart = m.Index;
                            textBox1.SelectionLength = m.Length;
                            textBox1.Focus();
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        //logger.Error(e);
                        MessageBox.Show(e.Message, Properties.Resources.Regex_Error);
                        return false;
                    }
                }
                else
                {
                    while (iStart + strFind.Length <= textBox1.Text.Length)
                    {
                        if (String.Compare(strFind, 0, searchData, iStart, strFind.Length, !bMatchCase) == 0)
                        {
                            textBox1.SelectionStart = iStart;
                            textBox1.SelectionLength = strFind.Length;
                            textBox1.Focus();
                            return true;
                        }
                        iStart++;
                    }
                }
            }
            else
            {
                if (bMatchRegex || (bMatchWholeWord.HasValue && bMatchWholeWord.Value))
                {
                    int iEnd = textBox1.SelectionStart;
                    try
                    {
                        //Regex regex = new Regex((bMatchCase ? string.Empty : "(?i)") + strFind, RegexOptions.Multiline);
                        //MatchCollection mc = regex.Matches(searchData.Substring(0, iEnd));
                        //if (mc.Count > 0)
                        //{
                        //    Match m = mc[mc.Count - 1]; // last match
                        //    textBox1.SelectionStart = m.Index;
                        //    textBox1.SelectionLength = m.Length;
                        //    return true;
                        //}
                        if (bMatchWholeWord.HasValue && bMatchWholeWord.Value)
                        {
                            strFind = "\\b" + strFind + "\\b";
                        }
                        Regex regex = new Regex((bMatchCase ? string.Empty : "(?i)") + string.Format("{0}(?!.*{0})", strFind), RegexOptions.Multiline | RegexOptions.Singleline);
                        Match m = regex.Match(searchData, 0, iEnd);
                        if (m.Success)
                        {
                            textBox1.SelectionStart = m.Index;
                            textBox1.SelectionLength = m.Length;
                            textBox1.Focus();
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        //logger.Error(e);
                        MessageBox.Show(e.Message, Properties.Resources.Regex_Error);
                        return false;
                    }
                }
                else
                {
                    int iStart = textBox1.SelectionStart - strFind.Length;

                    while (iStart >= 0)
                    {
                        if (String.Compare(strFind, 0, searchData, iStart, strFind.Length, !bMatchCase) == 0)
                        {
                            textBox1.SelectionStart = iStart;
                            textBox1.SelectionLength = strFind.Length;
                            textBox1.Focus();
                            return true;
                        }
                        iStart--;
                    }
                }
            }

            MessageBoxResult result = MessageBox.Show(Properties.Resources.Cannot_find_ + "\"" + textFind + "\".\n" +
                Properties.Resources.Continue_search_from_ + (bSearchDown ? Properties.Resources.beginning : Properties.Resources.end) + "?",
                strProgName, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

            if (result == MessageBoxResult.Yes)
            {
                if (bSearchDown)
                {
                    textBox1.SelectionStart = 0;
                }
                else
                {
                    textBox1.SelectionStart = textBox1.Text.Length;
                }

                textBox1.SelectionLength = 0;
                FindNext();
            }
            return false;
        }

        void ReplaceDialogOnReplace(object obj, RoutedEventArgs ea)
        {
            FindReplaceDialog dlg = (FindReplaceDialog)obj;
            textFind = dlg.FindText;
            //itemsFind = dlg.FindCollection; // memorize find strings
            strFind = textFind;

            textReplace = dlg.ReplaceText;
            //itemsReplace = dlg.ReplaceCollection; // memorize replace strings
            strReplace = textReplace;

            bMatchCase = dlg.MatchCase;
            bMatchWholeWord = dlg.MatchWholeWord;
            bMatchRegex = dlg.MatchRegex;
            bMatchDiacritics = dlg.MatchDiacritics;
            bSearchDown = dlg.SearchDown;

            selectedText = textBox1.SelectedText;

            if (selectedText == string.Empty)
            {
                FindNext();
                return;
            }

            if (!bMatchDiacritics)
            {
                strFind = VietUtilities.StripDiacritics(strFind);
                selectedText = VietUtilities.StripDiacritics(selectedText);
            }

            int start = textBox1.SelectionStart;
            if (bMatchRegex)
            {
                try
                {
                    Regex regex = new Regex((bMatchCase ? string.Empty : "(?i)") + strFind, RegexOptions.Multiline);
                    textBox1.SelectedText = regex.Replace(selectedText, Unescape(strReplace));
                }
                catch (Exception e)
                {
                    //logger.Error(e);
                    MessageBox.Show(e.Message, Properties.Resources.Regex_Error);
                    return;
                }
            }
            else if (String.Compare(strFind, selectedText, !bMatchCase) == 0)
            {
                textBox1.SelectedText = strReplace;
            }

            if (!bSearchDown)
            {
                textBox1.SelectionStart = start;
            }

            FindNext();
        }

        void ReplaceDialogOnReplaceAll(object obj, RoutedEventArgs ea)
        {
            FindReplaceDialog dlg = (FindReplaceDialog)obj;

            string str = textBox1.Text;
            string strTemp;

            textFind = dlg.FindText;
            //itemsFind = dlg.FindCollection; // memorize find strings
            strFind = textFind;

            textReplace = dlg.ReplaceText;
            //itemsReplace = dlg.ReplaceCollection; // memorize replace strings
            strReplace = textReplace;

            bMatchDiacritics = dlg.MatchDiacritics;
            if (!bMatchDiacritics)
            {
                strFind = VietUtilities.StripDiacritics(strFind);
                strTemp = VietUtilities.StripDiacritics(str);
            }
            else
            {
                strTemp = str;
            }

            bMatchCase = dlg.MatchCase;
            bMatchRegex = dlg.MatchRegex;
            int count = 0;

            if (bMatchRegex || bMatchDiacritics)
            {
                // only for MatchDiacritics
                try
                {
                    Regex regex = new Regex((bMatchCase ? string.Empty : "(?i)") + (bMatchRegex ? strFind : Regex.Escape(strFind)), RegexOptions.Multiline);
                    MatchCollection mc = regex.Matches(str);
                    count = mc.Count;
                    str = regex.Replace(str, Unescape(strReplace));
                }
                catch (Exception e)
                {
                    //logger.Error(e);
                    MessageBox.Show(e.Message, Properties.Resources.Regex_Error);
                    return;
                }
            }
            //else if (bMatchCase && bMatchDiacritics)
            //{
            //    count = (str.Length - str.Replace(strFind, "").Length) / strFind.Length;
            //    str = str.Replace(strFind, strReplace);
            //}
            else
            {
                StringBuilder strB = new StringBuilder(str);

                for (int i = 0; i <= strB.Length - strFind.Length;)
                {
                    if (String.Compare(strTemp, i, strFind, 0, strFind.Length, !bMatchCase) == 0)
                    {
                        strB.Remove(i, strFind.Length);
                        strB.Insert(i, strReplace);
                        if (!bMatchDiacritics)
                        {
                            strTemp = VietUtilities.StripDiacritics(strB.ToString());
                        }
                        else
                        {
                            strTemp = strB.ToString();
                        }
                        i += strReplace.Length;
                        count++;
                    }
                    else
                    {
                        i++;
                    }
                }
                str = strB.ToString();
            }

            if (str != textBox1.Text)
            {
                textBox1.Text = str;
                textBox1.SelectionStart = 0;
                textBox1.SelectionLength = 0;
                //textBox1.Modified = true;
            }

            MessageBox.Show(string.Format(Properties.Resources.ReplacedOccurrence, count), strProgName);
        }

        private string Unescape(string input)
        {
            return input.Replace(@"\n", "\n").Replace(@"\r", "\r").Replace(@"\t", "\t");
        }

        void FindReplaceDialogOnCloseDlg(object obj, RoutedEventArgs ea)
        {
            this.Focus();
        }

        protected override void LoadRegistryInfo(RegistryKey regkey)
        {
            base.LoadRegistryInfo(regkey);

            bMatchCase = Convert.ToBoolean((int)regkey.GetValue(strMatchCase, Convert.ToInt32(false)));
            bMatchWholeWord = Convert.ToBoolean((int)regkey.GetValue(strMatchWholeWord, Convert.ToInt32(false)));
            bMatchDiacritics = Convert.ToBoolean((int)regkey.GetValue(strMatchDiacritics, Convert.ToInt32(false)));
            bMatchRegex = Convert.ToBoolean((int)regkey.GetValue(strMatchRegex, Convert.ToInt32(false)));
        }

        protected override void SaveRegistryInfo(RegistryKey regkey)
        {
            base.SaveRegistryInfo(regkey);

            regkey.SetValue(strMatchCase, Convert.ToInt32(bMatchCase));
            if (bMatchWholeWord.HasValue) regkey.SetValue(strMatchWholeWord, Convert.ToInt32(bMatchWholeWord.Value));
            regkey.SetValue(strMatchDiacritics, Convert.ToInt32(bMatchDiacritics));
            regkey.SetValue(strMatchRegex, Convert.ToInt32(bMatchRegex));
        }
    }
}
