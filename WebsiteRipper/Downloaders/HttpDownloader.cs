using System;
using System.Net;

namespace WebsiteRipper.Downloaders
{
    [Downloader("http")]
    [Downloader("https")]
    sealed class HttpDownloader : Downloader
    {
        protected internal override DateTime LastModified
        {
            get
            {
                try { return ((HttpWebResponse)WebResponse).LastModified; }
                catch { return base.LastModified; }
            }
        }

        public HttpDownloader(Uri uri, int timeout, string preferredLanguages)
            : base(uri, timeout, preferredLanguages)
        {
            WebRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, preferredLanguages);
        }
    }
}
