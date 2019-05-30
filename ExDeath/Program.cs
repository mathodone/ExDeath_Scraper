using System;
using System.Collections.Generic;
using System.Linq;
using ExcelDataReader;
using System.IO;
using System.Data;

namespace ExDeath
{
    class Program
    {
        static void Main(string[] args) => TestCrawler();

        //static void School()
        //{
        //    string inputdata = @"C:\Users\mathmorales\Documents\freelance\SchoolScrape\contacts.xlsx";

        //    using (var stream = File.Open(inputdata, FileMode.Open, FileAccess.Read))
        //    {
        //        using (var reader = ExcelReaderFactory.CreateReader(stream))
        //        {
        //            //do
        //            //{
        //            //    while (reader.Read())
        //            //    {

        //            //    }
        //            //} while (reader.NextResult());


        //            // The result of each spreadsheet is in result.Tables
        //            var result = reader;
        //        }
        //    }

        //}

        static void TestCrawler()
        {
            Console.WriteLine("BEGIN");
            Crawler crawler = new Crawler("http://books.toscrape.com/", depth: 2, maxConnections: 2);
            //Crawler crawler = new Crawler("http://www.chinavitae.org/biography_browse.php?l=A");
            //Crawler crawler = new Crawler("https://d-fens.ch/2014/04/12/httpclient-and-how-to-use-headers-content-type-and-postasync/", useSearch: true, searchTerm: "curriculum");
            //crawler.LoadKeywords(@"../../keywords/school_words.txt");
            //crawler.Run();
            Console.WriteLine("DONE");
        }
    }
}
    