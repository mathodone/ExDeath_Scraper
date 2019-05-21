using System;
using System.Collections.Generic;
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

        public bool dlHtml { get; set; }
        public bool dlImage { get; set; }
        public bool useKeywords { get; set; }
        public bool useSearch { get; set; }



        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private async void Button_Search(object sender, RoutedEventArgs e)
        {
            string url = crawlUrl.Text;
            //ExDeath.Crawler crawler = new ExDeath.Crawler(url: url, usekeywords: useKeywords, dlHtml: dlHtml, dlImage: dlImage, useSearch: useSearch);
            //await crawler.Run();
            Console.WriteLine(dlHtml);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
