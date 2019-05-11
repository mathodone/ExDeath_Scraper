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
            Crawler crawler = new Crawler("http://mdn.github.io/beginner-html-site-styled/", depth:2, maxConnections: 3);
            //Crawler crawler = new Crawler("http://www.chinavitae.org/biography_browse.php?l=A");
            //crawler.LoadKeywords(@"../../keywords/chinavitae_words.txt");
            crawler.Run().Wait();

            //Search.SearchBing("martial arts studio");
            Console.WriteLine("DONE");
        }
    }
}
    