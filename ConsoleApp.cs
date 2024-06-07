using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VietOCR
{
    public class ConsoleApp
    {
        public static void Main(string[] args)
        {
            new ConsoleApp().PerformOCR(args);
        }

        void PerformOCR(string[] args)
        {
            if (args[0] == "-?" || args[0] == "-help" || args.Length == 1)
            {
                Console.WriteLine("Usage: vietocr imagefile outputfile [-l lang] [--psm pagesegmode] [text|hocr|pdf|pdf_textonly|unlv|box|alto|page|tsv|lstmbox|wordstrbox] [postprocessing] [correctlettercases] [deskew] [removelines] [removelinebreaks]");
                return;
            }

            FileInfo imageFile = new FileInfo(args[0]);
            FileInfo outputFile = new FileInfo(args[1]);

            if (!imageFile.Exists)
            {
                Console.WriteLine("Input file does not exist.");
                return;
            }

            ProcessingOptions options = new ProcessingOptions();

            HashSet<string> outputFormatSet = new HashSet<string>();
            string[] renderers = Enum.GetNames(typeof(Tesseract.RenderedFormat));
            string curLangCode = "eng"; //default language
            string psm = "3"; // or alternatively, "Auto"; // 3 - Fully automatic page segmentation, but no OSD (default)
            
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                // command-line options
                if ("-l" == arg)
                {
                    if ((i+1) < args.Length)
                    {
                        curLangCode = args[i + 1];
                    }
                }

                if ("--psm" == arg)
                {
                    if ((i+1) < args.Length)
                    {
                        psm = args[i + 1];
                        try
                        {
                            short psmval = Int16.Parse(psm);
                            if (psmval > 13) throw new ArgumentException();
                        }
                        catch
                        {
                            Console.WriteLine("Invalid input value for PSM.");
                            return;
                        }
                    }
                }

                // parse output formats
                if (renderers.Contains(arg.ToUpper()))
                {
                    outputFormatSet.Add(arg.ToUpper());
                }

                // enable pre-processing
                if ("deskew" == arg)
                {
                    options.Deskew = true;
                }
                if ("removelines" == arg)
                {
                    options.RemoveLines = true;
                }

                // enable post-processing
                if ("postprocessing" == arg)
                {
                    options.PostProcessing = true;
                }
                if ("correctlettercases" == arg)
                {
                    options.CorrectLetterCases = true;
                }
                if ("removelinebreaks" == arg)
                {
                    options.RemoveLineBreaks = true;
                }
            }

            if (outputFormatSet.Count == 0)
            {
                outputFormatSet.Add(Tesseract.RenderedFormat.TEXT.ToString());
            }

            string outputFormat = string.Join(",", outputFormatSet);

            try
            {
                OCRHelper.PerformOCR(imageFile.FullName, outputFile.FullName, curLangCode, psm, outputFormat, options);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }
    }
}
