using System;
using System.Linq;
using WebsiteRipper.Parsers.Html;
using WebsiteRipper.Tests.Fixtures;
using Xunit;

namespace WebsiteRipper.Tests.Parsers
{
    public sealed class HtmlParserTests
    {
        [Fact]
        public void Rip_BasicHtml_ReturnsResourceWithHtmlParser()
        {
            var html = "Body";
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = typeof(HtmlParser);
                var actual = WebTest.GetActualResources(webTest).Single().Parser;
                Assert.IsType(expected, actual);
            }
        }

        [Fact]
        public void Rip_BasicHtml_ReturnsResourceWithHtmlMimeType()
        {

            var html = "Body";
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = HtmlParser.MimeType;
                var actual = WebTest.GetActualResources(webTest).Single().Parser.ActualMimeType;
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_BasicHtml_ReturnsResourceWithRequestedUri()
        {
            var html = "Body";
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = webTest.Uri;
                var actual = WebTest.GetActualResources(webTest).Single().OriginalUri;
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("<html><head><title>Title</title></head><body>Body</body></html>", null)]
        [InlineData("<html><head><title>Title</title><base target=_blank></head><body>Body</body></html>", null)]
        [InlineData("<html><head><title>Title</title><base href=invalid></head><body>Body</body></html>", null)]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body</body></html>", "http://base")]
        public void Rip_Html_ReturnsResourceWithExpectedBase(string html, string expectedBase)
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = expectedBase != null ? new Uri(expectedBase) : null;
                var actual = ((HtmlParser)WebTest.GetActualResources(webTest).Single().Parser).BaseUri;
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_DuplicateBaseHtml_ThrowsDuplicateBaseHtmlElementException()
        {
            var html = "<html><head><title>Title</title><base href=http://base1><base href=http://base2></head><body>Body</body></html>";
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                Assert.Throws<DuplicateBaseHtmlElementException>(() => WebTest.GetActualResources(webTest));
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("Body")]
        [InlineData("<html><head><title>Title</title></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<fake href=fake>Fake</fake></body></html>")]
        public void Rip_BasicHtml_ReturnsNoSubResources(string html)
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = WebTest.GetExpectedResources(webTest);
                var actual = WebTest.GetActualResources(webTest);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_BasicHtml_ReturnsExpectedSubResources()
        {
            var html = "<html><head><title>Title</title></head><body>Body<a href=a>A</a></body></html>";
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var subUri = new Uri(webTest.Uri, "a");
                var expected = WebTest.GetExpectedResources(webTest, subUri);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, "a", HtmlParser.MimeType, "Body"));
                Assert.Equal(expected, actual);
            }
        }
    }
}
