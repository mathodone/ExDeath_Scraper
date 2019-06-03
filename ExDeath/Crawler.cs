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
        // is dictionary with trash var byte because ConcurrentSet doesn't exist
        ConcurrentDictionary<Uri, byte> seen;

        // list of links to crawl. we obtain this from the root url
        // int = depth level
        ConcurrentQueue<Tuple<Uri, int>> crawlQueue;                            

        // these are the character we split an array by
        static readonly char[] urlSplit = "/:_-#.".ToArray();

        // whether or not to use a keywords list
        bool useKeywords;

        // if we want to search our pages for a certain term
        bool searchPages;
        string searchTerm;

        // these are the keywords we use to flag links we're interested in.
        // these are loaded from a txt file in the LoadKeywords function
        static HashSet<string> keywords;

        //Downloader _downloader;

        // whether or not we want to download the page html
        bool downloadHtml, downloadImage;

        static string downloadsDirectory;

        public Crawler(string url, bool usekeywords = false, int maxConnections = 2, int depth = 2, bool dlHtml = false, bool dlImage = false, bool useSearch = false, string searchTerm = "")

        {
            maxDepth = depth;
            crawlQueue = new ConcurrentQueue<Tuple<Uri, int>>();
            uri = new Uri(url);
            client = new HttpClient();
            seen = new ConcurrentDictionary<Uri, byte>();
            useKeywords = usekeywords;
            downloadsDirectory = $"../../downloads/{uri.Host}";
            downloadHtml = dlHtml;
            downloadImage = dlImage;
            searchPages = useSearch;
            this.searchTerm = searchTerm;


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
        public async Task GenerateQueueAsync(Tuple<Uri, int> urlTuple, string html)
        {
            try
            {
                var url = urlTuple.Item1;
                var level = urlTuple.Item2;

                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

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
                    if (!seen.ContainsKey(fixedUri))
                    {
                        seen.TryAdd(fixedUri,0);

                        crawlQueue.Enqueue(Tuple.Create(fixedUri, level + 1));
                        //crawlQueue.Enqueue(fixedUri);
                    }
                }

                Logging.GeneratedQueue(url.ToString());
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Exception: ", e);
                Logging.FailedQueue(urlTuple.Item1.ToString());
            }
        }

        // process a url from the queue
        public async Task ProcessUrlAsync(Tuple<Uri, int> urlTuple, CancellationToken cancellationToken)
        {
            var url = urlTuple.Item1; 
            Logging.ProcessingNewUrl(url.ToString());

            using (var response = await client.GetAsync(url, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var source = await response.Content.ReadAsStringAsync();


                if (searchPages)
                {
                    IEnumerable<HtmlNode> nodes = Sift.SearchPage(source, searchTerm);
                    Console.WriteLine();
                }

                if (downloadHtml)
                {
                    // save html to file
                    await Downloader.DownloadHtml(source, url, downloadsDirectory);
                }
                if (downloadImage)
                {
                    // save all images to file
                    await Downloader.DownloadImages(source, url, downloadsDirectory);
                }

                // get all links from the page for queue
                if (urlTuple.Item2 < maxDepth)
                {
                    await GenerateQueueAsync(urlTuple, source);
                }
            }

            return;
        }

        private Task TryProcessQueue(CancellationToken cancellationToken)
        {
            if (crawlQueue.TryDequeue(out Tuple<Uri, int> poppedUri))
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

            var runningTasks = new HashSet<Task> { ProcessUrlAsync(Tuple.Create(uri,0), token) };
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

            Logging.CrawlFinished(uri.ToString());
            return;
        }
    }
}
