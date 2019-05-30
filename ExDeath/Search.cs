using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using HtmlAgilityPack;
using System.Net.Http;

namespace ExDeath
{
    public class Search
    {
        static HttpClient client;

        public Search()
        {
            client = new HttpClient();
        }

        // gets links from first page of results
        public async Task<List<string>> SearchBing(string term)
        {
            using (var response = await client.GetAsync($"https://www.bing.com/search?q={term.Replace(' ', '+')}"))
            {
                response.EnsureSuccessStatusCode();
                var source = await response.Content.ReadAsStringAsync();

                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(source);

                List<string> resultsLinks = htmlDoc.DocumentNode
                                   .SelectNodes("//li[@class='b_algo']/h2/a ")
                                   .Select(a => a.Attributes["href"].Value)
                                   .ToList();

                return resultsLinks;
            }
        }
    }
}
