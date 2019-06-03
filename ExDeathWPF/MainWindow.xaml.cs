using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.XPath;
using HtmlAgilityPack;

namespace ExDeathWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public bool DlHtml { get; set; }
        public bool DlImage { get; set; }
        public bool UseKeywords { get; set; }
        public bool UseSearch { get; set; }
        
        // search results from search tab
        public ObservableCollection<SearchResult> SearchResults { get; private set; }

        public ObservableCollection<CrawlResult> CrawlResults { get; private set; }
        public ObservableCollection<CrawlQueueResult> CrawlQueueResults { get; private set; }

        // lock object for synchronization
        private readonly static object _syncLock = new object();

        class MainViewModel
        {
            ObservableCollection<object> _children;

            public MainViewModel()
            {
                _children = new ObservableCollection<object>();
                _children.Add(new CrawlerViewModel());
                _children.Add(new SearchViewModel());
            }

            public ObservableCollection<object> Children { get { return _children; } }
        }

        class CrawlerViewModel {}
        public class SearchViewModel {}

        public class SearchResult
        {
            public string Term { get; set; }
            public string Link { get; set; }
        }

        public class CrawlResult
        {
            public string BaseUrl { get; set; }
            public string FoundUrl { get; set; }
            public string InnerText { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
        }

        public class CrawlQueueResult
        {
            public string BaseUrl { get; set; }
            public string QueuedUrl { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            SearchResults = new ObservableCollection<SearchResult>();
            CrawlResults = new ObservableCollection<CrawlResult>();
            CrawlQueueResults = new ObservableCollection<CrawlQueueResult>();

            // enable cross access to this collection elsewhere
            BindingOperations.EnableCollectionSynchronization(SearchResults, _syncLock);
            BindingOperations.EnableCollectionSynchronization(CrawlResults, _syncLock);
            BindingOperations.EnableCollectionSynchronization(CrawlQueueResults, _syncLock);
        }

        // Button____ handles the UI/instantion work when button clicked
        // the Perform___ function runs in another thread
        private async void Button_Search(object sender, RoutedEventArgs e)
        {
            SearchResults.Clear();
            dgSearchResults.ItemsSource = SearchResults;

            // get search terms and split by comma
            string[] terms = searchTerm.Text.Split(',');

            searchProgress.Value = 0;
            searchProgress.Maximum = 100;

            var progress = new Progress<int>(v =>
            {
                searchProgress.Value = v;
            });

            // where to save output file with results
            string filepath = $"../../search_results/{searchFilename.Text}.txt";

            // run search in separate thread so UI doesnt block
            await Task.Run(() => PerformSearch(progress, terms, filepath));
        }

        private async void PerformSearch(IProgress<int> progress, string[] terms, string filepath)
        {
            var driver = new ExDeath.Search();

            // clear log file if exists, otherwise make new log file
            File.WriteAllText(filepath, string.Empty);

            // for progress bar
            var term_count = 0;

            foreach (string term in terms)
            {
                term_count++;
                Console.WriteLine($"Searching for term:{term}");
                List<string> results = await Task.Run(() => driver.SearchBing(term));
                lock (_syncLock)
                {
                    SearchResults.Add(new SearchResult() { Term = term, Link = results.First() });
                }

                if (progress != null)
                {
                    progress.Report(term_count * 100 / terms.Length);
                }

                using (StreamWriter outputFile = File.AppendText(filepath))
                {
                    await outputFile.WriteAsync($"{term},{results.First()}{Environment.NewLine}");
                    await outputFile.FlushAsync();
                }

                System.Threading.Thread.Sleep(2000);
                
            }

            Console.WriteLine("Search Complete");
            MessageBox.Show("Done Searching", "ExDeath Scraper", MessageBoxButton.OK);
        }

        private async void Button_Crawl(object sender, RoutedEventArgs e)
        {
            CrawlResults.Clear();
            dgCrawlResults.ItemsSource = CrawlResults;
            CrawlQueueResults.Clear();
            dgCrawlQueue.ItemsSource = CrawlQueueResults;

            string baseUrl = crawlUrl.Text;

            await Task.Run(() => PerformCrawl(baseUrl));
        }

        private async void PerformCrawl(string baseUrl)
        {
            Uri url = new Uri(baseUrl);
            var client = new HttpClient();
            HashSet<string> seen = new HashSet<string>();
            Queue<Uri> crawlQ = new Queue<Uri>();
            crawlQ.Enqueue(url);

            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.47 Safari/537.36");

            while (crawlQ.Count > 0)
            {
                string nextUrl = crawlQ.Dequeue().ToString();

                using (var response = await client.GetAsync(nextUrl))
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                        var source = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(response.StatusCode);

                        HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(source);

                        Regex rx = new Regex(@"\S*(directory|administration|information|department|curriculum|director|instruction|administrative|staff)", RegexOptions.IgnoreCase);

                        List<string> pageLinks = htmlDoc.DocumentNode
                                            .Descendants("a")
                                            .Where(a => rx.IsMatch(a.InnerText))
                                            .Select(a =>  a.GetAttributeValue("href", null))
                                            .ToList();

                        foreach (string link in pageLinks)
                        {
                            string fixedLink;

                            if (!link.StartsWith("http"))
                            {
                                if (link.StartsWith("/"))
                                {
                                    fixedLink = $"{url.Scheme}://{url.Host}/{link.Substring(1)}".ToLower();
                                }
                                else
                                {
                                    fixedLink = $"{url.Scheme}://{url.Host}/{link}".ToLower();
                                }
                            }
                            else
                            {
                                fixedLink = link.ToLower();
                            }

                            if (!seen.Contains(fixedLink))
                            {
                                if (url.Host == new Uri(fixedLink).Host)
                                {
                                    seen.Add(fixedLink);
                                    crawlQ.Enqueue(new Uri(fixedLink));

                                    lock (_syncLock)
                                    {
                                        CrawlQueueResults.Add(new CrawlQueueResult() { BaseUrl = url.ToString(), QueuedUrl = fixedLink});
                                    }

                                    Console.WriteLine("queued", fixedLink);
                                }
                            }
                        }

                        Regex directorRx = new Regex(@"((director of curriculum (&|and|&amp;) instruction)|(curriculum development))+", RegexOptions.IgnoreCase);
                        Regex emailRx = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", RegexOptions.IgnoreCase);
                        Regex phoneRx = new Regex(@"(?:\(\d{3}\)\s*|(?=[\d.-]{10,20})\b\d{3}[-.]?)\d{3}[-.]?\d{4}( *x\d{4})?\b", RegexOptions.IgnoreCase);


                        List<string> directorMatches = htmlDoc.DocumentNode.SelectNodes(".//div")
                                                        .Where(s => directorRx.IsMatch(s.InnerText))
                                                        .Select(a => a.InnerText.Trim())
                                                        .Select(a => Regex.Replace(a, "[ \t\r\n]+", @" ").Trim())
                                                        .ToList();

                        HashSet<string> parsedMatches = new HashSet<string>(directorMatches);

                        HashSet<string> substringMatches = new HashSet<string>();

                        foreach (string parsed in parsedMatches)
                        {
                            MatchCollection collection = directorRx.Matches(parsed);
                            
                            foreach (Match fragment in collection)
                            {
                                string substring = parsed.Substring(fragment.Index-50, 150);
                                substringMatches.Add(substring);
                            }

                        }

                        Console.WriteLine("test");

                        foreach (string text in substringMatches)
                        {
                            lock (_syncLock)
                            {
                                CrawlResults.Add(
                                    new CrawlResult() {
                                        BaseUrl = url.ToString(),
                                        FoundUrl = nextUrl.ToString(),
                                        InnerText = text,
                                        Email = emailRx.Match(text).ToString(),
                                        Phone = phoneRx.Match(text).ToString()
                                    }
                                );
                            }
                        }



                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            Console.WriteLine("Done Crawling");
        }

        // these have to exist here or else WPF gets angry
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CrawlUrl_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        public static IEnumerable<HtmlNode> SelectNodes(HtmlNodeNavigator navigator, string xpath)
        {
            if (navigator == null)
                throw new ArgumentNullException("navigator");

            XPathExpression expr = navigator.Compile(xpath);
            expr.SetContext(new HtmlXsltContext());

            object eval = navigator.Evaluate(expr);
            XPathNodeIterator it = eval as XPathNodeIterator;
            if (it != null)
            {
                while (it.MoveNext())
                {
                    HtmlNodeNavigator n = it.Current as HtmlNodeNavigator;
                    if (n != null && n.CurrentNode != null)
                    {
                        yield return n.CurrentNode;
                    }
                }
            }
        }
    }
}
