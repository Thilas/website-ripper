using System.Linq;
using WebsiteRipper.Extensions;
using WebsiteRipper.Parsers.Xml;
using WebsiteRipper.Tests.Fixtures;
using Xunit;

namespace WebsiteRipper.Tests.Parsers
{
    public sealed class XmlParserTests
    {
        static readonly string _encodingName = WebTest.Encoding.WebName;

        const string EmptyXml = "";
        static readonly string _xmlFormat = string.Format("<?xml version=\"{{0}}\" encoding=\"{0}\"?>{{1}}<root><element attribute=\"value\">text</element></root>", _encodingName);

        private const string XmlVersion10 = "1.0";
        private const string XmlVersion11 = "1.1";

        static string GetXml(string xmlVersion, string processingInstructions = null)
        {
            return string.Format(_xmlFormat, xmlVersion, processingInstructions);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(EmptyXml)]
        [InlineData("\n")]
        [InlineData(" ")]
        [InlineData("<!-- comment -->")]
        public void Rip_EmptyXml_ReturnsSingleResource(string xml)
        {
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest);
                var actual = WebTest.GetActualResources(webTest);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(XmlParser.ApplicationMimeType)]
        [InlineData(XmlParser.TextMimeType)]
        public void Rip_EmptyXml_ReturnsXmlParser(string mimeType)
        {
            using (var webTest = new WebTestInfo(mimeType, EmptyXml))
            {
                var expected = typeof(XmlParser);
                var actual = WebTest.GetActualResources(webTest).Single().Parser;
                Assert.IsType(expected, actual);
            }
        }

        [Theory]
        [InlineData(XmlParser.ApplicationMimeType)]
        [InlineData(XmlParser.TextMimeType)]
        public void Rip_EmptyXml_ReturnsXmlMimeType(string mimeType)
        {
            using (var webTest = new WebTestInfo(mimeType, EmptyXml))
            {
                var expected = mimeType;
                var actual = WebTest.GetActualResources(webTest).Single().Parser.ActualMimeType;
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(XmlParser.ApplicationMimeType)]
        [InlineData(XmlParser.TextMimeType)]
        public void Rip_EmptyXml_ReturnsExpectedUri(string mimeType)
        {
            using (var webTest = new WebTestInfo(mimeType, EmptyXml))
            {
                var expected = webTest.Uri;
                var actual = WebTest.GetActualResources(webTest).Single().OriginalUri;
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(XmlVersion10, null)]
        [InlineData(XmlVersion11, null)]
        // Not yet supported
        [InlineData(XmlVersion11, "<?xml-stylesheet href=\"value\"?>")]
        public void Rip_BasicXmlWithNoReferences_ReturnsSingleResource(string xmlVersion, string processingInstructions)
        {
            var xml = GetXml(xmlVersion, processingInstructions);
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest);
                var actual = WebTest.GetActualResources(webTest);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(XmlVersion10, "<?xml-stylesheet href=\"{0}\"?>")]
        //[InlineData(XmlVersion11, "<?xml-stylesheet href=\"{0}\"?>")]
        public void Rip_BasicXmlWithReference_ReturnsExpectedResources(string xmlVersion, string processingInstructionsFormat)
        {
            const string subUriString = "uri";
            var xml = GetXml(xmlVersion, string.Format(processingInstructionsFormat, subUriString));
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString, XmlParser.MimeType, EmptyXml));
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(XmlVersion10, "<?xml-stylesheet href=\"uri&lt;&apos;&quot;&gt;\"?>")]
        //[InlineData(XmlVersion11, "<?xml-stylesheet href=\"uri&lt;&apos;&quot;&gt;\"?>")]
        public void Rip_BasicXmlWithComplexReference_ReturnsExpectedResources(string xmlVersion, string processingInstructions)
        {
            const string subUriString = "uri<'\">";
            var xml = GetXml(xmlVersion, processingInstructions);
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString, XmlParser.MimeType, EmptyXml));
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(XmlVersion10)]
        //[InlineData(XmlVersion11)]
        public void Rip_ComplexXml_ReturnsExpectedResources(string xmlVersion)
        {
            var subUriStrings = new[] { "xmlStyleSheetUri" };
            var xml = string.Format(@"<?xml version=""{0}"" encoding=""UTF-8""?>
<?xml-stylesheet href=""{1}""?>
<root>
    <element attribute=""value"">text</element>
</root>
", subUriStrings.Select(subUriString => (object)subUriString).Prepend(xmlVersion).ToArray());
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriStrings);
                var actual = WebTest.GetActualResources(webTest,
                    subUriStrings.Select(subUriString => new WebTestInfo(webTest, subUriString, XmlParser.MimeType, EmptyXml)).ToArray());
                Assert.Equal(expected, actual);
            }
        }
    }
}
