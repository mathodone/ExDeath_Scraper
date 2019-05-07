using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using HtmlAgilityPack;

namespace ExDeath
{
    public class Crawler
    {
        // this is the queue we'll crawl through when we download the site
        private Queue<string> crawlQueue;

        // these are the character we split an array by
        static readonly char[] urlSplit = "/:_-#.".ToArray();

        // these are the keywords we use to flag links we're interested in.
        // these are loaded from a txt file in the LoadKeywords function
        static HashSet<string> keywords;

        // this is the base Url that we being our crawl from
        static Uri uri;

        public Crawler(string url, int maxConcurrentConnections)
        {
            crawlQueue = new Queue<string>();
            uri = new Uri(url);
            ServicePointManager.DefaultConnectionLimit = maxConcurrentConnections;
        }

        // Load keywords from a text file into a hashset
        public void LoadKeywords(string path)
        {
            keywords = new HashSet<string>(File.ReadLines(path));
        }

        public async Task GenerateQueueAsync()
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


                    Logging.GeneratedQueue(uri.ToString());
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Exception: ", e);
                }
            }
        }

        public async Task<bool> Run()
        {
            File.WriteAllText("../../crawl_log.txt", string.Empty);
            Logging.StartingCrawl(uri.ToString());
            await GenerateQueueAsync();

            return true;
        }
    }
}
