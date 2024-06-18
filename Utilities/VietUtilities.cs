using System;

using System.Text;
using System.Text.RegularExpressions;

namespace Net.SourceForge.Vietpad.Utilities
{
    class VietUtilities
    {
        //private static readonly ILog logger = LogFactory.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /**
         * Strips accents off words.
         */
        public static string StripDiacritics(string accented)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");

            string strFormD = accented.Normalize(NormalizationForm.FormD);
            return regex.Replace(strFormD, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
    }
}