using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using VietOCR.NET.Utilities;
using VietOCR.NET.Postprocessing;

namespace VietOCR
{
    class OCRHelper
    {
        /// <summary>
        /// Performs OCR for bulk/batch and console operations.
        /// </summary>
        /// <param name="imageFile">Image file</param>
        /// <param name="outputFile">Output file without extension</param>
        /// <param name="langCode">language code</param>
        /// <param name="pageSegMode">page segmentation mode</param>
        /// <param name="outputFormat">format of output file. Possible values: <code>text</code>, <code>text+</code> (with post-corrections), <code>hocr</code></param>
        /// <param name="deskew">deskew</param>
        public static void PerformOCR(string imageFile, string outputFile, string langCode, string pageSegMode, string outputFormat, ProcessingOptions options)
        {
            DirectoryInfo dir = Directory.GetParent(outputFile);
            if (dir != null && !dir.Exists)
            {
                dir.Create();
            }

            OCR<Image> ocrEngine = new OCRImages();
            ocrEngine.PageSegMode = pageSegMode;
            ocrEngine.Language = langCode;
            ocrEngine.OutputFormat = outputFormat;
            ocrEngine.OutputFile = outputFile;
            ocrEngine.ProcessingOptions = options;

            // convert PDF to TIFF
            if (imageFile.ToLower().EndsWith(".pdf"))
            {
                imageFile = PdfUtilities.ConvertPdf2Tiff(imageFile);
            }

            ocrEngine.ProcessFile(imageFile);

            // post-corrections for text+ output
            if (options.PostProcessing)
            {
                string filename = outputFile + ".txt";
                string result = File.ReadAllText(filename);
                // postprocess to correct common OCR errors
                result = Processor.PostProcess(result, langCode);
                // correct letter cases
                result = TextUtilities.CorrectLetterCases(result);

                using (StreamWriter sw = new StreamWriter(filename, false, new System.Text.UTF8Encoding()))
                {
                    sw.Write(result);
                }
            }
        }
    }
}
