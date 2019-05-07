using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Net.Http;

namespace ExDeath
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("BEGIN");

            Crawler crawler = new Crawler("https://www.icademy.com/", 2);
            crawler.LoadKeywords(@"../../school_words.txt");
            crawler.Run().Wait();
            Console.WriteLine("DONE");
        }
    }
}
