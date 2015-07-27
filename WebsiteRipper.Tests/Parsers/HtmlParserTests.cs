using System;
using System.Linq;
using WebsiteRipper.Parsers.Html;
using WebsiteRipper.Tests.Fixtures;
using Xunit;

namespace WebsiteRipper.Tests.Parsers
{
    public sealed class HtmlParserTests
    {
        const string EmptyHtml = "";
        const string HtmlFormat = "<html{0}><head{1}><title>Title</title>{2}{3}</head><body{4}>Body{5}</body></html>";
        const string BaseFormat = "<base href={0}>";

        static string GetHtml(string htmlAttribute = null, string headAttribute = null, string baseElement = null, string headElement = null,
            string bodyAttribute = null, string bodyElement = null)
        {
            return string.Format(HtmlFormat, htmlAttribute, headAttribute, baseElement, headElement, bodyAttribute, bodyElement);
        }

        static string GetBaseElement(string baseUriStringWithoutScheme) { return string.Format(BaseFormat, WebTest.GetUri(baseUriStringWithoutScheme)); }

        [Theory]
        [InlineData(null)]
        [InlineData(EmptyHtml)]
        [InlineData("\n")]
        [InlineData(" ")]
        [InlineData("<!-- comment -->")]
        public void Rip_EmptyHtml_ReturnsSingleResource(string html)
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = WebTest.GetExpectedResources(webTest);
                var actual = WebTest.GetActualResources(webTest);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_BasicHtml_ReturnsHtmlParser()
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, EmptyHtml))
            {
                var expected = typeof(HtmlParser);
                var actual = WebTest.GetActualResources(webTest).Single().Parser;
                Assert.IsType(expected, actual);
            }
        }

        [Fact]
        public void Rip_BasicHtml_ReturnsHtmlMimeType()
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, EmptyHtml))
            {
                var expected = HtmlParser.MimeType;
                var actual = WebTest.GetActualResources(webTest).Single().Parser.ActualMimeType;
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_BasicHtml_ReturnsExpectedUri()
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, EmptyHtml))
            {
                var expected = webTest.Uri;
                var actual = WebTest.GetActualResources(webTest).Single().OriginalUri;
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "<fake href=fake>Fake</fake>")]
        [InlineData("<link rel=dns-prefetch href=sub>", null)]
        // Not yet supported attributes
        [InlineData(null, "<applet archive=sub>Sub</applet>")]
        [InlineData(null, "<applet codebase=sub>Sub</applet>")]
        [InlineData(null, "<object archive=sub>Sub</object>")]
        [InlineData(null, "<object codebase=sub>Sub</object>")]
        [InlineData(null, "<source srcset=sub>Sub</source>")]
        public void Rip_BasicHtmlWithNoReferences_ReturnsSingleResource(string headElement, string bodyElement)
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
        [InlineData(null, " profile=sub", null, null, null)]
        [InlineData(" manifest=sub", null, null, null, null)]
        [InlineData(null, null, null, null, "<iframe longdesc=sub>Sub</iframe>")]
        [InlineData(null, null, null, null, "<iframe src=sub>Sub</iframe>")]
        [InlineData(null, null, null, null, "<img longdesc=sub>Sub</img>")]
        [InlineData(null, null, null, null, "<img src=sub>Sub</img>")]
        [InlineData(null, null, null, null, "<input src=sub>Sub</input>")]
        [InlineData(null, null, "<link rel=icon href=sub>", null, null)]
        [InlineData(null, null, "<link rel=pingback href=sub>", null, null)]
        [InlineData(null, null, "<link rel=prefetch href=sub>", null, null)]
        [InlineData(null, null, "<link rel=stylesheet href=sub>", null, null)]
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
        public void Rip_BasicHtmlWithReference_ReturnsExpectedResources(string htmlAttribute, string headAttribute, string headElement,
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

        [Fact]
        public void Rip_ComplexHtml_ReturnsExpectedResources()
        {
            const string html = @"<!-- comment -->
<html aa=aa manifest=subHtmlManifest>
    <head profile=subHeadProfile>
        <title>Title</title>
        <link rel=dns-prefetch href=subLinkDnsPrefetchHref>
        <link rel=icon href=subLinkIconHref>
        <link rel=pingback href=subLinkPingBackHref>
        <link rel=prefetch href=subLinkPrefetchHref>
        <link rel=stylesheet href=subLinkStyleSheetHref>
    </head>
    <body background=subBodyBackground>
        Body<br>
        A: <a href=subAHref>Sub</a><br>
        Applet: <applet archive=subAppletArchive code=subAppletCode codebase=subAppletCodeBase>Sub</applet><br>
        Area: <map><area href=subAreaHref>Sub</map><br>
        Audio: <audio src=subAudioSrc>Sub</audio><br>
        Embed: <embed src=subEmbedSrc>Sub</embed><br>
        Frame: <frameset><frame longdesc=subFrameLongDesc src=subFrameSrc>Sub</frameset><br>
        IFrame: <iframe longdesc=subIFrameLongDesc src=subIFrameSrc>Sub</iframe><br>
        Img: <img longdesc=subImgLongDesc src=subImgSrc>Sub</img><br>
        Input: <input src=subInputSrc>Sub</input><br>
        MenuItem: <menu><menuitem icon=subMenuItemIcon>Sub</menu><br>
        Object: <object archive=subObjectArchive classid=subObjectClassId codebase=subObjectCodeBase data=subObjectData>Sub</object><br>
        Script: <script src=subScriptSrc>Sub</script><br>
        Source: <audio><source src=subSourceSrc srcsetSourceSrcSet=sub>Sub</audio><br>
        Track: <video><track src=subTrackSrc>Sub</video><br>
        Video: <video poster=subVideoPoster src=subVideoSrc>Sub</video><br>
    </body>
</html>
";
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var subUriStrings = new[]
                {
                    "subHtmlManifest", "subHeadProfile", "subLinkIconHref", "subLinkPingBackHref", "subLinkPrefetchHref",
                    "subLinkStyleSheetHref", "subBodyBackground", "subAHref", "subAppletCode", "subAreaHref",
                    "subAudioSrc", "subEmbedSrc", "subFrameLongDesc", "subFrameSrc", "subIFrameLongDesc", "subIFrameSrc",
                    "subImgLongDesc", "subImgSrc", "subInputSrc", "subMenuItemIcon", "subObjectClassId", "subObjectData",
                    "subScriptSrc", "subSourceSrc", "subTrackSrc", "subVideoPoster", "subVideoSrc"
                };
                var expected = WebTest.GetExpectedResources(webTest, subUriStrings);
                var actual = WebTest.GetActualResources(webTest,
                    subUriStrings.Select(subUriString => new WebTestInfo(webTest, subUriString, HtmlParser.MimeType, EmptyHtml)).ToArray());
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("", null)]
        [InlineData("<base target=_blank>", null)]
        [InlineData("<base href=invalid>", null)]
        [InlineData("<base href={0}>", "base")]
        public void Rip_BasicHtmlWithBase_ReturnsExpectedBaseUri(string baseElementFormat, string baseUriStringWithoutScheme)
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
        public void Rip_BasicHtmlWithDuplicateBases_ThrowsDuplicateBasesException()
        {
            var html = GetHtml(baseElement: string.Format("{0}{1}", GetBaseElement("base1"), GetBaseElement("base2")));
            Assert.Throws<DuplicateBasesException>(() =>
            {
                using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
                {
                    WebTest.GetActualResources(webTest);
                }
            });
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
        [InlineData(null, " profile=sub", null, null, null)]
        [InlineData(null, null, null, null, "<iframe longdesc=sub>Sub</iframe>")]
        [InlineData(null, null, null, null, "<iframe src=sub>Sub</iframe>")]
        [InlineData(null, null, null, null, "<img longdesc=sub>Sub</img>")]
        [InlineData(null, null, null, null, "<img src=sub>Sub</img>")]
        [InlineData(null, null, null, null, "<input src=sub>Sub</input>")]
        [InlineData(null, null, "<link rel=icon href=sub>", null, null)]
        [InlineData(null, null, "<link rel=pingback href=sub>", null, null)]
        [InlineData(null, null, "<link rel=prefetch href=sub>", null, null)]
        [InlineData(null, null, "<link rel=stylesheet href=sub>", null, null)]
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
        public void Rip_BasicHtmlWithBaseSensitiveReference_ReturnsRebasedResources(string htmlAttribute, string headAttribute, string headElement,
            string bodyAttribute, string bodyElement)
        {
            const string baseUriStringWithoutScheme = "base";
            var html = GetHtml(htmlAttribute, headAttribute, GetBaseElement(baseUriStringWithoutScheme), headElement, bodyAttribute, bodyElement);
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var subUri = new Uri(WebTest.GetUri(baseUriStringWithoutScheme), "sub");
                var expected = WebTest.GetExpectedResources(webTest, subUri);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUri, HtmlParser.MimeType, EmptyHtml));
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(" manifest=sub", null, null, null, null)]
        public void Rip_BasicHtmlWithBaseInsensitiveReference_ReturnsNotRebasedResources(string htmlAttribute, string headAttribute, string headElement,
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
