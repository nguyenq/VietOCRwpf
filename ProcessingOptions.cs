using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietOCR
{
    public class ProcessingOptions
    {
        public bool Deskew { get; set; }
        public bool PostProcessing { get; set; }
        public bool RemoveLines { get; set; }
        public bool RemoveLineBreaks { get; set; }
        public bool CorrectLetterCases { get; set; }
        public bool RemoveHyphens { get; set; }
        public bool ReplaceHyphens { get; set; }
        public bool DangAmbigsEnabled { get; set; }
        public string DangAmbigsPath { get; set; }
    }
}
