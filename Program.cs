using System;

namespace ExDeath
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("BEGIN");

            Crawler crawler = new Crawler("http://www.chinavitae.org/biography_browse.php?l=A", false);
            crawler.LoadKeywords(@"../../keywords/chinavitae_words.txt");
            crawler.Run().Wait();

            Console.WriteLine("DONE");
        }
    }
}
