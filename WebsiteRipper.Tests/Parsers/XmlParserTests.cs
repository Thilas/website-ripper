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

        const string EmptyXml = "<root/>";
        static readonly string _xmlFormat = string.Format("<?xml version=\"1.0\" encoding=\"{0}\"?>{{0}}<root><element attribute=\"value\">text</element></root>", _encodingName);

        static string GetXml(string processingInstructions = null)
        {
            return string.Format(_xmlFormat, processingInstructions);
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
        [InlineData(null)]
        [InlineData("<?processingInstruction data?>")]
        public void Rip_BasicXmlWithNoReferences_ReturnsSingleResource(string processingInstructions)
        {
            var xml = GetXml(processingInstructions);
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest);
                var actual = WebTest.GetActualResources(webTest);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("<!DOCTYPE root SYSTEM \"{0}\">")]
        [InlineData("<?xml-stylesheet href=\"{0}\"?>")]
        public void Rip_BasicXmlWithReference_ReturnsExpectedResources(string processingInstructionsFormat)
        {
            const string subUriString = "uri";
            var xml = GetXml(string.Format(processingInstructionsFormat, subUriString));
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString));
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("<?xml-stylesheet href=\"uri&lt;&apos;&quot;&gt;\"?>")]
        public void Rip_BasicXmlWithEscapedReference_ReturnsExpectedResources(string processingInstructions)
        {
            const string subUriString = "uri<'\">";
            var xml = GetXml(processingInstructions);
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString));
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_ComplexXml_ReturnsExpectedResources()
        {
            var args = new[] { _encodingName, XmlParser.XsltNamespace };
            var subUriStrings = new[]
            {
                "doctypeUri", "xmlStyleSheetUri", "prefixedImportHrefUri", "prefixedImportSchemaSchemaLocationUri",
                "prefixedIncludeHrefUri", "importHrefUri", "importSchemaSchemaLocationUri", "includeHrefUri"
            };
            var xml = string.Format(@"<?xml version=""1.0"" encoding=""{0}""?>
<!DOCTYPE root SYSTEM ""{2}"">
<?xml-stylesheet href=""{3}""?>
<root xmlns=""ns1Uri"" xmlns:prefix1=""ns2Uri"">
    <element attribute=""value"" xmlns=""ns3Uri"" xmlns:prefix2=""ns4Uri"" xmlns:prefix3=""ns5Uri"" xmlns:prefix4=""ns6Uri"" xmlns:prefix5=""ns7Uri"">text</element>
    <element attribute=""value"" xmlns=""ns3Uri"" xmlns:prefix2=""ns4Uri"" xmlns:prefix3=""ns8Uri"" xmlns:prefix6=""ns6Uri"" xmlns:prefix7=""ns9Uri"">text</element>
    <element attribute=""value"" xmlns=""ns10Uri"">text</element>
    <xsl:stylesheet xmlns:xsl=""{1}"" version=""1.0"">
        <xsl:import href=""{4}""/>
        <xsl:import-schema schema-location=""{5}""/>
        <xsl:include href=""{6}""/>
    </xsl:stylesheet>
    <stylesheet xmlns=""{1}"" version=""2.0"">
        <import href=""{7}""/>
        <import-schema schema-location=""{8}""/>
        <include href=""{9}""/>
    </stylesheet>
</root>
", args.Concat(subUriStrings).Cast<object>().ToArray());
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriStrings);
                var actual = WebTest.GetActualResources(webTest,
                    subUriStrings.Select(subUriString => new WebTestInfo(webTest, subUriString)).ToArray());
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_ComplexXmlWithEmbeddedStyleSheet_ReturnsExpectedResources()
        {
            // This test comes from: http://www.w3.org/TR/2007/REC-xslt20-20070123/#embedded
            var subUriStrings = new[] { "doc.dtd", "doc.xsl" };
            var xml = string.Format(@"<?xml-stylesheet type=""application/xslt+xml"" href=""#style1""?>
<!DOCTYPE doc SYSTEM ""{1}"">
<doc>
    <head>
        <xsl:stylesheet id=""style1""
                        version=""2.0""
                        xmlns:xsl=""{0}""
                        xmlns:fo=""http://www.w3.org/1999/XSL/Format"">
        <xsl:import href=""{2}""/>
        <xsl:template match=""id('foo')"">
          <fo:block font-weight=""bold""><xsl:apply-templates/></fo:block>
        </xsl:template>
        <xsl:template match=""xsl:stylesheet"">
          <!-- ignore -->
        </xsl:template>
        </xsl:stylesheet>
    </head>
    <body>
        <para id=""foo"">
        ...
        </para>
    </body>
</doc>
", subUriStrings.Prepend(XmlParser.XsltNamespace).Cast<object>().ToArray());
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriStrings);
                var actual = WebTest.GetActualResources(webTest,
                    subUriStrings.Select(subUriString => new WebTestInfo(webTest, subUriString)).ToArray());
                Assert.Equal(expected, actual);
            }
        }
    }
}
