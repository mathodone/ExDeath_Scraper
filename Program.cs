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
        // this is the queue we'll crawl through when we download the site
        static Queue<string> crawlQueue = new Queue<string>();

        // this loads the client used to load pages. i am not attached to this and
        // will be getting away from this soon since it doesnt support async
        static HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

        // these are the character we split an array by
        static readonly char[] urlSplit = "/:_-#.".ToArray();
        
        // these are the keywords we use to flag links we're interested in.
        // these are loaded from a txt file in the LoadKeywords function
        static HashSet<string> keywords;

        // this is the base Url that we being our crawl from
        static Uri uri;

        // Load keywords from a text file into a hashset
        static void LoadKeywords(string path)
        {
            keywords = new HashSet<string>(File.ReadAllLines(path));
        }

        // Given a base url, collect links on the page that are
        // likely to contain the information desired based on 
        // whether or not the link contains certain keywords
        static void GenerateQueue(Uri url)
        {
            // load url
            HtmlAgilityPack.HtmlDocument doc = web.Load(url);

            // get all the links on the page
            var pageLinks = doc.DocumentNode
                              .Descendants("a")
                              .Select(a => a.Attributes["href"].Value)
                              .ToList();

            foreach (string link in pageLinks)
            {
                // this checks if the link has keywords in it
                if (link.Split(urlSplit).Intersect(keywords).Any())
                {
                    // no dupes allowed
                    if (!crawlQueue.Contains(link))
                    {
                        // add to our queue of links to scrape
                        crawlQueue.Enqueue(link);
                        Logging.QueuedLink(link);
                    }
                }
            }

            Logging.GeneratedQueue(url.ToString());
        }

        // work through the existing queue
        // this setup will probably change
        static void ProcessQueue()
        {
            while (crawlQueue.Count > 0)
            {
                string link = crawlQueue.Dequeue();

                // this function scrapes the link
                ProcessLink(link);
            }

            Logging.ProcessedQueue();
        }

        // do stuff with the link from a queue
        static void ProcessLink(string link)
        {
            // visit page and get source

            Logging.ProcessedLink(link);
        }

        // the main function but will probably be changed in
        // favor of startCrawlerAsync
        static void ScrapeSite(string url)
        {
            // empties log file if exists, otherwise makes new one
            File.WriteAllText("../../crawl_log.txt", string.Empty);
            Logging.StartingCrawl(url);

            // we use a Uri object so that we always know the base domain name
            // this is used to convert relative paths to absolute for the queue
            uri = new Uri(url);

            GenerateQueue(uri);
            //ProcessQueue();
        }

        static async Task startCrawlerAsync()
        {
            //
        }

        static void Main(string[] args)
        {
            Console.WriteLine("BEGIN");

            // load keywords
            LoadKeywords(@"../../school_words.txt");

            // scrape the site
            ScrapeSite("https://www.icademy.com/");

            Console.WriteLine("DONE");
        }
    }
}
