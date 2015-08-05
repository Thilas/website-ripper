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
        const string HtmlFormat = "<html {0}><head {1}><title>Title</title>{2}{3}</head><body {4}>Body{5}</body></html>";
        const string BaseFormat = "<base href={0}>";

        static string GetHtml(string htmlAttributes = null, string headAttributes = null, string baseNodes = null, string headNodes = null,
            string bodyAttributes = null, string bodyNodes = null)
        {
            return string.Format(HtmlFormat, htmlAttributes, headAttributes, baseNodes, headNodes, bodyAttributes, bodyNodes);
        }

        static string GetBaseNode(string baseUriStringWithoutScheme) { return string.Format(BaseFormat, WebTest.GetUri(baseUriStringWithoutScheme)); }

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
        [InlineData(null, "<node attribute=value>Text</node>")]
        [InlineData("<link rel=dns-prefetch href=value>Text</link>", null)]
        // Not yet supported
        [InlineData(null, "<applet archive=value>Text</applet>")]
        [InlineData(null, "<applet codebase=value>Text</applet>")]
        [InlineData(null, "<object archive=value>Text</object>")]
        [InlineData(null, "<object codebase=value>Text</object>")]
        [InlineData(null, "<source srcset=value>Text</source>")]
        public void Rip_BasicHtmlWithNoReferences_ReturnsSingleResource(string headNodes, string bodyNodes)
        {
            var html = GetHtml(headNodes: headNodes, bodyNodes: bodyNodes);
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
            string headAttributesFormat, string headNodesFormat,
            string bodyAttributesFormat, string bodyNodesFormat)
        {
            const string subUriString = "uri";
            var html = GetHtml(
                htmlAttributes: string.Format(htmlAttributesFormat, subUriString),
                headAttributes: string.Format(headAttributesFormat, subUriString),
                headNodes: string.Format(headNodesFormat, subUriString),
                bodyAttributes: string.Format(bodyAttributesFormat, subUriString),
                bodyNodes: string.Format(bodyNodesFormat, subUriString));
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString, HtmlParser.MimeType, EmptyHtml));
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("", "", "", "", "<a href={0}>Text</a>")]
        public void Rip_BasicHtmlWithEscapedReference_ReturnsExpectedResources(string htmlAttributesFormat,
            string headAttributesFormat, string headNodesFormat,
            string bodyAttributesFormat, string bodyNodesFormat)
        {
            const string encodedSubUriString = "uri&lt;&amp;&quot;&gt;";
            const string subUriString = "uri<&\">";
            var html = GetHtml(
                htmlAttributes: string.Format(htmlAttributesFormat, encodedSubUriString),
                headAttributes: string.Format(headAttributesFormat, encodedSubUriString),
                headNodes: string.Format(headNodesFormat, encodedSubUriString),
                bodyAttributes: string.Format(bodyAttributesFormat, encodedSubUriString),
                bodyNodes: string.Format(bodyNodesFormat, encodedSubUriString));
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
        Node: <node attribute=value>Text</node>
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
        public void Rip_BasicHtmlWithBase_ReturnsExpectedBaseUri(string baseNodeFormat, string baseUriStringWithoutScheme)
        {
            var baseUri = baseUriStringWithoutScheme != null ? WebTest.GetUri(baseUriStringWithoutScheme) : null;
            var html = GetHtml(baseNodes: string.Format(baseNodeFormat, baseUri));
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
            var html = GetHtml(
                baseNodes: string.Format("{0}{1}", GetBaseNode("base1"), GetBaseNode("base2")),
                bodyNodes: "<a href=value>Text</a>");
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
            string headAttributesFormat, string headNodesFormat,
            string bodyAttributesFormat, string bodyNodesFormat)
        {
            const string subUriString = "uri";
            const string baseUriStringWithoutScheme = "baseUri";
            var html = GetHtml(
                htmlAttributes: string.Format(htmlAttributesFormat, subUriString),
                headAttributes: string.Format(headAttributesFormat, subUriString),
                baseNodes: GetBaseNode(baseUriStringWithoutScheme),
                headNodes: string.Format(headNodesFormat, subUriString),
                bodyAttributes: string.Format(bodyAttributesFormat, subUriString),
                bodyNodes: string.Format(bodyNodesFormat, subUriString));
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
            string headAttributesFormat, string headNodesFormat,
            string bodyAttributesFormat, string bodyNodesFormat)
        {
            const string subUriString = "uri";
            var html = GetHtml(
                htmlAttributes: string.Format(htmlAttributesFormat, subUriString),
                headAttributes: string.Format(headAttributesFormat, subUriString),
                headNodes: string.Format(headNodesFormat, subUriString),
                bodyAttributes: string.Format(bodyAttributesFormat, subUriString),
                bodyNodes: string.Format(bodyNodesFormat, subUriString));
            using (var webTest = new WebTestInfo(HtmlParser.MimeType, html))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString, HtmlParser.MimeType, EmptyHtml));
                Assert.Equal(expected, actual);
            }
        }
    }
}
