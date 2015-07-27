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
            const string html = "Body";
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
            const string html = "Body";
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
            const string html = "Body";
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
        public void Rip_Html_ReturnsResourceWithExpectedBase(string html, string baseUriString)
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = baseUriString != null ? new Uri(baseUriString) : null;
                var actual = ((HtmlParser)WebTest.GetActualResources(webTest).Single().Parser).BaseUri;
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_DuplicateBaseHtml_ThrowsDuplicateBaseHtmlElementException()
        {
            const string html = "<html><head><title>Title</title><base href=http://base1><base href=http://base2></head><body>Body</body></html>";
            Assert.Throws<DuplicateBaseHtmlElementException>(() =>
            {
                using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
                {
                    WebTest.GetActualResources(webTest);
                }
            });
        }

        [Theory]
        [InlineData("")]
        [InlineData("Body")]
        [InlineData("<html><head><title>Title</title></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<fake href=fake>Fake</fake></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<applet archive=sub>Sub</applet></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<applet codebase=sub>Sub</applet></body></html>")]
        [InlineData("<html><head><title>Title</title><link rel=dns-prefetch href=sub>Sub</link></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<object archive=sub>Sub</object></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<object codebase=sub>Sub</object></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<source srcset=sub>Sub</source></body></html>")]
        public void Rip_BasicHtml_ReturnsNoSubResources(string html)
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = WebTest.GetExpectedResources(webTest);
                var actual = WebTest.GetActualResources(webTest);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("<html><head><title>Title</title></head><body>Body<a href=sub>Sub</a></body></html>")]
        //[InlineData("<html><head><title>Title</title></head><body>Body<applet archive=sub>Sub</applet></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<applet code=sub>Sub</applet></body></html>")]
        //[InlineData("<html><head><title>Title</title></head><body>Body<applet codebase=sub>Sub</applet></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<area href=sub>Sub</area></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<audio src=sub>Sub</audio></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body background=sub>Body</body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<embed src=sub>Sub</embed></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<frame longdesc=sub>Sub</frame></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<frame src=sub>Sub</frame></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<area href=sub>Sub</area></body></html>")]
        [InlineData("<html><head profile=sub><title>Title</title></head><body>Body</body></html>")]
        [InlineData("<html manifest=sub><head><title>Title</title></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<iframe longdesc=sub>Sub</iframe></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<iframe src=sub>Sub</iframe></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<img longdesc=sub>Sub</img></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<img src=sub>Sub</img></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<input src=sub>Sub</input></body></html>")]
        //[InlineData("<html><head><title>Title</title><link rel=dns-prefetch href=sub>Sub</link></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title><link rel=icon href=sub>Sub</link></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title><link rel=pingback href=sub>Sub</link></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title><link rel=prefetch href=sub>Sub</link></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title><link rel=stylesheet href=sub>Sub</link></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<menuitem icon=sub>Sub</menuitem></body></html>")]
        //[InlineData("<html><head><title>Title</title></head><body>Body<object archive=sub>Sub</object></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<object classid=sub>Sub</object></body></html>")]
        //[InlineData("<html><head><title>Title</title></head><body>Body<object codebase=sub>Sub</object></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<object data=sub>Sub</object></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<script src=sub>Sub</script></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<source src=sub>Sub</source></body></html>")]
        //[InlineData("<html><head><title>Title</title></head><body>Body<source srcset=sub>Sub</source></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<track src=sub>Sub</track></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<video poster=sub>Sub</video></body></html>")]
        [InlineData("<html><head><title>Title</title></head><body>Body<video src=sub>Sub</video></body></html>")]
        public void Rip_BasicHtml_ReturnsExpectedSubResources(string html)
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                const string subUriString = "sub";
                const string subHtml = "Body";
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString, HtmlParser.MimeType, subHtml));
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<a href=sub>Sub</a></body></html>")]
        //[InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<applet archive=sub>Sub</applet></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<applet code=sub>Sub</applet></body></html>")]
        //[InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<applet codebase=sub>Sub</applet></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<area href=sub>Sub</area></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<audio src=sub>Sub</audio></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body background=sub>Body</body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<embed src=sub>Sub</embed></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<frame longdesc=sub>Sub</frame></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<frame src=sub>Sub</frame></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<area href=sub>Sub</area></body></html>")]
        [InlineData("<html><head profile=sub><title>Title</title><base href=http://base></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<iframe longdesc=sub>Sub</iframe></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<iframe src=sub>Sub</iframe></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<img longdesc=sub>Sub</img></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<img src=sub>Sub</img></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<input src=sub>Sub</input></body></html>")]
        //[InlineData("<html><head><title>Title</title><base href=http://base><link rel=dns-prefetch href=sub>Sub</link></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base><link rel=icon href=sub>Sub</link></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base><link rel=pingback href=sub>Sub</link></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base><link rel=prefetch href=sub>Sub</link></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base><link rel=stylesheet href=sub>Sub</link></head><body>Body</body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<menuitem icon=sub>Sub</menuitem></body></html>")]
        //[InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<object archive=sub>Sub</object></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<object classid=sub>Sub</object></body></html>")]
        //[InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<object codebase=sub>Sub</object></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<object data=sub>Sub</object></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<script src=sub>Sub</script></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<source src=sub>Sub</source></body></html>")]
        //[InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<source srcset=sub>Sub</source></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<track src=sub>Sub</track></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<video poster=sub>Sub</video></body></html>")]
        [InlineData("<html><head><title>Title</title><base href=http://base></head><body>Body<video src=sub>Sub</video></body></html>")]
        public void Rip_BasicHtml_ReturnsExpectedRebasedSubResources(string html)
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                const string baseUriString = "http://base";
                const string subUriString = "sub";
                var subUri = new Uri(new Uri(baseUriString), subUriString);
                const string subHtml = "Body";
                var expected = WebTest.GetExpectedResources(webTest, subUri);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUri, HtmlParser.MimeType, subHtml));
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("<html manifest=sub><head><title>Title</title><base href=http://base></head><body>Body</body></html>")]
        public void Rip_BasicHtml_ReturnsExpectedNotRebasedSubResources(string html)
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                const string subUriString = "sub";
                const string subHtml = "Body";
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString, HtmlParser.MimeType, subHtml));
                Assert.Equal(expected, actual);
            }
        }
    }
}
