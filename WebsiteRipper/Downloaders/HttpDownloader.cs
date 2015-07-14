using System;
using System.Net;

namespace WebsiteRipper.Downloaders
{
    [Downloader("http")]
    [Downloader("https")]
    internal sealed class HttpDownloader : Downloader
    {
        protected internal override DateTime LastModified { get { return ((HttpWebResponse)WebResponse).LastModified; } }

        public HttpDownloader(Uri url, int timeout, string preferredLanguages)
            : base(url, timeout, preferredLanguages)
        {
            WebRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, preferredLanguages);
        }
    }
}
