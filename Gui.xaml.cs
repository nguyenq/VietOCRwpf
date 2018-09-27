
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

using Tesseract;
using Net.SourceForge.Vietpad.InputMethod;
using Net.SourceForge.Vietpad.Utilities;
using VietOCR.NET.Utilities;

namespace VietOCR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Gui : Window
    {
        public const string strProgName = "VietOCR.NET";
        public const string TO_BE_IMPLEMENTED = "To be implemented";

        protected bool textModified;
        protected string textFilename;
        protected System.CodeDom.Compiler.TempFileCollection tempFileCollection = new System.CodeDom.Compiler.TempFileCollection();
        protected readonly string baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        protected int imageIndex;
        protected int imageTotal;

        protected float scaleX = 1f;
        protected float scaleY = 1f;

        private const int FONT_MIN_SIZE = 6;
        private const int FONT_MAX_SIZE = 50;

        const string strUILang = "UILanguage";
        const string strOcrLanguage = "OcrLanguage";
        const string strWordWrap = "WordWrap";
        const string strFontFace = "FontFace";
        const string strFontSize = "FontSize";
        const string strFontStyle = "FontStyle";
        const string strForeColor = "ForeColor";
        const string strBackColor = "BackColor";
        const string strFilterIndex = "FilterIndex";

        const string strSegmentedRegions = "SegmentedRegions";
        const string strSegmentedRegionsPara = "SegmentedRegionsPara";
        const string strSegmentedRegionsTextLine = "SegmentedRegionsTextLine";
        const string strSegmentedRegionsSymbol = "SegmentedRegionsSymbol";
        const string strSegmentedRegionsBlock = "SegmentedRegionsBlock";
        const string strSegmentedRegionsWord = "SegmentedRegionsWord";

        protected bool isFitImageSelected;
        protected string selectedUILanguage;
        private int filterIndex;
        protected string curLangCode = "vie";

        protected DataSource dataSource;

        private Dictionary<string, string> installedLanguageCodes;

        private Dictionary<string, string> lookupISO639;

        public Dictionary<string, string> LookupISO639
        {
            get { return lookupISO639; }
        }

        private Dictionary<string, string> lookupISO_3_1_Codes;

        public Dictionary<string, string> LookupISO_3_1_Codes
        {
            get { return lookupISO_3_1_Codes; }
        }

        public static readonly RoutedCommand Command = new RoutedCommand();

        public Gui()
        {
            InitializeComponent();

            dataSource = new DataSource();
            dataSource.InstalledLanguages = GetInstalledLanguagePacks();
            dataSource.PropertyChanged += DataSource_PropertyChanged;
            this.DataContext = dataSource;
        }

        /// <summary>
        /// Gets Tesseract's installed language data packs.
        /// </summary>
        ObservableCollection<string> GetInstalledLanguagePacks()
        {
            lookupISO639 = new Dictionary<string, string>();
            lookupISO_3_1_Codes = new Dictionary<string, string>();
            installedLanguageCodes = new Dictionary<string, string>();
            ObservableCollection<string> installedLanguages = new ObservableCollection<string>();

            try
            {
                string tessdataDir = Path.Combine(baseDir, "tessdata");
                string[] installedLanguagePacks = Directory.GetFiles(tessdataDir, "*.traineddata");
                installedLanguagePacks = installedLanguagePacks.Where(x => !x.EndsWith("osd.traineddata")).ToArray();

                string xmlFilePath = Path.Combine(baseDir, "Data/ISO639-3.xml");
                VietOCR.NET.Utilities.Utilities.LoadFromXML(lookupISO639, xmlFilePath);
                xmlFilePath = Path.Combine(baseDir, "Data/ISO639-1.xml");
                VietOCR.NET.Utilities.Utilities.LoadFromXML(lookupISO_3_1_Codes, xmlFilePath);

                if (installedLanguagePacks != null)
                {
                    foreach (string langPack in installedLanguagePacks)
                    {
                        string langCode = Path.GetFileNameWithoutExtension(langPack);
                        // translate ISO codes to full English names for user-friendliness
                        if (lookupISO639.ContainsKey(langCode))
                        {
                            installedLanguageCodes.Add(langCode, lookupISO639[langCode]);
                        }
                        else
                        {
                            installedLanguageCodes.Add(langCode, langCode);
                        }
                    }

                    List<string> lst = installedLanguageCodes.Values.ToList();
                    lst.Sort();
                    installedLanguages = new ObservableCollection<string>(lst);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(this, e.Message, strProgName);
                // this also applies to missing language data files in tessdata directory
                Console.WriteLine(e.StackTrace);
            }

            return installedLanguages;
        }

        protected virtual void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !OkToTrash();
        }

        protected virtual void Window_Closed(object sender, EventArgs e)
        {

        }

        protected virtual void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        protected virtual void Window_LocationChanged(object sender, EventArgs e)
        {
        }

        protected virtual void buttonSpellcheck_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void buttonPostProcess_Click(object sender, RoutedEventArgs e)
        {
        }

        protected virtual void postprocessToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
        }

        protected virtual void scanToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
        }

        protected void saveToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
        }

        protected void saveAsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
        }

        protected virtual void oCRToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void oCRAllPagesToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void bulkOCRToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void metadataToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void deskewToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void autocropToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void cropToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void removeLinesToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void despeckle2x2ToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void despeckle3x3ToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void undoToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void screenshotModeToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.screenshotModeToolStripMenuItem.IsChecked ^= true;
            this.statusLabelSMvalue.Content = this.screenshotModeToolStripMenuItem.IsChecked ? "On" : "Off";
        }

        protected virtual void segmentedRegionsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.segmentedRegionsToolStripMenuItem.IsChecked ^= true;
            this.buttonSegmentedRegions.Visibility = this.segmentedRegionsToolStripMenuItem.IsChecked ? Visibility.Visible : Visibility.Hidden;
            setSegmentedRegions();
        }

        protected virtual void wordWrapToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void fontToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void changeCaseToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void removeLineBreaksToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void downloadLangDataToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void optionsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void mergeTiffToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void splitTiffToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void mergePdfToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void splitPdfToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void convertPdfToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void helpToolStripMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            HtmlHelpDialog dlg = new HtmlHelpDialog(Properties.Resources.readme, "VietOCR");
            dlg.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string releaseDate = System.Configuration.ConfigurationManager.AppSettings["ReleaseDate"];
            string version = System.Configuration.ConfigurationManager.AppSettings["Version"];
            string tessVersion = System.Configuration.ConfigurationManager.AppSettings["TessVersion"];

            MessageBox.Show(string.Format("VietOCR.NET WPF {0}\n", version) +
                string.Format(Properties.Resources.Program_desc, tessVersion) + "\n" +
                DateTime.Parse(releaseDate).ToString("D", System.Threading.Thread.CurrentThread.CurrentUICulture).Normalize() + "\n" +
                "http://vietocr.sourceforge.net", "About VietOCR.NET", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected virtual void buttonOCR_Click(object sender, RoutedEventArgs e)
        {
            this.oCRToolStripMenuItem_Click(sender, e);
        }

        protected string inputfilename;
        protected IList<System.Drawing.Image> imageList;

        protected virtual void buttonCancelOCR_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            this.textBox1.Clear();
        }

        private void buttonRemoveLineBreaks_Click(object sender, RoutedEventArgs e)
        {
            this.removeLineBreaksToolStripMenuItem_Click(sender, e);
        }

        private void buttonScan_Click(object sender, RoutedEventArgs e)
        {
            this.scanToolStripMenuItem_Click(sender, e);
        }

        protected virtual void loadThumbnails()
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }

        protected virtual void brightenToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }

        protected virtual void contrastToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }

        protected virtual void grayscaleToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }

        protected virtual void monochromeToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }

        protected virtual void invertToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }

        protected virtual void sharpenToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }

        protected virtual void smoothToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }

        protected virtual void bilateralToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }

        protected virtual void gammaToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }

        protected virtual void thresholdToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }

        /// <summary>
        /// Changes localized text and messages
        /// </summary>
        /// <param name="locale"></param>
        /// <param name="firstTime"></param>
        protected virtual void ChangeUILanguage(string locale)
        {
            //string imageProperties = this.toolStripStatusLabelDimValue.Text; // retain values of image properties in statusbar
            //string totalPage = this.toolStripLabelPageNum.Text;

            FormLocalizer localizer = new FormLocalizer(this, typeof(Gui));
            localizer.ApplyCulture(new CultureInfo(locale));

            this.statusLabel.Content = null;

            foreach (Window dlg in this.OwnedWindows)
            {
                HtmlHelpDialog helpForm = dlg as HtmlHelpDialog;
                if (helpForm != null)
                {
                    helpForm.Title = Properties.Resources.VietOCR_Help;
                }
            }

            //this.contextMenuStrip1.ChangeUILanguage();

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Gui));
            //this.toolTip1.SetToolTip(this.buttonCollapseExpand, resources.GetString("buttonCollapseExpand.ToolTipText"));
            //this.toolStripStatusLabelDimValue.Text = imageProperties; // restore prior values
            //this.toolStripLabelPageNum.Text = totalPage;
        }

        protected virtual void toolStripBtnPrev_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }
        protected virtual void toolStripBtnNext_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }
        protected virtual void toolStripBtnFitImage_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }
        protected virtual void toolStripBtnActualSize_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }
        protected virtual void toolStripBtnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }
        protected virtual void toolStripBtnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }
        protected virtual void toolStripBtnRotateCCW_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }
        protected virtual void toolStripBtnRotateCW_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }
        protected virtual void splitContainerImage_SplitterMoved(object sender, EventArgs e)
        {
            MessageBox.Show(TO_BE_IMPLEMENTED, strProgName);
        }

        void PasteImage()
        {
            BitmapSource image = ImageHelper.GetClipboardImage();
            if (image != null)
            {
                string tempFileName = Path.GetTempFileName();
                File.Delete(tempFileName);
                tempFileName = Path.ChangeExtension(tempFileName, ".png");
                tempFileCollection.AddFile(tempFileName, false);
                using (var fileStream = new FileStream(tempFileName, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(fileStream);
                }
                openFile(tempFileName);
            }
        }

        protected virtual void openFile(string fileName)
        {

        }

        protected virtual bool saveAction()
        {
            return true;
        }

        protected virtual bool saveAsAction()
        {
            return true;
        }

        protected bool OkToTrash()
        {
            if (!textModified)
            {
                return true;
            }

            MessageBoxResult dr =
                MessageBox.Show(Properties.Resources.Do_you_want_to_save_the_changes_to_ + FileTitle() + "?",
                strProgName,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Exclamation);
            switch (dr)
            {
                case MessageBoxResult.Yes:
                    return saveAction();
                case MessageBoxResult.No:
                    return true;
                case MessageBoxResult.Cancel:
                    return false;
            }
            return false;
        }

        protected string FileTitle()
        {
            return (this.textFilename != null && this.textFilename.Length > 1) ?
                Path.GetFileName(this.textFilename) : Properties.Resources.Untitled;
        }

        protected void centerPicturebox()
        {
            //this.splitContainerImage.Panel2.AutoScrollPosition = Point.Empty;
            //int x = 0;
            //int y = 0;

            //if (this.imageMain.Width < this.splitContainerImage.Panel2.Width)
            //{
            //    x = (this.splitContainerImage.Panel2.Width - this.imageMain.Width) / 2;
            //}

            //if (this.imageMain.Height < this.splitContainerImage.Panel2.Height)
            //{
            //    y = (this.splitContainerImage.Panel2.Height - this.imageMain.Height) / 2;
            //}

            //this.imageMain.Location = new Point(x, y);
            //this.imageMain.Invalidate();
        }

        protected System.Drawing.Size fitImagetoContainer(int w, int h, int maxWidth, int maxHeight)
        {
            float ratio = (float)w / h;

            w = maxWidth;
            h = (int)Math.Floor(maxWidth / ratio);

            if (h > maxHeight)
            {
                h = maxHeight;
                w = (int)Math.Floor(maxHeight * ratio);
            }

            return new System.Drawing.Size(w, h);
        }

        protected virtual void clearStack()
        {
            // to be implemented in subclass
        }

        protected virtual void LoadRegistryInfo(RegistryKey regkey)
        {
            string selectedLanguagesText = (string)regkey.GetValue(strOcrLanguage, String.Empty);
            dataSource.SelectedLanguages = new ObservableCollection<string>(selectedLanguagesText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).Distinct().ToList());
            curLangCode = GetLangCodes(selectedLanguagesText);

            this.textBox1.TextWrapping = (TextWrapping)regkey.GetValue(strWordWrap, TextWrapping.NoWrap);
            this.textBox1.FontFamily = new System.Windows.Media.FontFamily(regkey.GetValue(strFontFace, "Microsoft Sans Serif").ToString());
            this.textBox1.FontSize = double.Parse((string)regkey.GetValue(strFontSize, "10"));
            var fsc = new FontStyleConverter();
            this.textBox1.FontStyle = (FontStyle)fsc.ConvertFromString((string)regkey.GetValue(strFontStyle, "Normal"));
            var bc = new BrushConverter();
            this.textBox1.Foreground = (Brush)bc.ConvertFromString((string)regkey.GetValue(strForeColor, "Black"));
            this.textBox1.Background = (Brush)bc.ConvertFromString((string)regkey.GetValue(strBackColor, "White"));
            filterIndex = (int)regkey.GetValue(strFilterIndex, 1);
            selectedUILanguage = Thread.CurrentThread.CurrentUICulture.Name;

            this.segmentedRegionsToolStripMenuItem.IsChecked = Convert.ToBoolean((int)regkey.GetValue(strSegmentedRegions, Convert.ToInt32(false)));
            this.buttonSegmentedRegions.Visibility = this.segmentedRegionsToolStripMenuItem.IsChecked ? Visibility.Visible : Visibility.Hidden;
            this.toolStripMenuItemPara.IsChecked = Convert.ToBoolean((int)regkey.GetValue(strSegmentedRegionsPara, Convert.ToInt32(false)));
            this.toolStripMenuItemTextLine.IsChecked = Convert.ToBoolean((int)regkey.GetValue(strSegmentedRegionsTextLine, Convert.ToInt32(false)));
            this.toolStripMenuItemSymbol.IsChecked = Convert.ToBoolean((int)regkey.GetValue(strSegmentedRegionsSymbol, Convert.ToInt32(false)));
            this.toolStripMenuItemBlock.IsChecked = Convert.ToBoolean((int)regkey.GetValue(strSegmentedRegionsBlock, Convert.ToInt32(false)));
            this.toolStripMenuItemWord.IsChecked = Convert.ToBoolean((int)regkey.GetValue(strSegmentedRegionsWord, Convert.ToInt32(false)));
        }

        protected virtual void SaveRegistryInfo(RegistryKey regkey)
        {
            regkey.SetValue(strOcrLanguage, dataSource.SelectedLanguagesText);

            regkey.SetValue(strWordWrap, Convert.ToInt32(this.textBox1.TextWrapping));
            regkey.SetValue(strFontFace, this.textBox1.FontFamily);
            regkey.SetValue(strFontSize, this.textBox1.FontSize);
            regkey.SetValue(strFontStyle, this.textBox1.FontStyle.ToString());
            var bc = new BrushConverter();
            regkey.SetValue(strForeColor, bc.ConvertToString(this.textBox1.Foreground));
            regkey.SetValue(strBackColor, bc.ConvertToString(this.textBox1.Background));
            regkey.SetValue(strFilterIndex, filterIndex);
            regkey.SetValue(strUILang, Thread.CurrentThread.CurrentUICulture.Name);
            regkey.SetValue(strSegmentedRegions, Convert.ToInt32(this.segmentedRegionsToolStripMenuItem.IsChecked));
            regkey.SetValue(strSegmentedRegionsPara, Convert.ToInt32(this.toolStripMenuItemPara.IsChecked));
            regkey.SetValue(strSegmentedRegionsTextLine, Convert.ToInt32(this.toolStripMenuItemTextLine.IsChecked));
            regkey.SetValue(strSegmentedRegionsSymbol, Convert.ToInt32(this.toolStripMenuItemSymbol.IsChecked));
            regkey.SetValue(strSegmentedRegionsBlock, Convert.ToInt32(this.toolStripMenuItemBlock.IsChecked));
            regkey.SetValue(strSegmentedRegionsWord, Convert.ToInt32(this.toolStripMenuItemWord.IsChecked));
        }

        bool srClicked;

        private void buttonSegmentedRegions_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            btn.ContextMenu.PlacementTarget = btn;
            btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            btn.ContextMenu.IsOpen = !srClicked;
            srClicked ^= true;
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            srClicked = false;
            Keyboard.ClearFocus();
        }

        private void srMenuItem_Click(object sender, RoutedEventArgs e)
        {
            srClicked = false;
            Keyboard.ClearFocus();
            setSegmentedRegions();
        }

        protected void setSegmentedRegions()
        {
            if (!this.segmentedRegionsToolStripMenuItem.IsChecked || imageList == null || this.buttonActualSize.IsEnabled)
            {
                this.imageCanvas.SegmentedRegions = null;
                return;
            }

            OCR<System.Drawing.Image> ocrEngine = new OCRImages();
            Dictionary<System.Windows.Media.SolidColorBrush, List<System.Drawing.Rectangle>> map = this.imageCanvas.SegmentedRegions;
            if (map == null)
            {
                map = new Dictionary<System.Windows.Media.SolidColorBrush, List<System.Drawing.Rectangle>>();
            }

            System.Drawing.Bitmap image = (System.Drawing.Bitmap)imageList[imageIndex];

            List<System.Drawing.Rectangle> regions;

            if (toolStripMenuItemBlock.IsChecked)
            {
                if (!map.ContainsKey(System.Windows.Media.Brushes.Gray))
                {
                    regions = ocrEngine.GetSegmentedRegions(image, PageIteratorLevel.Block);
                    map.Add(System.Windows.Media.Brushes.Gray, regions);
                }
            }
            else
            {
                map.Remove(System.Windows.Media.Brushes.Gray);
            }

            if (toolStripMenuItemPara.IsChecked)
            {
                if (!map.ContainsKey(System.Windows.Media.Brushes.Green))
                {
                    regions = ocrEngine.GetSegmentedRegions(image, PageIteratorLevel.Para);
                    map.Add(System.Windows.Media.Brushes.Green, regions);
                }
            }
            else
            {
                map.Remove(System.Windows.Media.Brushes.Green);
            }

            if (toolStripMenuItemTextLine.IsChecked)
            {
                if (!map.ContainsKey(System.Windows.Media.Brushes.Red))
                {
                    regions = ocrEngine.GetSegmentedRegions(image, PageIteratorLevel.TextLine);
                    map.Add(System.Windows.Media.Brushes.Red, regions);
                }
            }
            else
            {
                map.Remove(System.Windows.Media.Brushes.Red);
            }

            if (toolStripMenuItemWord.IsChecked)
            {
                if (!map.ContainsKey(System.Windows.Media.Brushes.Blue))
                {
                    regions = ocrEngine.GetSegmentedRegions(image, PageIteratorLevel.Word);
                    map.Add(System.Windows.Media.Brushes.Blue, regions);
                }
            }
            else
            {
                map.Remove(System.Windows.Media.Brushes.Blue);
            }

            if (toolStripMenuItemSymbol.IsChecked)
            {
                if (!map.ContainsKey(System.Windows.Media.Brushes.Magenta))
                {
                    regions = ocrEngine.GetSegmentedRegions(image, PageIteratorLevel.Symbol);
                    map.Add(System.Windows.Media.Brushes.Magenta, regions);
                }
            }
            else
            {
                map.Remove(System.Windows.Media.Brushes.Magenta);
            }

            this.imageCanvas.SegmentedRegions = map;
        }

        protected virtual void buttonRotateCCW_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void buttonRotateCW_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void buttonFitImage_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void buttonActualSize_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void buttonZoomIn_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void buttonZoomOut_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void buttonPrev_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void buttonNext_Click(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void comboBoxPageNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            bool dropEnabled = true;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop);

                string allowedExt = @"\.(bmp|gif|jpg|jpeg|png|tif|tiff|pdf|txt)$";
                if (!Regex.Match(System.IO.Path.GetExtension(filenames[0]), allowedExt, RegexOptions.IgnoreCase).Success)
                {
                    dropEnabled = false;
                }
            }
            else
            {
                dropEnabled = false;
            }

            if (!dropEnabled)
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
                openFile(filenames[0]);
            }
        }

        protected virtual void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        protected virtual void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            saveAction();
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!IsLoaded) return;
            e.CanExecute = textBox1.Text.Length > 0;
        }

        protected virtual void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            saveAsAction();
        }

        private void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!IsLoaded) return;
            e.CanExecute = textBox1.Text.Length > 0;
        }

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PasteImage();
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsImage();
        }

        protected virtual void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        protected virtual void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = true;
        }

        private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (OkToTrash())
            {
                this.Close();
            }
        }

        private void Exit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter != null)
            {
                // Parameter has MenuItem's name
                MenuItem mi = this.FindName(e.Parameter.ToString()) as MenuItem;
                if (mi != null)
                {
                    mi.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent)); // simulate click
                }
            }
        }

        private void OnSubmenuOpened(object sender, RoutedEventArgs e)
        {
            this.wordWrapToolStripMenuItem.IsChecked = this.textBox1.TextWrapping == TextWrapping.Wrap;
        }

        private void textBox1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                double newSize = this.textBox1.FontSize + e.Delta / 120;
                if (newSize > FONT_MIN_SIZE && newSize < FONT_MAX_SIZE)
                {
                    this.textBox1.FontSize = newSize;
                }
            }
        }

        private void scrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Delta <= 0)
                {
                    // set minimum size to zoom
                    if (this.imageCanvas.Width < 100)
                        return;
                }
                else
                {
                    // set maximum size to zoom
                    if (this.imageCanvas.Width > 10000)
                        return;
                }

                this.imageCanvas.Deselect();
                this.imageCanvas.Width += this.imageCanvas.Width * e.Delta / 1000;
                this.imageCanvas.Height += this.imageCanvas.Height * e.Delta / 1000;
                System.Drawing.Bitmap currentImage = (System.Drawing.Bitmap)imageList[imageIndex];
                scaleX = (float)currentImage.Width / (float)this.imageCanvas.Width;
                scaleY = (float)currentImage.Height / (float)this.imageCanvas.Height;
                this.centerPicturebox();
                isFitImageSelected = false;
                this.buttonFitImage.IsEnabled = true;
                this.buttonActualSize.IsEnabled = true;
                this.buttonZoomIn.IsEnabled = true;
                this.buttonZoomOut.IsEnabled = true;

                e.Handled = true;
            }
        }

        protected virtual void textBox1_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        /// <summary>
        /// OCR language selection change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataSource_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedLanguagesText")
            {
                curLangCode = GetLangCodes(dataSource.SelectedLanguagesText);

                // Hide Viet Input Method submenu if selected OCR Language is not Vietnamese
                bool vie = curLangCode.Contains("vie") || curLangCode.Contains("Vietnamese");
                VietKeyHandler.VietModeEnabled = vie;
                this.vietInputMethodToolStripMenuItem.Visibility = vie ? Visibility.Visible : Visibility.Collapsed;

                // Spellcheck
                if (this.buttonSpellcheck.IsChecked.Value)
                {
                    ToggleButtonAutomationPeer peer = new ToggleButtonAutomationPeer(buttonSpellcheck);
                    System.Windows.Automation.Provider.IToggleProvider toggleProvider = peer.GetPattern(PatternInterface.Toggle) as System.Windows.Automation.Provider.IToggleProvider;
                    toggleProvider.Toggle(); // disable spellcheck
                    buttonSpellcheck.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    toggleProvider.Toggle(); // re-enable spellcheck
                    buttonSpellcheck.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
        }

        /// <summary>
        /// Gets language codes based on language names.
        /// </summary>
        /// <param name="languages"></param>
        /// <returns></returns>
        string GetLangCodes(string languages)
        {
            char[] delimiterChars = { ',', '+' };
            string langCode = string.Empty;

            foreach (var lang in languages.Split(delimiterChars))
            {
                if (string.IsNullOrWhiteSpace(lang))
                {
                    continue;
                }
                langCode += installedLanguageCodes.FirstOrDefault(x => x.Value == lang).Key + "+";
            }

            return langCode.TrimEnd('+');
        }

        protected virtual void buttonFind_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
