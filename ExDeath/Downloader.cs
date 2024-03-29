﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using HtmlAgilityPack;
using System.Collections.Concurrent;


namespace ExDeath
{
    // all download logic will be moved in here eventually
    class Downloader
    {
        //TODO: empty dict after each crawl or else all memory will be used up
        static ConcurrentDictionary<Uri, string> seen = new ConcurrentDictionary<Uri, string>();
        static Downloader()
        {
            return;
        }

        public static async Task DownloadImages(string html, Uri url, string downloadsDirectory)
        {
            // parse HTML
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            // get all image nodes on the page
            List<string> imgLinks = htmlDoc.DocumentNode
                                        .Descendants("img")
                                        .Select(img => img.Attributes["src"].Value)
                                        .ToList();

            string directory = $"{downloadsDirectory}/{url.AbsolutePath.Substring(1)}";
            //directory = directory.Substring(0, directory.LastIndexOf('/'));
            Directory.CreateDirectory(directory);
            WebClient client = new WebClient();

            //loop through all imgs and save to directory
            foreach (string imgLink in imgLinks)
            {
                string imgName = imgLink.Substring(imgLink.LastIndexOf('/') + 1);
                Uri absoluteUrl = new Uri(url, imgLink);
                // if we see the same img, then just make a new copy of it. faster than DLing it again
                if (!seen.ContainsKey(absoluteUrl))
                {
                    await client.DownloadFileTaskAsync(absoluteUrl, $"{directory}/{imgName}");
                    seen.TryAdd(absoluteUrl, $"{directory}/{imgName}");
                }
                else
                {
                    File.Copy(seen[absoluteUrl], $"{directory}/{imgName}");
                }
            }
        }

        public static async Task DownloadHtml(string source, Uri url, string downloadsDirectory)
        {
            // create save directory 
            string directory = $"{downloadsDirectory}/{url.AbsolutePath.Substring(1)}";
            //directory = directory.Substring(0, directory.LastIndexOf('/'));

            Directory.CreateDirectory(directory);
            string filepath = $"{directory}/html.txt";

            // save html to file
            using (StreamWriter outputFile = new StreamWriter(filepath))
            {
                await outputFile.WriteAsync(source);
                await outputFile.FlushAsync();
            }
        }
    }
}
