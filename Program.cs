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
        static Queue<string> crawlQueue = new Queue<string>();
        static HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
        static readonly char[] urlSplit = "/:_-#.".ToArray();
        static HashSet<string> keywords;
        static Uri uri;

        // Load keywords from a text file into a hashset
        static void LoadKeywords(string path)
        {
            keywords = new HashSet<string>(File.ReadAllLines(path));
        }

        // Given a base url, collect links on the page that are
        // likely to contain the information desired
        static void GenerateQueue(Uri url)
        {
            HtmlAgilityPack.HtmlDocument doc = web.Load(url);
            var pageLinks = doc.DocumentNode
                              .Descendants("a")
                              .Select(a => a.Attributes["href"].Value)
                              .ToList();

            foreach (string link in pageLinks)
            {
                if (link.Split(urlSplit).Intersect(keywords).Any())
                {
                    if (!crawlQueue.Contains(link))
                    {
                        crawlQueue.Enqueue(link);
                        Logging.QueuedLink(link);
                    }
                }
            }

            Logging.GeneratedQueue(url.ToString());
        }

        static void ProcessQueue()
        {
            while (crawlQueue.Count > 0)
            {
                string link = crawlQueue.Dequeue();
                ProcessLink(link);
            }

            Logging.ProcessedQueue();
        }

        static void ProcessLink(string link)
        {
            // vist page and get source

            Logging.ProcessedLink(link);
        }

        static void ScrapeSite(string url)
        {
            Logging.StartingCrawl(url);
            uri = new Uri(url);
            GenerateQueue(uri);
            //ProcessQueue();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("BEGIN");

            // empty existing log file if exists, otherwise creates new empty one
            File.WriteAllText("../../crawl_log.txt", string.Empty);
            LoadKeywords(@"../../school_words.txt");
            ScrapeSite("https://www.icademy.com/");
            Console.WriteLine("DONE");
        }
    }
}
