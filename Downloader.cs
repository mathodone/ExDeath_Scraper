using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace ExDeath
{
    // all download logic will be moved in here eventually
    class Downloader
    {
        static Downloader()
        {
            return;
        }

        //public static void DownloadImages(string html)
        //{
        //    HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlDocument();
        //    htmlDoc.LoadHtml(html);

        //    // get all image nodes on the page
        //    List<string> imgLinks = htmlDoc.DocumentNode
        //                                .Descendants("img")
        //                                .Select(img => img.Attributes["src"].Value)
        //                                .ToList();

        //    foreach (string imgLink in imgLinks)
        //    {
        //        //
        //    }
        //}
    }
}
