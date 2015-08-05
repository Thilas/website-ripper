using System.Linq;
using WebsiteRipper.Parsers.Css;
using WebsiteRipper.Tests.Fixtures;
using Xunit;

namespace WebsiteRipper.Tests.Parsers
{
    public sealed class CssParserTests
    {
        const string EmptyCss = "";

        [Theory]
        [InlineData(null)]
        [InlineData(EmptyCss)]
        [InlineData("\n")]
        [InlineData(" ")]
        [InlineData("/* comment */")]
        public void Rip_EmptyCss_ReturnsSingleResource(string css)
        {
            using (var webTest = new WebTestInfo(CssParser.MimeType, css))
            {
                var expected = WebTest.GetExpectedResources(webTest);
                var actual = WebTest.GetActualResources(webTest);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_EmptyCss_ReturnsCssParser()
        {
            using (var webTest = new WebTestInfo(CssParser.MimeType, EmptyCss))
            {
                var expected = typeof(CssParser);
                var actual = WebTest.GetActualResources(webTest).Single().Parser;
                Assert.IsType(expected, actual);
            }
        }

        [Fact]
        public void Rip_EmptyCss_ReturnsCssMimeType()
        {
            using (var webTest = new WebTestInfo(CssParser.MimeType, EmptyCss))
            {
                var expected = CssParser.MimeType;
                var actual = WebTest.GetActualResources(webTest).Single().Parser.ActualMimeType;
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_EmptyCss_ReturnsExpectedUri()
        {
            using (var webTest = new WebTestInfo(CssParser.MimeType, EmptyCss))
            {
                var expected = webTest.Uri;
                var actual = WebTest.GetActualResources(webTest).Single().OriginalUri;
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("selector {property:value}")]
        public void Rip_BasicCssWithNoReferences_ReturnsSingleResource(string css)
        {
            using (var webTest = new WebTestInfo(CssParser.MimeType, css))
            {
                var expected = WebTest.GetExpectedResources(webTest);
                var actual = WebTest.GetActualResources(webTest);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("@import '{0}'")]
        [InlineData("@import '{0}' media")]
        [InlineData("@import url('{0}')")]
        [InlineData("@import url('{0}') media")]
        [InlineData("selector {{property:url('{0}')}}")]
        public void Rip_BasicCssWithReference_ReturnsExpectedResources(string cssFormat)
        {
            const string subUriString = "uri";
            var css = string.Format(cssFormat, subUriString);
            using (var webTest = new WebTestInfo(CssParser.MimeType, css))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString, CssParser.MimeType, EmptyCss));
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("@import '{0}'")]
        public void Rip_BasicCssWithEscapedReference_ReturnsExpectedResources(string cssFormat)
        {
            const string encodedSubUriString = "uri\\''";
            const string subUriString = "uri'";
            var css = string.Format(cssFormat, encodedSubUriString);
            using (var webTest = new WebTestInfo(CssParser.MimeType, css))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString, CssParser.MimeType, EmptyCss));
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_ComplexCss_ReturnsExpectedResources()
        {
            var subUriStrings = new[] { "importUri", "selector2Property2Uri", "selector3Property1Uri", "selector3Property2Uri" };
            var css = string.Format(@"/* comment */
@import '{0}';

selector1 {{
    property:value;
}}
selector2 {{
    property1:value;
    property2:url('{1}');
}}
selector3 {{
    property1:url('{2}');
    property2:url('{3}');
}}
", subUriStrings.Cast<object>().ToArray());
            using (var webTest = new WebTestInfo(CssParser.MimeType, css))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriStrings);
                var actual = WebTest.GetActualResources(webTest,
                    subUriStrings.Select(subUriString => new WebTestInfo(webTest, subUriString, CssParser.MimeType, EmptyCss)).ToArray());
                Assert.Equal(expected, actual);
            }
        }
    }
}
