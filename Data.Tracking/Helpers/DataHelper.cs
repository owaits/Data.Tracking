using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Data.Tracking.Helpers
{
    public static class DataHelper
    {
        public static Regex urlMatch = new Regex(@"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string FormatHtmlLinks(string text)
        {
            return urlMatch.Replace(text, "<a target='_blank' href='$1'>$1</a>");
        }

    }
}
