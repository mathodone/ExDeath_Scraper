using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace ExDeath
{
    class Search
    {
        public static void TestHeadless()
        {
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl(@"https://www.bing.com/");

            string title = driver.Title;
            string html = driver.PageSource;
            var searchbar = driver.FindElement(By.Id("sb_form_q"));
            searchbar.SendKeys(Keys.Control + "a");
            searchbar.SendKeys(Keys.Delete);
            string term = "martial arts studio";

            searchbar.SendKeys(term + Keys.Enter);

            Console.ReadLine();
        }
        
    }
}
