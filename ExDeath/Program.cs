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
            Crawler crawler = new Crawler("http://books.toscrape.com/", depth:2, maxConnections: 2);
            //Crawler crawler = new Crawler("http://www.chinavitae.org/biography_browse.php?l=A");
            //Crawler crawler = new Crawler("https://d-fens.ch/2014/04/12/httpclient-and-how-to-use-headers-content-type-and-postasync/", useSearch: true, searchTerm: "curriculum");
            //crawler.LoadKeywords(@"../../keywords/school_words.txt");
            //crawler.Run();
            Console.WriteLine("DONE");
        }
    }
}
    