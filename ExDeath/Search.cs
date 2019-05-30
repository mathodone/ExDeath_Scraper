using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using HtmlAgilityPack;

namespace ExDeath
{
    public class Search
    {
        static IWebDriver WebDriver;

        public Search()
        {
            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("--headless");
            WebDriver = new ChromeDriver(options);
        }

        // gets links from first page of results
        public List<string> SearchBing(string term)
        {
            WebDriver.Navigate().GoToUrl($"https://www.bing.com/search?q={term.Replace(' ','+')}");
            string title = WebDriver.Title;
            string html = WebDriver.PageSource;
            //IWebElement searchbar = WebDriver.FindElement(By.Id("sb_form_q"));
            //searchbar.SendKeys(term);
            //System.Threading.Thread.Sleep(5000);
            //WebDriver.FindElement(By.Id("sb_form_go")).Click();

            // load the search results html
            string resultsHtml = WebDriver.PageSource;
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(resultsHtml);

            List<string> resultsLinks = htmlDoc.DocumentNode
                                               .SelectNodes("//li[@class='b_algo']/h2/a ")
                                               .Select(a => a.Attributes["href"].Value)
                                               .ToList();

            return resultsLinks;
        }
    }
}
