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
        public void Rip_BasicCss_ReturnsCssParser()
        {
            using (var webTest = new WebTestInfo(CssParser.MimeType, EmptyCss))
            {
                var expected = typeof(CssParser);
                var actual = WebTest.GetActualResources(webTest).Single().Parser;
                Assert.IsType(expected, actual);
            }
        }

        [Fact]
        public void Rip_BasicCss_ReturnsCssMimeType()
        {
            using (var webTest = new WebTestInfo(CssParser.MimeType, EmptyCss))
            {
                var expected = CssParser.MimeType;
                var actual = WebTest.GetActualResources(webTest).Single().Parser.ActualMimeType;
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_BasicCss_ReturnsExpectedUri()
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
        [InlineData("@import 'sub'")]
        [InlineData("@import 'sub' media")]
        [InlineData("@import url('sub')")]
        [InlineData("@import url('sub') media")]
        [InlineData("selector {property:url('sub')}")]
        public void Rip_BasicCssWithReference_ReturnsExpectedResources(string css)
        {
            using (var webTest = new WebTestInfo(CssParser.MimeType, css))
            {
                const string subUriString = "sub";
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString, CssParser.MimeType, EmptyCss));
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_ComplexCss_ReturnsExpectedResources()
        {
            const string css = @"/* comment */
@import 'subImport';

selector1 {
    property:value;
}
selector2 {
    property1:value;
    property2:url('subValue1');
}
selector3 {
    property1:url('subValue2');
    property2:url('subValue3');
}
";
            using (var webTest = new WebTestInfo(CssParser.MimeType, css))
            {
                var subUriStrings = new[] { "subImport", "subValue1", "subValue2", "subValue3" };
                var expected = WebTest.GetExpectedResources(webTest, subUriStrings);
                var actual = WebTest.GetActualResources(webTest,
                    subUriStrings.Select(subUriString => new WebTestInfo(webTest, subUriString, CssParser.MimeType, EmptyCss)).ToArray());
                Assert.Equal(expected, actual);
            }
        }
    }
}
