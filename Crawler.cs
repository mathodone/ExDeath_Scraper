using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExDeath
{
    public class Crawler
    {
        // list of links to crawl. we obtain this from the root url
        Queue<string> crawlQueue = new Queue<string>();

        // these are the character we split an array by
        static readonly char[] urlSplit = "/:_-#.".ToArray();

        // whether or not to use a keywords list
        bool useKeywords;

        // these are the keywords we use to flag links we're interested in.
        // these are loaded from a txt file in the LoadKeywords function
        static HashSet<string> keywords;

        // this is the base Url that we being our crawl from
        static Uri uri;

        public Crawler(string url, bool usekeywords = false, int maxConnections = 2)
        {
            crawlQueue = new Queue<string>();
            uri = new Uri(url);
            useKeywords = usekeywords;
            // max # of connections allowed to an IP in parallel
            // if too high, the program will be throttled/blocked. best to use 2 for most sites.
            ServicePointManager.DefaultConnectionLimit = maxConnections;
        }

        // Load keywords from a text file into a hashset
        public void LoadKeywords(string path)
        {
            keywords = new HashSet<string>(File.ReadLines(path));
        }
        
        // gets a list of links from a page and adds them
        // to the crawlQueue
        public async Task<bool> GenerateQueue()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(uri);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(responseBody);

                    // get all links on the page
                    List<string> pageLinks = htmlDoc.DocumentNode
                                                .Descendants("a")
                                                .Select(a => a.Attributes["href"].Value)
                                                .ToList();

                    if (useKeywords)
                    {
                        foreach (string link in pageLinks)
                        {
                            // check if link has any keywords in it
                            if (link.Split(urlSplit).Intersect(keywords).Any())
                            {
                                // no dupes allowed
                                if (!crawlQueue.Contains(link))
                                {
                                    crawlQueue.Enqueue(link);
                                    Logging.QueuedLink(link);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (string link in pageLinks)
                        {
                            if (!crawlQueue.Contains(link))
                            {
                                crawlQueue.Enqueue(link);
                                Logging.QueuedLink(link);
                            }
                        }
                    }

                    Logging.GeneratedQueue(uri.ToString());
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Exception: ", e);
                }
            }

            return true;
        }

        //public async Task<IEnumerable<string>> processUrl(string url)
        //{
        //    return 
        //}

        public async Task<bool> Run()
        {
            // clear log file if exists, otherwise make new log file
            File.WriteAllText("../../crawl_log.txt", string.Empty);
            Logging.StartingCrawl(uri.ToString());
            var runningTasks = new List<Task>();
            var result = new List<string>();

            runningTasks.Add(GenerateQueue());
            result.Add(uri.ToString());

            while (runningTasks.Any())
            {
                var firstCompletedTask = await Task.WhenAny(runningTasks);
                runningTasks.Remove(firstCompletedTask);
                
                // if we still have pages to crawl and connections available
                while (crawlQueue.Any() && runningTasks.Count < ServicePointManager.DefaultConnectionLimit)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var url = crawlQueue.Dequeue();
                        //runningTasks.Add(processUrl(url));
                    }
                }

            }

            Logging.CrawlFinished(uri.ToString());
            return true;
        }
    }
}
