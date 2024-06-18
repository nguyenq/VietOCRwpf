using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Collections;
using Net.SourceForge.Vietpad.Utilities;
using System.Windows;
using System.Windows.Controls;
using NHunspell;
using VietOCR.Controls;
using System.Windows.Documents;
using System.Collections.ObjectModel;

namespace VietOCR
{
    public class SpellCheckHelper
    {
        TextBox textbox;
        string localeId;
        static ObservableCollection<CharacterRange> spellingErrorRanges = new ObservableCollection<CharacterRange>();
        static ObservableCollection<string> userWordList = new ObservableCollection<string>();
        static DateTime mapLastModified = DateTime.MinValue;
        Hunspell spellChecker;

        static AdornerLayer myAdornerLayer;
        static RedUnderlineAdorner myAdorner;

        static string baseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string strUserDictFile;

        public ObservableCollection<CharacterRange> GetSpellingErrorRanges
        {
            get { return spellingErrorRanges; }
        }

        public SpellCheckHelper(TextBox textbox, string localeId)
        {
            this.textbox = textbox;
            this.localeId = localeId;
            string userAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appdataDir = Path.Combine(userAppData, "VietOCR");
            if (!Directory.Exists(appdataDir))
            {
                Directory.CreateDirectory(appdataDir);
            }

            strUserDictFile = Path.Combine(appdataDir, "user.dic");
            if (!File.Exists(strUserDictFile))
            {
                File.CreateText(strUserDictFile).Close();
            }
        }

        public bool InitializeSpellCheck()
        {
            if (localeId == null)
            {
                return false;
            }
            try
            {
                string dictPath = baseDir + "/dict/" + localeId;
                spellChecker = new Hunspell(dictPath + ".aff", dictPath + ".dic");
                if (!LoadUserDictionary())
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void EnableSpellCheck()
        {
            if (!InitializeSpellCheck())
            {
                throw new Exception("Spellcheck initialization error!");
            }

            myAdornerLayer = AdornerLayer.GetAdornerLayer(this.textbox);
            myAdorner = new RedUnderlineAdorner(this.textbox, this);
            myAdornerLayer.Add(myAdorner);

            SpellCheck();
        }

        public void DisableSpellCheck()
        {
            spellingErrorRanges.Clear();

            if (myAdorner != null)
            {
                myAdornerLayer.Remove(myAdorner);
                myAdorner.Dispose();
            }

            if (spellChecker != null)
            {
                spellChecker.Dispose();
            }
        }

        public void SpellCheck()
        {
            spellingErrorRanges.Clear();
            List<string> words = ParseText(textbox.Text);
            List<string> misspelledWords = SpellCheck(words);
            if (misspelledWords.Count == 0)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (string word in misspelledWords)
            {
                sb.Append(word).Append("|");
            }
            sb.Length -= 1; //remove last |

            // build regex
            string patternStr = "\\b(" + sb.ToString() + ")\\b";
            Regex regex = new Regex(patternStr, RegexOptions.IgnoreCase);
            MatchCollection mc = regex.Matches(textbox.Text);

            // Loop through the match collection to retrieve all 
            // matches and positions.
            for (int i = 0; i < mc.Count; i++)
            {
                spellingErrorRanges.Add(new CharacterRange(mc[i].Index, mc[i].Length));
            }
        }

        public bool IsMispelled(string word)
        {
            if (!spellChecker.Spell(word))
            {
                // is mispelled word in user.dic?
                if (!userWordList.Contains(word.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        List<string> SpellCheck(List<string> words)
        {
            List<string> misspelled = new List<string>();

            foreach (string word in words)
            {
                if (IsMispelled(word))
                {
                    misspelled.Add(word);
                }
            }

            return misspelled;
        }

        List<string> ParseText(string text)
        {
            List<string> words = new List<string>();
            BreakIterator boundary = BreakIterator.GetWordInstance();
            boundary.Text = text;
            int start = boundary.First();
            for (int end = boundary.Next(); end != BreakIterator.DONE; start = end, end = boundary.Next())
            {
                if (!Char.IsLetter(text[start]))
                {
                    continue;
                }
                words.Add(text.Substring(start, end - start));
            }

            return words;
        }

        public List<string> Suggest(string misspelled)
        {
            List<string> list = new List<string>();
            list.Add(misspelled);

            if (SpellCheck(list).Count == 0)
            {
                return null;
            }
            else
            {
                return spellChecker.Suggest(misspelled); // TODO: exception thrown here.
            }
        }

        public void IgnoreWord(string word)
        {
            if (!userWordList.Contains(word.ToLower()))
            {
                userWordList.Add(word.ToLower());
            }
        }

        public void AddWord(string word)
        {
            if (!userWordList.Contains(word.ToLower()))
            {
                userWordList.Add(word.ToLower());
                
                using (StreamWriter sw = new StreamWriter(strUserDictFile, true, Encoding.UTF8))
                {
                    sw.WriteLine(word);
                }
            }
        }

        bool LoadUserDictionary()
        {
            try
            {
                FileInfo userDict = new FileInfo(strUserDictFile);
                DateTime fileLastModified = userDict.LastWriteTime;

                if (fileLastModified <= mapLastModified)
                {
                    return true; // no need to reload dictionary
                }

                mapLastModified = fileLastModified;
                userWordList.Clear();

                using (StreamReader sr = new StreamReader(strUserDictFile, Encoding.UTF8))
                {
                    string str;

                    while ((str = sr.ReadLine()) != null)
                    {
                        userWordList.Add(str.ToLower());
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
