﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace VietOCR
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int pid);
     
        private string strRegKey = "Software\\VietUnicode\\";
        const string strUILang = "UILanguage";
        public const string strProgName = "VietOCR.NET";

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length == 0)
            {
                base.OnStartup(e);

                // Access registry to determine which UI Language to be loaded.
                // The desired locale must be known before initializing visual components
                // with language text. Waiting until OnLoad would be too late.
                strRegKey += strProgName;

                RegistryKey regkey = Registry.CurrentUser.OpenSubKey(strRegKey);

                if (regkey == null)
                    regkey = Registry.CurrentUser.CreateSubKey(strRegKey);

                string selectedUILanguage = (string)regkey.GetValue(strUILang, "en-US");
                regkey.Close();

                //XmlLanguage lang = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
                //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(lang));
                //FrameworkContentElement.LanguageProperty.OverrideMetadata(typeof(System.Windows.Documents.TextElement), new FrameworkPropertyMetadata(lang));

                // Sets the UI culture to the selected language.
                ChangeCulture(new CultureInfo(selectedUILanguage));
            }
            else
            {
                // Command line given, display console
                if (!AttachConsole(-1)) // Attach to a parent process console
                {
                    AllocConsole(); // Alloc a new console
                }

                ConsoleApp.Main(e.Args);
                Environment.Exit(0);
            }
        }

        public static void ChangeCulture(CultureInfo newCulture)
        {
            Thread.CurrentThread.CurrentUICulture = newCulture;

            var oldWindow = Application.Current.MainWindow;

            Application.Current.MainWindow = new GuiWithTools();
            Application.Current.MainWindow.Show();

            if (oldWindow != null)
            {
                oldWindow.Close();
            }
        }
    }
}
