using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ExDeath
{
    // contains functions for searching/scanning html data
    public class Sift
    {
        const RegexOptions _options = RegexOptions.Compiled | RegexOptions.IgnoreCase;
        static string PhoneRegex = @"(1 |\+1 )?[ .-]?(\([0-9]{3}\) ?|[0-9]{3} ?)[ .-]?[0-9]{3}[ .-][0-9]{4}[ ]?(( |x|ext|ext.|extension|#){1}[ ]?([0-9]){1,7}){0,1}";
        static string EmailRegex = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
        static string AddressRegex = @"(([0-9]{1,6}|One|Two|Three) .* [0-9]{5}([- ][0-9]{4})?|[0-9]{1,5} .* [A-Za-z]{2})|([0-9]{1,4} .* Suite [0-9]+)|(((([P|p][.]?[ ]?[O|o][.]?[ ]?)([B|b][O|o][X|x])?)|(Post Office Box)|([B|b)][O|o][X|x])) .* ([0-9]{5}([- ][0-9]{4})?|[A-Za-z]{2})?)|(([0-9]{1,5}|[0-9]{1}[A-Z]{1}[A-Z]{1}|[A-Z]{1}[0-9]{1}[A-Z]{1}) .* (Canada|United Kingdom|Papua New Guinea|United States|Norway|France))|((London|Canada) ([0-9]{1,5}|[0-9]{1}[A-Z]{1}[A-Z]{1}|[A-Z]{1}[0-9]{1}[A-Z]{1}))|([0-9]{1,5}.* [0-9]{5})";
        static string HtmlRegex = @"<(.|\n)*?>";
        static string customRegex;

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

        public static IEnumerable<HtmlNode> SearchPage(string html, string term)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var nodes = from div in htmlDoc.DocumentNode.Descendants("div")
                        from text in div.DescendantsAndSelf("text()")
                        where text.InnerText.Contains(term)
                        let firstParent = text.AncestorsAndSelf("div").First()
                        select firstParent;

            return nodes;

        }

    }
}
