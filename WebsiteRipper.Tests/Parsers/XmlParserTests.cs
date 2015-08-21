using System;
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
        static readonly string _xmlFormat = string.Format("<?xml version=\"1.0\" encoding=\"{0}\"?>{{0}}<{{1}} {{2}}>{{3}}</{{1}}>", _encodingName);

        static string GetXml(string processingInstructions = null, string rootElement = null, string rootAttributes = null, string rootElements = null)
        {
            return string.Format(_xmlFormat, processingInstructions, rootElement ?? "root", rootAttributes, rootElements);
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
        [InlineData(XmlParser.XsltApplicationMimeType)]
        [InlineData(XmlParser.XsltTextMimeType)]
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
        [InlineData(XmlParser.XsltApplicationMimeType)]
        [InlineData(XmlParser.XsltTextMimeType)]
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
        [InlineData(XmlParser.XsltApplicationMimeType)]
        [InlineData(XmlParser.XsltTextMimeType)]
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
        [InlineData(null, null, null)]
        [InlineData("<?processingInstruction data?>", null, null)]
        [InlineData(null, "attribute=\"value\"", null)]
        [InlineData(null, null, "text")]
        [InlineData(null, null, "<element attribute=\"value\"/>")]
        [InlineData(null, null, "<element attribute=\"value\">text</element>")]
        public void Rip_BasicXmlWithNoReferences_ReturnsSingleResource(string processingInstructions, string rootAttributes, string rootElements)
        {
            var xml = GetXml(
                processingInstructions: processingInstructions,
                rootAttributes: rootAttributes,
                rootElements: rootElements);
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest);
                var actual = WebTest.GetActualResources(webTest);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("<!DOCTYPE root SYSTEM \"{0}\">", null, "", "", null)]
        [InlineData("<?xml-stylesheet href=\"{0}\"?>", null, "", "", null)]
        [InlineData("", null, "xmlns:xsi=\"{{0}}\" xsi:noNamespaceSchemaLocation=\"{0}\"", "", XmlParser.XsiNamespace)]
        [InlineData("", "schema", "xmlns=\"{{0}}\"", "<include schemaLocation=\"{0}\"/>", XmlParser.XsdNamespace)]
        [InlineData("", "xsd:schema", "xmlns:xsd=\"{{0}}\"", "<xsd:include schemaLocation=\"{0}\"/>", XmlParser.XsdNamespace)]
        [InlineData("", "xsd:schema", "xmlns:xsd=\"{{0}}\"", "<xsd:redefine schemaLocation=\"{0}\"/>", XmlParser.XsdNamespace)]
        [InlineData("", "xsd:schema", "xmlns:xsd=\"{{0}}\"", "<xsd:override schemaLocation=\"{0}\"/>", XmlParser.XsdNamespace)]
        [InlineData("", "xsd:schema", "xmlns:xsd=\"{{0}}\"", "<xsd:import schemaLocation=\"{0}\"/>", XmlParser.XsdNamespace)]
        [InlineData("", "xsd:schema", "xmlns:xsd=\"{{0}}\"", "<xsd:appinfo source=\"{0}\"/>", XmlParser.XsdNamespace)]
        [InlineData("", "xsd:schema", "xmlns:xsd=\"{{0}}\"", "<xsd:documentation source=\"{0}\"/>", XmlParser.XsdNamespace)]
        [InlineData("", "xsl:stylesheet", "xmlns:xsl=\"{{0}}\"", "<xsl:import href=\"{0}\"/>", XmlParser.XsltNamespace)]
        [InlineData("", "xsl:stylesheet", "xmlns:xsl=\"{{0}}\"", "<xsl:import-schema schema-location=\"{0}\"/>", XmlParser.XsltNamespace)]
        [InlineData("", "xsl:stylesheet", "xmlns:xsl=\"{{0}}\"", "<xsl:include href=\"{0}\"/>", XmlParser.XsltNamespace)]
        public void Rip_BasicXmlWithReference_ReturnsExpectedResources(string processingInstructionsFormat,
            string rootElement, string rootAttributesFormat, string rootElementsFormat, string @namespace)
        {
            const string subUriString = "uri";
            var xml = string.Format(GetXml(
                processingInstructions: string.Format(processingInstructionsFormat, subUriString),
                rootElement: rootElement,
                rootAttributes: string.Format(rootAttributesFormat, subUriString),
                rootElements: string.Format(rootElementsFormat, subUriString)), @namespace);
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString));
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("xmlns:xsi=\"{{0}}\" xsi:schemaLocation=\"value\"", XmlParser.XsiNamespace, new string[] { })]
        [InlineData("xmlns:xsi=\"{{0}}\" xsi:schemaLocation=\"nsUri {0}\"", XmlParser.XsiNamespace, new[] { "uri" })]
        [InlineData("xmlns:xsi=\"{{0}}\" xsi:schemaLocation=\"nsUri {0} value\"", XmlParser.XsiNamespace, new[] { "uri" })]
        [InlineData("xmlns:xsi=\"{{0}}\" xsi:schemaLocation=\"nsUri1 {0} nsUri2 {1} nsUri3 {2}\"", XmlParser.XsiNamespace, new[] { "uri1", "uri2", "uri3" })]
        [InlineData("xmlns:xsi=\"{{0}}\" xsi:schemaLocation=\"nsUri1 {0} nsUri2 {1} nsUri3 {2} value\"", XmlParser.XsiNamespace, new[] { "uri1", "uri2", "uri3" })]
        public void Rip_BasicXmlWithMultipleReferences_ReturnsExpectedResources(string rootAttributesFormat, string @namespace, string[] subUriStrings)
        {
            var xml = string.Format(GetXml(rootAttributes: string.Format(rootAttributesFormat, subUriStrings.Cast<object>().ToArray())), @namespace);
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriStrings);
                var actual = WebTest.GetActualResources(webTest, subUriStrings.Select(subUriString => new WebTestInfo(webTest, subUriString)).ToArray());
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("<!DOCTYPE root SYSTEM \"{0}\">", "uri<'>")]
        [InlineData("<!DOCTYPE root SYSTEM '{0}'>", "uri<\">")]
        public void Rip_BasicXmlWithEscapedReferenceInDocType_ReturnsExpectedResources(string processingInstructionsFormat, string subUriString)
        {
            var xml = GetXml(processingInstructions: string.Format(processingInstructionsFormat, subUriString));
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString));
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("<?xml-stylesheet href=\"{0}\"?>", null, "", "", null)]
        [InlineData("", "xsd:schema", "xmlns:xsd=\"{{0}}\"", "<xsd:include schemaLocation=\"{0}\"/>", XmlParser.XsdNamespace)]
        public void Rip_BasicXmlWithEscapedReference_ReturnsExpectedResources(string processingInstructionsFormat,
            string rootElement, string rootAttributesFormat, string rootElementsFormat, string @namespace)
        {
            const string encodedSubUriString = "uri&lt;&apos;&quot;&gt;";
            const string subUriString = "uri<'\">";
            var xml = string.Format(GetXml(
                processingInstructions: string.Format(processingInstructionsFormat, encodedSubUriString),
                rootElement: rootElement ?? "root",
                rootAttributes: string.Format(rootAttributesFormat, encodedSubUriString),
                rootElements: string.Format(rootElementsFormat, encodedSubUriString)), @namespace);
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var expected = WebTest.GetExpectedResources(webTest, subUriString);
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString));
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Rip_BasicXmlStyleSheetWithType_ReturnsResourcesWithExpectedMimeType()
        {
            const string mimeType = "type/subtype";
            const string subUriString = "uri";
            var xml = GetXml(processingInstructions: string.Format("<?xml-stylesheet type=\"{0}\" href=\"{1}\"?>", mimeType, subUriString));
            using (var webTest = new WebTestInfo(XmlParser.MimeType, xml))
            {
                var actual = WebTest.GetActualResources(webTest, new WebTestInfo(webTest, subUriString));
                Assert.Single(actual, resource => string.Equals(resource.Parser.ActualMimeType, mimeType, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public void Rip_ComplexXml_ReturnsExpectedResources()
        {
            var args = new[] { _encodingName, XmlParser.XsdNamespace, XmlParser.XsiNamespace, XmlParser.XsltNamespace };
            var subUriStrings = new[]
            {
                "doctypeUri", "xmlStyleSheetUri", "xsiSchemaLocationUri1", "xsiSchemaLocationUri2",
                "xsiNoNamespaceSchemaLocationUri", "schemaIncludeSchemaLocationUri", "xsdIncludeSchemaLocationUri",
                "xsdRedefineSchemaLocationUri", "xsdOverrideSchemaLocationUri", "xsdImportSchemaLocationUri",
                "xsdAppInfoSourceUri", "xsdDocumentationSourceUri", "xsltImportHrefUri",
                "xsltImportSchemaSchemaLocationUri", "xsltIncludeHrefUri"
            };
            var xml = string.Format(@"<?xml version=""1.0"" encoding=""{0}""?>
<!DOCTYPE root SYSTEM ""{4}"">
<?xml-stylesheet href=""{5}""?>
<root xmlns=""nsUri"" xmlns:prefix=""prefixNsUri"" xmlns:xsi=""{2}"" xsi:schemaLocation=""ns1 {6} ns2 {7}"" xsi:noNamespaceSchemaLocation=""{8}"">
    <schema xmlns=""{1}"">
        <include schemaLocation=""{9}""/>
    </schema>
    <xsd:schema xmlns:xsd=""{1}"">
        <xsd:include schemaLocation=""{10}""/>
        <xsd:redefine schemaLocation=""{11}""/>
        <xsd:override schemaLocation=""{12}""/>
        <xsd:import schemaLocation=""{13}""/>
        <xsd:appinfo source=""{14}""/>
        <xsd:documentation source=""{15}""/>
    </xsd:schema>
    <xsl:stylesheet xmlns:xsl=""{3}"" version=""1.0"">
        <xsl:import href=""{16}""/>
        <xsl:import-schema schema-location=""{17}""/>
        <xsl:include href=""{18}""/>
    </xsl:stylesheet>
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
