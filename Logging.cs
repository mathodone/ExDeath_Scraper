using System;
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
            path = "../../logs/";
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

        public static void QueuedUrl(string url)
        {
            WriteToLog("Queued Link", url);
        }

        public static void ProcessingNewUrl(string url)
        {
            WriteToLog("Processing new page", url);
        }

        public static void ProcessedQueue()
        {
            WriteToLog("Finished Processing Queue", "");
        }

        public static void ProcessedUrl(string url)
        {
            WriteToLog("Finished Processing Link", url);
        }

        public static void FailedUrl(string url)
        {
            WriteToLog("Failed to process url", url);
        }

        public static void FailedQueue(string url)
        {
            WriteToLog("Failed to generate queue", url);
        }

        public static void DownloadedImage(string filename)
        {
            WriteToLog("Downloaded Image", filename);
        }

        public static void DownloadedFile(string filename)
        {
            WriteToLog("Downloaded", filename);
        }

        public static void CrawlFinished(string url)
        {
            WriteToLog("Crawl Finished", url);
        }
    }
}
