using WebsiteRipper.Downloaders;

namespace WebsiteRipper.Tests.Fixtures
{
    [Downloader(WebTest.Scheme)]
    sealed class WebTestDownloader : Downloader
    {
        public WebTestDownloader(DownloaderArgs downloaderArgs) : base(downloaderArgs) { }
    }
}
