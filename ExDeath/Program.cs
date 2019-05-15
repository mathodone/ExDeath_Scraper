using System;
using System.Collections.Generic;
using System.Linq;

namespace ExDeath
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("BEGIN");
            Crawler crawler = new Crawler("http://books.toscrape.com/", depth:2, maxConnections: 2, dlImage: true);
            //Crawler crawler = new Crawler("http://www.chinavitae.org/biography_browse.php?l=A");
            //crawler.LoadKeywords(@"../../keywords/chinavitae_words.txt");
            crawler.Run().Wait();
            Console.WriteLine("DONE");
        }
    }
}
    