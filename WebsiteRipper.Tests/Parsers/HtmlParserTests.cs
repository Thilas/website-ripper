using System;
using System.Linq;
using WebsiteRipper.Parsers.Html;
using WebsiteRipper.Tests.Fixtures;
using Xunit;

namespace WebsiteRipper.Tests.Parsers
{
    public sealed class HtmlParserTests
    {
        const string EmptyHtml = "Empty";
        const string HtmlFormat = "<html{0}><head{1}><title>Title</title>{2}{3}</head><body{4}>Body{5}</body></html>";
        const string BaseFormat = "<base href={0}>";

        static string GetHtml(string htmlAttribute = null, string headAttribute = null, string baseElement = null, string headElement = null,
            string bodyAttribute = null, string bodyElement = null)
        {
            return string.Format(HtmlFormat, htmlAttribute, headAttribute, baseElement, headElement, bodyAttribute, bodyElement);
        }

        static string GetBaseElement(string baseUriStringWithoutScheme) { return string.Format(BaseFormat, WebTest.GetUri(baseUriStringWithoutScheme)); }

        [Fact]
        public void Rip_BasicHtml_ReturnsResourceWithHtmlParser()
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, EmptyHtml))
            {
                var expected = typeof(HtmlParser);
                var actual = WebTest.GetActualResources(webTest).Single().Parser;
                Assert.IsType(expected, actual);
            }
        }

        [Fact]
        public void Rip_BasicHtml_ReturnsResourceWithHtmlMimeType()
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, EmptyHtml))
            {
                var expected = HtmlParser.MimeType;
                var actual = WebTest.GetActualResources(webTest).Single().Parser.ActualMimeType;
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_BasicHtml_ReturnsResourceWithRequestedUri()
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, EmptyHtml))
            {
                var expected = webTest.Uri;
                var actual = WebTest.GetActualResources(webTest).Single().OriginalUri;
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("", null)]
        [InlineData("<base target=_blank>", null)]
        [InlineData("<base href=invalid>", null)]
        [InlineData("<base href={0}>", "base")]
        public void Rip_BaseHtml_ReturnsResourceWithExpectedBase(string baseElementFormat, string baseUriStringWithoutScheme)
        {
            var baseUri = baseUriStringWithoutScheme != null ? WebTest.GetUri(baseUriStringWithoutScheme) : null;
            var html = GetHtml(baseElement: string.Format(baseElementFormat, baseUri));
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = baseUri;
                var actual = ((HtmlParser)WebTest.GetActualResources(webTest).Single().Parser).BaseUri;
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_DuplicateBaseHtml_ThrowsDuplicateBaseHtmlElementException()
        {
            var html = GetHtml(baseElement: string.Format("{0}{1}", GetBaseElement("base1"), GetBaseElement("base2")));
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
        [InlineData(EmptyHtml)]
        public void Rip_EmptyHtml_ReturnsNoSubResources(string html)
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = WebTest.GetExpectedResources(webTest);
                var actual = WebTest.GetActualResources(webTest);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "<fake href=fake>Fake</fake>")]
        // Not yet supported attributes
        [InlineData(null, "<applet archive=sub>Sub</applet>")]
        [InlineData(null, "<applet codebase=sub>Sub</applet>")]
        [InlineData("<link rel=dns-prefetch href=sub>Sub</link>", null)]
        [InlineData(null, "<object archive=sub>Sub</object>")]
        [InlineData(null, "<object codebase=sub>Sub</object>")]
        [InlineData(null, "<source srcset=sub>Sub</source>")]
        public void Rip_BasicHtml_ReturnsNoSubResources(string headElement, string bodyElement)
        {
            var html = GetHtml(headElement: headElement, bodyElement: bodyElement);
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = WebTest.GetExpectedResources(webTest);
                var actual = WebTest.GetActualResources(webTest);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(null, null, null, null, "<a href=sub>Sub</a>")]
        //[InlineData(null, null, null, null, "<applet archive=sub>Sub</applet>")]
        [InlineData(null, null, null, null, "<applet code=sub>Sub</applet>")]
        //[InlineData(null, null, null, null, "<applet codebase=sub>Sub</applet>")]
        [InlineData(null, null, null, null, "<area href=sub>Sub</area>")]
        [InlineData(null, null, null, null, "<audio src=sub>Sub</audio>")]
        [InlineData(null, null, null, " background=sub", null)]
        [InlineData(null, null, null, null, "<embed src=sub>Sub</embed>")]
        [InlineData(null, null, null, null, "<frame longdesc=sub>Sub</frame>")]
        [InlineData(null, null, null, null, "<frame src=sub>Sub</frame>")]
        [InlineData(null, null, null, null, "<area href=sub>Sub</area>")]
        [InlineData(null, " profile=sub", null, null, null)]
        [InlineData(" manifest=sub", null, null, null, null)]
        [InlineData(null, null, null, null, "<iframe longdesc=sub>Sub</iframe>")]
        [InlineData(null, null, null, null, "<iframe src=sub>Sub</iframe>")]
        [InlineData(null, null, null, null, "<img longdesc=sub>Sub</img>")]
        [InlineData(null, null, null, null, "<img src=sub>Sub</img>")]
        [InlineData(null, null, null, null, "<input src=sub>Sub</input>")]
        //[InlineData(null, null, "<link rel=dns-prefetch href=sub>Sub</link>", null, null)]
        [InlineData(null, null, "<link rel=icon href=sub>Sub</link>", null, null)]
        [InlineData(null, null, "<link rel=pingback href=sub>Sub</link>", null, null)]
        [InlineData(null, null, "<link rel=prefetch href=sub>Sub</link>", null, null)]
        [InlineData(null, null, "<link rel=stylesheet href=sub>Sub</link>", null, null)]
        [InlineData(null, null, null, null, "<menuitem icon=sub>Sub</menuitem>")]
        //[InlineData(null, null, null, null, "<object archive=sub>Sub</object>")]
        [InlineData(null, null, null, null, "<object classid=sub>Sub</object>")]
        //[InlineData(null, null, null, null, "<object codebase=sub>Sub</object>")]
        [InlineData(null, null, null, null, "<object data=sub>Sub</object>")]
        [InlineData(null, null, null, null, "<script src=sub>Sub</script>")]
        [InlineData(null, null, null, null, "<source src=sub>Sub</source>")]
        //[InlineData(null, null, null, null, "<source srcset=sub>Sub</source>")]
        [InlineData(null, null, null, null, "<track src=sub>Sub</track>")]
        [InlineData(null, null, null, null, "<video poster=sub>Sub</video>")]
        [InlineData(null, null, null, null, "<video src=sub>Sub</video>")]
        public void Rip_BasicHtml_ReturnsExpectedSubResources(string htmlAttribute, string headAttribute, string headElement,
            string bodyAttribute, string bodyElement)
        {
            var html = GetHtml(htmlAttribute, headAttribute, null, headElement, bodyAttribute, bodyElement);
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                const string subUriString = "sub";
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString, HtmlParser.MimeType, EmptyHtml));
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(null, null, null, null, "<a href=sub>Sub</a>")]
        //[InlineData(null, null, null, null, "<applet archive=sub>Sub</applet>")]
        [InlineData(null, null, null, null, "<applet code=sub>Sub</applet>")]
        //[InlineData(null, null, null, null, "<applet codebase=sub>Sub</applet>")]
        [InlineData(null, null, null, null, "<area href=sub>Sub</area>")]
        [InlineData(null, null, null, null, "<audio src=sub>Sub</audio>")]
        [InlineData(null, null, null, " background=sub", null)]
        [InlineData(null, null, null, null, "<embed src=sub>Sub</embed>")]
        [InlineData(null, null, null, null, "<frame longdesc=sub>Sub</frame>")]
        [InlineData(null, null, null, null, "<frame src=sub>Sub</frame>")]
        [InlineData(null, null, null, null, "<area href=sub>Sub</area>")]
        [InlineData(null, " profile=sub", null, null, null)]
        [InlineData(null, null, null, null, "<iframe longdesc=sub>Sub</iframe>")]
        [InlineData(null, null, null, null, "<iframe src=sub>Sub</iframe>")]
        [InlineData(null, null, null, null, "<img longdesc=sub>Sub</img>")]
        [InlineData(null, null, null, null, "<img src=sub>Sub</img>")]
        [InlineData(null, null, null, null, "<input src=sub>Sub</input>")]
        //[InlineData(null, null, "<link rel=dns-prefetch href=sub>Sub</link>", null, null)]
        [InlineData(null, null, "<link rel=icon href=sub>Sub</link>", null, null)]
        [InlineData(null, null, "<link rel=pingback href=sub>Sub</link>", null, null)]
        [InlineData(null, null, "<link rel=prefetch href=sub>Sub</link>", null, null)]
        [InlineData(null, null, "<link rel=stylesheet href=sub>Sub</link>", null, null)]
        [InlineData(null, null, null, null, "<menuitem icon=sub>Sub</menuitem>")]
        //[InlineData(null, null, null, null, "<object archive=sub>Sub</object>")]
        [InlineData(null, null, null, null, "<object classid=sub>Sub</object>")]
        //[InlineData(null, null, null, null, "<object codebase=sub>Sub</object>")]
        [InlineData(null, null, null, null, "<object data=sub>Sub</object>")]
        [InlineData(null, null, null, null, "<script src=sub>Sub</script>")]
        [InlineData(null, null, null, null, "<source src=sub>Sub</source>")]
        //[InlineData(null, null, null, null, "<source srcset=sub>Sub</source>")]
        [InlineData(null, null, null, null, "<track src=sub>Sub</track>")]
        [InlineData(null, null, null, null, "<video poster=sub>Sub</video>")]
        [InlineData(null, null, null, null, "<video src=sub>Sub</video>")]
        public void Rip_BasicHtml_ReturnsExpectedRebasedSubResources(string htmlAttribute, string headAttribute, string headElement,
            string bodyAttribute, string bodyElement)
        {
            const string baseUriStringWithoutScheme = "base";
            var html = GetHtml(htmlAttribute, headAttribute, GetBaseElement(baseUriStringWithoutScheme), headElement, bodyAttribute, bodyElement);
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                const string subUriString = "sub";
                var subUri = new Uri(WebTest.GetUri(baseUriStringWithoutScheme), subUriString);
                var expected = WebTest.GetExpectedResources(webTest, subUri);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUri, HtmlParser.MimeType, EmptyHtml));
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(" manifest=sub", null, null, null, null)]
        public void Rip_BasicHtml_ReturnsExpectedNotRebasedSubResources(string htmlAttribute, string headAttribute, string headElement,
            string bodyAttribute, string bodyElement)
        {
            var html = GetHtml(htmlAttribute, headAttribute, null, headElement, bodyAttribute, bodyElement);
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                const string subUriString = "sub";
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString, HtmlParser.MimeType, EmptyHtml));
                Assert.Equal(expected, actual);
            }
        }
    }
}
