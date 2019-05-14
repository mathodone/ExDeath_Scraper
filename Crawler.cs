using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExDeath
{
    public class Crawler
    {
        static HttpClient client;

        // this is the base Url that we being our crawl from
        static Uri uri;

        // how far down the rabbit hole goes
        static int maxDepth;

        // so children don't loop back to already seen node
        // TODO: use bloom filter. optimizes lookup
        HashSet<Uri> seen;

        // list of links to crawl. we obtain this from the root url
        // TODO: make to into hashset. that way don't need to check for dupes
        ConcurrentQueue<Uri> crawlQueue;                            

        // these are the character we split an array by
        static readonly char[] urlSplit = "/:_-#.".ToArray();

        // whether or not to use a keywords list
        bool useKeywords;

        // these are the keywords we use to flag links we're interested in.
        // these are loaded from a txt file in the LoadKeywords function
        static HashSet<string> keywords;

        //Downloader _downloader;

        static string downloadsDirectory;

        public Crawler(string url, bool usekeywords = false, int maxConnections = 2, int depth = 2)
        {
            maxDepth = depth;
            crawlQueue = new ConcurrentQueue<Uri>();
            uri = new Uri(url);
            client = new HttpClient();
            seen = new HashSet<Uri>();
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
        
        // gets a list of links from a given page and add them
        // to the crawlQueue
        public async Task GenerateQueueAsync(Uri url)
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
                    string fixedLink;

                    if (!link.StartsWith("http"))
                    {
                        if (link.StartsWith("/"))
                        {
                            fixedLink = $"{url.Scheme}://{url.Host}/{link.Substring(1)}";
                        }
                        else
                        {
                            fixedLink = $"{url.Scheme}://{url.Host}/{link}";
                        }
                    }
                    else
                    {
                        fixedLink = link;
                    }

                    //string fixedLink = !link.StartsWith("http") ? $"{url.Scheme}://{url.Host}/{link}" : link;
                    Uri fixedUri = new Uri(fixedLink);
                    //add fixedUri to crawlQueue if not in seen
                    if (!seen.Contains(fixedUri))
                    {
                        seen.Add(fixedUri);
                        crawlQueue.Enqueue(fixedUri);

                        //TODO: add fuzzy matching for keywords and/or regex pattern matching
                        //if (!useKeywords || fixedLink.Split(urlSplit).Intersect(keywords).Any())
                        //{
                        //    lock (seen)
                        //    {
                        //        if (!seen.Contains(fixedLink))
                        //        {
                        //            seen.Add(fixedLink);
                        //            crawlQueue.Enqueue(fixedLink);
                        //            Logging.QueuedUrl(fixedLin);
                        //        }
                        //    }
                        //}
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

        // download HTML of a page
        private async Task ProcessUrlAsync(Uri url, CancellationToken cancellationToken)
        {
            Logging.ProcessingNewUrl(url.ToString());

            using (var response = await client.GetAsync(url, cancellationToken))
            {
                //response.EnsureSuccessStatusCode();
                var source = await response.Content.ReadAsStringAsync();

                string directory = $"{downloadsDirectory}/{url.AbsolutePath.Substring(1)}";
                Directory.CreateDirectory(directory);
                string filepath = $"{directory.Substring(0, directory.LastIndexOf('/'))}/html.txt";

                using (StreamWriter outputFile = new StreamWriter(filepath))
                {
                    await outputFile.WriteAsync(source);
                    await outputFile.FlushAsync();
                }
            }

            return;
        }

        // create child with 1 less depth
        // TODO: clean up dead children
        //public async Task Spawn(string url)
        //{
        //    await ProcessUrlAsync(url);
        //    if (maxDepth > 1)
        //    {
        //        Crawler crawler = new Crawler(url, depth: maxDepth - 1, 
        //                                    usekeywords: useKeywords, 
        //                                    maxConnections: ServicePointManager.DefaultConnectionLimit);
        //        await crawler.Run();
        //    }
        //}

        private Task TryProcessQueue(CancellationToken cancellationToken)
        {
            if (crawlQueue.TryDequeue(out Uri poppedUri))
            {
                return ProcessUrlAsync(poppedUri, cancellationToken);
            }

            return null;
        }

        public async Task Run()
        {
            // clear log file if exists, otherwise make new log file
            File.WriteAllText("../../logs/crawl_log.txt", string.Empty);
            Logging.StartingCrawl(uri.ToString());

            // generate our CancellationTokens
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            // create new directory to store files
            DirectoryInfo siteDirectory = Directory.CreateDirectory(downloadsDirectory);

            var runningTasks = new HashSet<Task> { GenerateQueueAsync(uri) };
            var maxTasks = ServicePointManager.DefaultConnectionLimit;

            void AddProcessTask()
            {
                var task = TryProcessQueue(cancellationToken: token);
                if (task != null)
                {
                    runningTasks.Add(task);
                }
            }

            while (runningTasks.Count > 0 && !token.IsCancellationRequested)
            {
                try
                {
                    var completedTask = await Task.WhenAny(runningTasks);
                    runningTasks.Remove(completedTask);
                }
                catch(TaskCanceledException)
                {
                    return;
                }

                AddProcessTask();

                if (crawlQueue.Count > maxTasks && runningTasks.Count < maxTasks)
                {
                    AddProcessTask();
                }
            }



            //while (runningTasks.Any())
            //{
            //    var firstCompletedTask = await Task.WhenAny(runningTasks);
            //    runningTasks.Remove(firstCompletedTask);

            //    // if we still have pages to crawl and connections available
            //    // TODO: manage request delays and maximum connections per domain
            //    while (crawlQueue.Any() && runningTasks.Count < ServicePointManager.DefaultConnectionLimit)
            //    {
            //        crawlQueue.TryDequeue(out Uri url);
            //        // create recursive child on each crawlQueue
            //        runningTasks.Add(Task.Run(() => Spawn(url)));
            //    }
            //}
            //await Task.WhenAll(runningTasks);

            Logging.CrawlFinished(uri.ToString());
            return;
        }
    }
}
