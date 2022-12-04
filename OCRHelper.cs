using System.IO;
using System.Drawing;
using VietOCR.NET.Utilities;
using VietOCR.NET.Postprocessing;
using System.Linq;

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

            string workingTiffFile = null;

            try
            {
                // convert PDF to TIFF
                if (imageFile.ToLower().EndsWith(".pdf"))
                {
                    workingTiffFile = PdfUtilities.ConvertPdf2Tiff(imageFile);
                    imageFile = workingTiffFile;
                }

                ocrEngine.ProcessFile(imageFile);

                if (outputFormat.Split(',').Contains(Tesseract.RenderedFormat.TEXT.ToString()))
                {
                    // post-corrections for text output
                    if (options.PostProcessing || options.CorrectLetterCases || options.RemoveLineBreaks)
                    {
                        string outputFilename = outputFile + ".txt";
                        string result = File.ReadAllText(outputFilename);

                        // postprocess to correct common OCR errors
                        if (options.PostProcessing)
                        {
                            result = Processor.PostProcess(result, langCode, options.DangAmbigsPath, options.DangAmbigsEnabled, options.ReplaceHyphens);
                        }

                        // correct letter cases
                        if (options.CorrectLetterCases)
                        {
                            result = TextUtilities.CorrectLetterCases(result);
                        }

                        // remove line breaks
                        if (options.RemoveLineBreaks)
                        {
                            result = Net.SourceForge.Vietpad.Utilities.TextUtilities.RemoveLineBreaks(result, options.RemoveHyphens);
                        }

                        using (StreamWriter sw = new StreamWriter(outputFilename, false, new System.Text.UTF8Encoding()))
                        {
                            sw.Write(result);
                        }
                    }
                }
            }
            finally
            {
                if (workingTiffFile != null && File.Exists(workingTiffFile))
                {
                    File.Delete(workingTiffFile);
                }
            }
        }
    }
}
