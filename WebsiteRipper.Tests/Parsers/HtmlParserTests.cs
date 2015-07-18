using System;
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
            var webTest = new WebTestInfo(HtmlParser.MimeType, html);
            var expected = typeof(HtmlParser);
            var actual = WebTest.GetActualResource(webTest).Parser;
            Assert.IsType(expected, actual);
        }

        [Fact]
        public void Rip_BasicHtml_ReturnsResourceWithHtmlMimeType()
        {

            var html = "Body";
            var webTest = new WebTestInfo(HtmlParser.MimeType, html);
            var expected = HtmlParser.MimeType;
            var actual = WebTest.GetActualResource(webTest).Parser.ActualMimeType;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Rip_BasicHtml_ReturnsResourceWithRequestedUrl()
        {
            var html = "Body";
            var webTest = new WebTestInfo(HtmlParser.MimeType, html);
            var expected = webTest.Url;
            var actual = WebTest.GetActualResource(webTest).OriginalUrl;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("<html><head><title>Title</title></head><body>Body</body></html>", null)]
        [InlineData("<html><head><title>Title</title><base target=_blank></head><body>Body</body></html>", null)]
        [InlineData("<html><head><title>Title</title><base href=invalid></head><body>Body</body></html>", null)]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body</body></html>", "http://base")]
        public void Rip_Html_ReturnsResourceWithExpectedBase(string html, string expectedBase)
        {
            var webTest = new WebTestInfo(HtmlParser.MimeType, html);
            var expected = expectedBase != null ? new Uri(expectedBase) : null;
            var actual = ((HtmlParser)WebTest.GetActualResource(webTest).Parser).BaseUrl;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Rip_DuplicateBaseHtml_ThrowsDuplicateBaseHtmlElementException()
        {
            var html = "<html><head><title>Title</title><base href=http://base1><base href=http://base2></head><body>Body</body></html>";
            var webTest = new WebTestInfo(HtmlParser.MimeType, html);
            Assert.Throws<DuplicateBaseHtmlElementException>(() => WebTest.GetActualSubResources(webTest));
        }

        [Theory]
        [InlineData("")]
        [InlineData("Body")]
        [InlineData("<html><head><title>Title</title></head><body>Body</body></html>")]
        public void Rip_BasicHtml_ReturnsNoSubResources(string html)
        {
            var webTest = new WebTestInfo(HtmlParser.MimeType, html);
            var expected = WebTest.GetExpectedSubResources();
            var actual = WebTest.GetActualSubResources(webTest);
            Assert.Equal(expected, actual);
        }
    }
}
