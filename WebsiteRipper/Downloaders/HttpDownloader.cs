using System;
using System.Net;

namespace WebsiteRipper.Downloaders
{
    [Downloader("http")]
    [Downloader("https")]
    sealed class HttpDownloader : Downloader
    {
        public static string UserAgent = string.Format("WebsiteRipper/{0}", typeof(HttpDownloader).Assembly.GetName().Version);

        protected internal override DateTime LastModified
        {
            get
            {
                try { return ((HttpWebResponse)WebResponse).LastModified; }
                catch { return base.LastModified; }
            }
        }

        public HttpDownloader(DownloaderArgs downloaderArgs)
            : base(downloaderArgs)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest;
            httpWebRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, downloaderArgs.PreferredLanguages);
            httpWebRequest.UserAgent = UserAgent;
        }
    }
}
