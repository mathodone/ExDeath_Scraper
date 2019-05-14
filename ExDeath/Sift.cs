using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ExDeath
{
    // contains functions for searching/scanning html data
    class Sift
    {
        public const RegexOptions _options = RegexOptions.Compiled | RegexOptions.IgnoreCase;
        public static string PhoneRegex = @"(?:(?:t(?:ele?(?:phone)?)?|p(hone)?|fa?(?:x|cs(?:im(?:ile)?)?))[\:\.\s]*)?(\+?(?:[0-1]{1}[-.\s])?\(?(?:[0-9]{3})\)?[-. ]?(?:[0-9]{3})[-. ]?(?:[0-9]{4}))";
        public static string EmailRegex = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
        public static string AddressRegex = @"(([0-9]{1,6}|One|Two|Three) .* [0-9]{5}([- ][0-9]{4})?|[0-9]{1,5} .* [A-Za-z]{2})|([0-9]{1,4} .* Suite [0-9]+)|(((([P|p][.]?[ ]?[O|o][.]?[ ]?)([B|b][O|o][X|x])?)|(Post Office Box)|([B|b)][O|o][X|x])) .* ([0-9]{5}([- ][0-9]{4})?|[A-Za-z]{2})?)|(([0-9]{1,5}|[0-9]{1}[A-Z]{1}[A-Z]{1}|[A-Z]{1}[0-9]{1}[A-Z]{1}) .* (Canada|United Kingdom|Papua New Guinea|United States|Norway|France))|((London|Canada) ([0-9]{1,5}|[0-9]{1}[A-Z]{1}[A-Z]{1}|[A-Z]{1}[0-9]{1}[A-Z]{1}))|([0-9]{1,5}.* [0-9]{5})";
        public static string HtmlRegex = @"<(.|\n)*?>";

        // removes html tags and returns only the text
        public static string StripHTML(string html)
        {
            string clean = Regex.Replace(html, HtmlRegex, " ", _options);
            return Regex.Replace(clean, " {2,}", " ", _options).Replace("amp", " ").Trim();
        }

        public static MatchCollection ExtractMatches(string text, string regex)
        {
            string clean = StripHTML(text);
            return Regex.Matches(clean, regex);
        }
    }
}
