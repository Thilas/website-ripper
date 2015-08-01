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

        static string GetHtml(string htmlAttributes = null, string headAttributes = null, string baseElements = null, string headElements = null,
            string bodyAttributes = null, string bodyElements = null)
        {
            return string.Format(HtmlFormat, htmlAttributes, headAttributes, baseElements, headElements, bodyAttributes, bodyElements);
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
        public void Rip_EmptyHtml_ReturnsHtmlParser()
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, EmptyHtml))
            {
                var expected = typeof(HtmlParser);
                var actual = WebTest.GetActualResources(webTest).Single().Parser;
                Assert.IsType(expected, actual);
            }
        }

        [Fact]
        public void Rip_EmptyHtml_ReturnsHtmlMimeType()
        {
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, EmptyHtml))
            {
                var expected = HtmlParser.MimeType;
                var actual = WebTest.GetActualResources(webTest).Single().Parser.ActualMimeType;
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_EmptyHtml_ReturnsExpectedUri()
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
        [InlineData(null, "<element attribute=value>Text</element>")]
        [InlineData("<link rel=dns-prefetch href=value>Text</link>", null)]
        // Not yet supported
        [InlineData(null, "<applet archive=value>Text</applet>")]
        [InlineData(null, "<applet codebase=value>Text</applet>")]
        [InlineData(null, "<object archive=value>Text</object>")]
        [InlineData(null, "<object codebase=value>Text</object>")]
        [InlineData(null, "<source srcset=value>Text</source>")]
        public void Rip_BasicHtmlWithNoReferences_ReturnsSingleResource(string headElements, string bodyElements)
        {
            var html = GetHtml(headElements: headElements, bodyElements: bodyElements);
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = WebTest.GetExpectedResources(webTest);
                var actual = WebTest.GetActualResources(webTest);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("", "", "", "", "<a href={0}>Text</a>")]
        //[InlineData("", "", "", "", "<applet archive={0}>Text</applet>")]
        [InlineData("", "", "", "", "<applet code={0}>Text</applet>")]
        //[InlineData("", "", "", "", "<applet codebase={0}>Text</applet>")]
        [InlineData("", "", "", "", "<area href={0}>Text</area>")]
        [InlineData("", "", "", "", "<audio src={0}>Text</audio>")]
        [InlineData("", "", "", " background={0}", "")]
        [InlineData("", "", "", "", "<embed src={0}>Text</embed>")]
        [InlineData("", "", "", "", "<frame longdesc={0}>Text</frame>")]
        [InlineData("", "", "", "", "<frame src={0}>Text</frame>")]
        [InlineData("", " profile={0}", "", "", "")]
        [InlineData(" manifest={0}", "", "", "", "")]
        [InlineData("", "", "", "", "<iframe longdesc={0}>Text</iframe>")]
        [InlineData("", "", "", "", "<iframe src={0}>Text</iframe>")]
        [InlineData("", "", "", "", "<img longdesc={0}>Text</img>")]
        [InlineData("", "", "", "", "<img src={0}>Text</img>")]
        [InlineData("", "", "", "", "<input src={0}>Text</input>")]
        [InlineData("", "", "<link rel=icon href={0}>Text</link>", "", "")]
        [InlineData("", "", "<link rel=pingback href={0}>Text</link>", "", "")]
        [InlineData("", "", "<link rel=prefetch href={0}>Text</link>", "", "")]
        [InlineData("", "", "<link rel=stylesheet href={0}>Text</link>", "", "")]
        [InlineData("", "", "", "", "<menuitem icon={0}>Text</menuitem>")]
        //[InlineData("", "", "", "", "<object archive={0}>Text</object>")]
        [InlineData("", "", "", "", "<object classid={0}>Text</object>")]
        //[InlineData("", "", "", "", "<object codebase={0}>Text</object>")]
        [InlineData("", "", "", "", "<object data={0}>Text</object>")]
        [InlineData("", "", "", "", "<script src={0}>Text</script>")]
        [InlineData("", "", "", "", "<source src={0}>Text</source>")]
        //[InlineData("", "", "", "", "<source srcset={0}>Text</source>")]
        [InlineData("", "", "", "", "<track src={0}>Text</track>")]
        [InlineData("", "", "", "", "<video poster={0}>Text</video>")]
        [InlineData("", "", "", "", "<video src={0}>Text</video>")]
        public void Rip_BasicHtmlWithReference_ReturnsExpectedResources(string htmlAttributesFormat,
            string headAttributesFormat, string headElementsFormat,
            string bodyAttributesFormat, string bodyElementsFormat)
        {
            const string subUriString = "uri";
            var html = GetHtml(string.Format(htmlAttributesFormat, subUriString), string.Format(headAttributesFormat, subUriString),
                null, string.Format(headElementsFormat, subUriString),
                string.Format(bodyAttributesFormat, subUriString), string.Format(bodyElementsFormat, subUriString));
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString, HtmlParser.MimeType, EmptyHtml));
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_BasicHtmlWithEscapedReference_ReturnsExpectedResources()
        {
            const string subUriString = "uri<&\">";
            var html = GetHtml(bodyElements: "<a href=uri&lt;&amp;&quot;&gt;>Text</a>");
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString, HtmlParser.MimeType, EmptyHtml));
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_ComplexHtml_ReturnsExpectedResources()
        {
            var subUriStrings = new[]
            {
                "htmlManifestUri", "headProfileUri", "linkIconHrefUri", "linkPingBackHrefUri", "linkPrefetchHrefUri",
                "linkStyleSheetHrefUri", "bodyBackgroundUri", "aHrefUri", "appletCodeUri", "areaHrefUri", "audioSrcUri",
                "embedSrcUri", "frameLongDescUri", "frameSrcUri", "iframeLongDescUri", "iframeSrcUri", "imgLongDescUri",
                "imgSrcUri", "inputSrcUri", "menuItemIconUri", "objectClassIdUri", "objectDataUri", "scriptSrcUri",
                "sourceSrcUri", "trackSrcUri", "videoPosterUri", "videoSrcUri"
            };
            var html = string.Format(@"<!-- comment -->
<html manifest={0}>
    <head profile={1}>
        <title>Title</title>
        <link rel=dns-prefetch href=value>Text</link>
        <link rel=icon href={2}>Text</link>
        <link rel=pingback href={3}>Text</link>
        <link rel=prefetch href={4}>Text</link>
        <link rel=stylesheet href={5}>Text</link>
    </head>
    <body background={6}>
        Body<br>
        A: <a href={7}>Text</a><br>
        Applet: <applet archive=value code={8} codebase=value>Text</applet><br>
        Area: <map><area href={9}>Text</area></map><br>
        Audio: <audio src={10}>Text</audio><br>
        Element: <element attribute=value>Text</element>
        Embed: <embed src={11}>Text</embed><br>
        Frame: <frameset><frame longdesc={12} src={13}>Text</frame></frameset><br>
        Iframe: <iframe longdesc={14} src={15}>Text</iframe><br>
        Img: <img longdesc={16} src={17}>Text</img><br>
        Input: <input src={18}>Text</input><br>
        MenuItem: <menu><menuitem icon={19}>Text</menuitem></menu><br>
        Object: <object archive=value classid={20} codebase=value data={21}>Text</object><br>
        Script: <script src={22}>Text</script><br>
        Source: <audio><source src={23} srcsetSourceSrcSet=value>Text</source></audio><br>
        Track: <video><track src={24}>Text</track></video><br>
        Video: <video poster={25} src={26}>Text</video><br>
    </body>
</html>
", subUriStrings.Cast<object>().ToArray());
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriStrings);
                var actual = WebTest.GetActualResources(webTest,
                    subUriStrings.Select(subUriString => new WebTestInfo(webTest, subUriString, HtmlParser.MimeType, EmptyHtml)).ToArray());
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("", null)]
        [InlineData("<base target=value>", null)]
        [InlineData("<base href=invalidAbsoluteUri>", null)]
        [InlineData("<base href={0}>", "baseUri")]
        public void Rip_BasicHtmlWithBase_ReturnsExpectedBaseUri(string baseElementFormat, string baseUriStringWithoutScheme)
        {
            var baseUri = baseUriStringWithoutScheme != null ? WebTest.GetUri(baseUriStringWithoutScheme) : null;
            var html = GetHtml(baseElements: string.Format(baseElementFormat, baseUri));
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = baseUri;
                var actual = ((HtmlParser)WebTest.GetActualResources(webTest).Single().Parser).BaseUri;
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_BasicHtmlWithDuplicateBases_ReturnsSingleResource()
        {
            var html = GetHtml(baseElements: string.Format("{0}{1}", GetBaseElement("base1"), GetBaseElement("base2")), bodyElements: "<a href=value>Text</a>");
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = WebTest.GetExpectedResources(webTest);
                var actual = WebTest.GetActualResources(webTest);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("", "", "", "", "<a href={0}>Text</a>")]
        //[InlineData("", "", "", "", "<applet archive={0}>Text</applet>")]
        [InlineData("", "", "", "", "<applet code={0}>Text</applet>")]
        //[InlineData("", "", "", "", "<applet codebase={0}>Text</applet>")]
        [InlineData("", "", "", "", "<area href={0}>Text</area>")]
        [InlineData("", "", "", "", "<audio src={0}>Text</audio>")]
        [InlineData("", "", "", " background={0}", "")]
        [InlineData("", "", "", "", "<embed src={0}>Text</embed>")]
        [InlineData("", "", "", "", "<frame longdesc={0}>Text</frame>")]
        [InlineData("", "", "", "", "<frame src={0}>Text</frame>")]
        [InlineData("", " profile={0}", "", "", "")]
        [InlineData("", "", "", "", "<iframe longdesc={0}>Text</iframe>")]
        [InlineData("", "", "", "", "<iframe src={0}>Text</iframe>")]
        [InlineData("", "", "", "", "<img longdesc={0}>Text</img>")]
        [InlineData("", "", "", "", "<img src={0}>Text</img>")]
        [InlineData("", "", "", "", "<input src={0}>Text</input>")]
        [InlineData("", "", "<link rel=icon href={0}>Text</link>", "", "")]
        [InlineData("", "", "<link rel=pingback href={0}>Text</link>", "", "")]
        [InlineData("", "", "<link rel=prefetch href={0}>Text</link>", "", "")]
        [InlineData("", "", "<link rel=stylesheet href={0}>Text</link>", "", "")]
        [InlineData("", "", "", "", "<menuitem icon={0}>Text</menuitem>")]
        //[InlineData("", "", "", "", "<object archive={0}>Text</object>")]
        [InlineData("", "", "", "", "<object classid={0}>Text</object>")]
        //[InlineData("", "", "", "", "<object codebase={0}>Text</object>")]
        [InlineData("", "", "", "", "<object data={0}>Text</object>")]
        [InlineData("", "", "", "", "<script src={0}>Text</script>")]
        [InlineData("", "", "", "", "<source src={0}>Text</source>")]
        //[InlineData("", "", "", "", "<source srcset={0}>Text</source>")]
        [InlineData("", "", "", "", "<track src={0}>Text</track>")]
        [InlineData("", "", "", "", "<video poster={0}>Text</video>")]
        [InlineData("", "", "", "", "<video src={0}>Text</video>")]
        public void Rip_BasicHtmlWithBaseSensitiveReference_ReturnsRebasedResources(string htmlAttributesFormat,
            string headAttributesFormat, string headElementsFormat,
            string bodyAttributesFormat, string bodyElementsFormat)
        {
            const string subUriString = "uri";
            const string baseUriStringWithoutScheme = "baseUri";
            var html = GetHtml(string.Format(htmlAttributesFormat, subUriString), string.Format(headAttributesFormat, subUriString),
                GetBaseElement(baseUriStringWithoutScheme), string.Format(headElementsFormat, subUriString),
                string.Format(bodyAttributesFormat, subUriString), string.Format(bodyElementsFormat, subUriString));
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var subUri = new Uri(WebTest.GetUri(baseUriStringWithoutScheme), subUriString);
                var expected = WebTest.GetExpectedResources(webTest, subUri);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUri, HtmlParser.MimeType, EmptyHtml));
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(" manifest={0}", "", "", "", "")]
        public void Rip_BasicHtmlWithBaseInsensitiveReference_ReturnsNotRebasedResources(string htmlAttributesFormat,
            string headAttributesFormat, string headElementsFormat,
            string bodyAttributesFormat, string bodyElementsFormat)
        {
            const string subUriString = "uri";
            var html = GetHtml(string.Format(htmlAttributesFormat, subUriString), string.Format(headAttributesFormat, subUriString),
                null, string.Format(headElementsFormat, subUriString),
                string.Format(bodyAttributesFormat, subUriString), string.Format(bodyElementsFormat, subUriString));
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString, HtmlParser.MimeType, EmptyHtml));
                Assert.Equal(expected, actual);
            }
        }
    }
}
