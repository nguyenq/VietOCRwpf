using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VietOCR
{
    /// <summary>
    /// Interaction logic for HtmlHelpForm.xaml
    /// </summary>
    public partial class HtmlHelpDialog : Window
    {
        const string ABOUT = "about:";

        public HtmlHelpDialog(string helpFileName, string title)
        {
            InitializeComponent();

            this.Title = title;

            // Load HTML document as a stream
            Uri uri = new Uri(@"pack://application:,,,/" + helpFileName, UriKind.Absolute);
            Stream source = Application.GetResourceStream(uri).Stream;

            // Navigate to HTML document stream
            this.webBrowser.NavigateToStream(source);
        }

        private void webBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri == null)
            {
                return;
            }
            string url = e.Uri.ToString();

            if (url.StartsWith(ABOUT) && url != "about:blank")
            {
                this.webBrowser.NavigateToStream(Application.GetResourceStream(e.Uri).Stream);
            }
            else if (url.StartsWith("http"))
            {
                // Display external links using default webbrowser
                e.Cancel = true;
                System.Diagnostics.Process.Start(url);
            }
        }
    }
}
