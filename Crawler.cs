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
        // how far down the rabbit hole goes
        int maxDepth;

        // so children don't loop back to already seen node
        static HashSet<string> seen = new HashSet<string>();

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

        private static HttpClient client;

        static string downloadsDirectory;

        public Crawler(string url, bool usekeywords = false, int maxConnections = 2, int depth = 2)
        {
            maxDepth = depth;
            crawlQueue = new Queue<string>();
            uri = new Uri(url);
            client = new HttpClient();
            useKeywords = usekeywords;
            downloadsDirectory = $"../../downloads/{uri.Host}";

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
        public async Task GenerateQueueAsync()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                // save html of root page to file
                File.WriteAllText(downloadsDirectory + $"/{uri.Host}.txt", responseBody);

                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(responseBody);

                // get all links on the page
                List<string> pageLinks = htmlDoc.DocumentNode
                                            .Descendants("a")
                                            .Select(a => a.Attributes["href"].Value)
                                            .ToList();

                foreach (string link in pageLinks)
                {
                    // turn relative paths to absolute
                    string fixedlink = link.StartsWith("/") ? $"{uri.Scheme}://{uri.Host}/{link.Substring(1)}" : link;
                    //add it if its new and contains keywords
                    if (!crawlQueue.Contains(fixedlink))
                    {
                        if (!useKeywords || fixedlink.Split(urlSplit).Intersect(keywords).Any())
                        {
                            lock (seen)
                            {
                                if (!seen.Contains(fixedlink))
                                {
                                    seen.Add(fixedlink);
                                    crawlQueue.Enqueue(fixedlink);
                                    Logging.QueuedUrl(fixedlink);
                                }
                            }
                        }
                    }
                }

                Logging.GeneratedQueue(uri.ToString());
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Exception: ", e);
                Logging.FailedQueue(uri.ToString());
            }
        }

        // visit a url and get the source html, then save the html to a file
        // the reason we save html to a file is because we can process the html
        // offline much more quickly
        public async Task ProcessUrlAsync(string url, bool saveImages = false)
        {
            Logging.ProcessingNewUrl(url);

            try
            {
                string path = new Uri(url).LocalPath.Substring(1);
                DirectoryInfo di = Directory.CreateDirectory($"{downloadsDirectory}/{path}");

                HttpResponseMessage response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                using (StreamWriter outputFile = new StreamWriter($"{downloadsDirectory}/{path}_html.txt"))
                {
                    await outputFile.WriteAsync(responseBody);
                    await outputFile.FlushAsync();
                }


                //if (saveImages)
                //{
                //    DownloadImages(responseBody);
                //    HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlDocument();
                //    htmlDoc.LoadHtml(responseBody);
                //}

                Logging.ProcessedUrl(url);

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Exception: ", e);
                Logging.FailedUrl(url);
            }
        }

        public static void DownloadImages(string html)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            // get all image nodes on the page
            List<string> imgLinks = htmlDoc.DocumentNode
                                        .Descendants("img")
                                        .Select(img => img.Attributes["src"].Value)
                                        .ToList();

            foreach (string imgLink in imgLinks)
            {
                //
            }
        }

        // create child with 1 less depth
        public async Task Spawn(string url)
        {
            await ProcessUrlAsync(url);
            if (maxDepth > 1)
            {
                Crawler crawler = new Crawler(url, depth: maxDepth - 1, 
                                            usekeywords: useKeywords, 
                                            maxConnections: ServicePointManager.DefaultConnectionLimit);
                await crawler.Run();
            }
        }

        public async Task Run()
        {
            // clear log file if exists, otherwise make new log file
            File.WriteAllText("../../logs/crawl_log.txt", string.Empty);
            Logging.StartingCrawl(uri.ToString());
            var runningTasks = new List<Task>();

            // create new directory to store files
            DirectoryInfo di = Directory.CreateDirectory(downloadsDirectory);

            runningTasks.Add(GenerateQueueAsync());

            while (runningTasks.Any())
            {
                var firstCompletedTask = await Task.WhenAny(runningTasks);
                runningTasks.Remove(firstCompletedTask);

                // if we still have pages to crawl and connections available
                while (crawlQueue.Any() && runningTasks.Count < ServicePointManager.DefaultConnectionLimit)
                {
                    var url = crawlQueue.Dequeue();
                    // create recursive child on each crawlQueue
                    runningTasks.Add(Task.Run(() => Spawn(url)));
                }
            }
            await Task.WhenAll(runningTasks);

            Logging.CrawlFinished(uri.ToString());
            //return true;
        }
    }
}
