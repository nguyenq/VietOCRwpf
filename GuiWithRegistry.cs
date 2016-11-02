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
using System.Windows;

namespace VietOCR
{
    public class GuiWithRegistry : Gui
    {
        const string strWinState = "WindowState";
        const string strLocationX = "LocationX";
        const string strLocationY = "LocationY";
        const string strWidth = "Width";
        const string strHeight = "Height";

        protected string strRegKey = "Software\\VietUnicode\\";

        Rect restoreBounds;

        public GuiWithRegistry()
        {
            strRegKey += strProgName;
            InitializeComponent();
            restoreBounds = this.RestoreBounds;
        }

        protected override void Window_Loaded(object sender, RoutedEventArgs e)
        {
            base.Window_Loaded(sender, e);

            // Load registry information.
            RegistryKey regkey = Registry.CurrentUser.OpenSubKey(strRegKey);

            if (regkey == null)
                regkey = Registry.CurrentUser.CreateSubKey(strRegKey);

            LoadRegistryInfo(regkey);
            regkey.Close();
        }

        protected override void Window_Closed(object sender, EventArgs e)
        {
            // Save registry information.
            RegistryKey regkey = Registry.CurrentUser.OpenSubKey(strRegKey, true);

            if (regkey == null)
                regkey = Registry.CurrentUser.CreateSubKey(strRegKey);

            SaveRegistryInfo(regkey);
            regkey.Close();

            base.Window_Closed(sender, e);
        }

        protected override void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                restoreBounds = this.RestoreBounds;
        }

        protected override void Window_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Normal)
                restoreBounds = this.RestoreBounds;
        }

        protected override void SaveRegistryInfo(RegistryKey regkey)
        {
            base.SaveRegistryInfo(regkey);

            regkey.SetValue(strWinState, (int) WindowState);
            regkey.SetValue(strLocationX, restoreBounds.X);
            regkey.SetValue(strLocationY, restoreBounds.Y);
            regkey.SetValue(strWidth, restoreBounds.Width);
            regkey.SetValue(strHeight, restoreBounds.Height);
        }

        protected override void LoadRegistryInfo(RegistryKey regkey)
        {
            base.LoadRegistryInfo(regkey);

            double x = Convert.ToDouble(regkey.GetValue(strLocationX, 100));
            double y = Convert.ToDouble(regkey.GetValue(strLocationY, 100));
            double cx = Convert.ToDouble(regkey.GetValue(strWidth, 324));
            double cy = Convert.ToDouble(regkey.GetValue(strHeight, 300));

            restoreBounds = new Rect(x, y, cx, cy);

            // Adjust rectangle for any change in desktop size.

            Rect rectDesk = SystemParameters.WorkArea;

            restoreBounds.Width = Math.Min(restoreBounds.Width, rectDesk.Width);
            restoreBounds.Height = Math.Min(restoreBounds.Height, rectDesk.Height);
            restoreBounds.X -= Math.Max(restoreBounds.Right - rectDesk.Right, 0);
            restoreBounds.Y -= Math.Max(restoreBounds.Bottom - rectDesk.Bottom, 0);

            // Set form properties.
            this.Left = restoreBounds.Left;
            this.Top = restoreBounds.Top;
            this.Width = restoreBounds.Width;
            this.Height = restoreBounds.Height;

            WindowState = (WindowState)regkey.GetValue(strWinState, 0);
        }
    }
}

