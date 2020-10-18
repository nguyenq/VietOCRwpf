using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VietOCR
{
    class ConsoleApp
    {
        public static void Main(string[] args)
        {
            new ConsoleApp().PerformOCR(args);
        }

        void PerformOCR(string[] args)
        {
            if (args[0] == "-?" || args[0] == "-help" || args.Length == 1 || args.Length >= 8)
            {
                Console.WriteLine("Usage: vietocr imagefile outputfile [-l lang] [-psm pagesegmode] [text|hocr|pdf|unlv|box|alto|tsv|lstmbox|wordstrbox] [postprocessing] [correctlettercases] [deskew] [removelines] [removelinebreaks]");
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
            string[] RENDERERS = Enum.GetNames(typeof(Tesseract.RenderedFormat));

            foreach (string arg in args)
            {
                // parse output formats
                if (RENDERERS.Contains(arg.ToUpper()))
                {
                    outputFormatSet.Add(arg.ToUpper());
                }

                if ("postprocessing" == arg)
                {
                    options.PostProcessing = true;
                }
                if ("correctlettercases" == arg)
                {
                    options.CorrectLetterCases = true;
                }
                // pre- and post-processing
                if ("deskew" == arg)
                {
                    options.Deskew = true;
                }
                if ("removelines" == arg)
                {
                    options.RemoveLines = true;
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
            Console.WriteLine(outputFormat);

            string curLangCode = "eng"; //default language
            string psm = "3"; // or alternatively, "Auto"; // 3 - Fully automatic page segmentation, but no OSD (default)

            if (args.Length == 4 || args.Length == 5)
            {
                if (args[2].Equals("-l"))
                {
                    curLangCode = args[3];
                }
                else if (args[2].Equals("-psm"))
                {
                    psm = args[3];
                }
            }
            else if (args.Length == 6 || args.Length == 7)
            {
                curLangCode = args[3];
                psm = args[5];
                try
                {
                    Int16.Parse(psm);
                }
                catch
                {
                    Console.WriteLine("Invalid input value.");
                    return;
                }
            }

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
