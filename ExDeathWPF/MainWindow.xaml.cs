using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            SearchResults = new ObservableCollection<SearchResult>();

            // enable cross access to this collection elsewhere
            BindingOperations.EnableCollectionSynchronization(SearchResults, _syncLock);
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
            string url = crawlUrl.Text;
            ExDeath.Crawler crawler = new ExDeath.Crawler(url: url, usekeywords: UseKeywords, dlHtml: DlHtml, dlImage: DlImage, useSearch: UseSearch);
            await crawler.Run();
            Console.WriteLine(DlHtml);
        }

        private async void PerformCrawl()
        {

        }

        // these have to exist here or else WPF gets angry
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
