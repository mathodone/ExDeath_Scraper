using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ExDeath
{
    public static class Logging
    {
        private static string file { get; set; }
        private static string path { get; set; }

        static Logging()
        {
            file = "crawl_log.txt";
            path = "../../";
        }

        public static void WriteToLog(string entry, string url = "")
        {
            Console.WriteLine("[{0}]: {1}: {2}", DateTime.Now, entry, url);

            lock (file)
            {
                string line = String.Format("{0,-25}{1,-40}{2}", "[" + DateTime.Now + "]", entry, url);
                File.AppendAllText(path + file, line + Environment.NewLine);
            }
        }

        public static void StartingCrawl(string url)
        {
            WriteToLog("Starting Crawl", url);
        }

        public static void GeneratedQueue(string url)
        {
            WriteToLog("Generated Queue", url);
        }

        public static void QueuedLink(string url)
        {
            WriteToLog("Queued Link", url);
        }

        public static void ProcessedQueue()
        {
            WriteToLog("Finished Processing Queue", "");
        }

        public static void ProcessedLink(string url)
        {
            WriteToLog("Finished Processing Link", url);
        }

        public static void FoundURL(string url)
        {
            WriteToLog("Grabbed URL from page", url);
        }

        public static void LoadingNewPage(string url)
        {
            WriteToLog("Loading new page", url);
        }

        public static void LoadSuccess(string url)
        {
            WriteToLog("Page load successful", url);
        }

        public static void SkippedThisQueuedURL(string url)
        {
            WriteToLog("Skipping...URL already queued", url);
        }

        public static void SkippedThisExcludedURL(string url)
        {
            WriteToLog("Skipping...URL domain is excluded", url);
        }

        public static void SkippedThisExcludedFileType(string url)
        {
            WriteToLog("Skipping...file type is excluded", url);
        }

        public static void EngueuedURL(string url)
        {
            WriteToLog("Queuing", url);
        }

        public static void DownloadedFile(string filename)
        {
            WriteToLog("Downloaded", filename);
        }
    }
}
