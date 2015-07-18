using System;
using WebsiteRipper.Downloaders;

namespace WebsiteRipper.Tests.Fixtures
{
    [Downloader(WebTest.Scheme)]
    sealed class WebTestDownloader : Downloader
    {
        public WebTestDownloader(Uri url, int timeout, string preferredLanguages) : base(url, timeout, preferredLanguages) { }
    }
}
