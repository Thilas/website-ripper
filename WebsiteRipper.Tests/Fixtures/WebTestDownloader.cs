using System;
using WebsiteRipper.Downloaders;

namespace WebsiteRipper.Tests.Fixtures
{
    [Downloader(WebTest.Scheme)]
    sealed class WebTestDownloader : Downloader
    {
        public WebTestDownloader(Uri uri, int timeout, string preferredLanguages) : base(uri, timeout, preferredLanguages) { }
    }
}
