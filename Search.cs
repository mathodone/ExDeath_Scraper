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
    class Search
    {
        public static void SearchBing(string term)
        {
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl(@"https://www.bing.com/");

            string title = driver.Title;
            string html = driver.PageSource;
            var searchbar = driver.FindElement(By.Id("sb_form_q"));
            searchbar.SendKeys(Keys.Control + "a");
            searchbar.SendKeys(Keys.Delete);
            searchbar.SendKeys(term);
            searchbar.SendKeys(Keys.Enter);

            // load the search results html
            var resultsHtml = driver.PageSource;
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(resultsHtml);

            var resultsLinks = htmlDoc.DocumentNode
                                       .SelectNodes("//li[@class='b_algo']/h2/a ")
                                       .Select(a => a.Attributes["href"].Value)
                                       .ToList();

            Console.ReadLine();
        }
    }
}
