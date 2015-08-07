using System.Xml;

namespace WebsiteRipper.Parsers.Xml.XsltReferences
{
    [ReferenceElement(Name = "import-schema", Namespace = XmlParser.XsltNamespace)]
    [ReferenceAttribute("schema-location")]
    public sealed class ImportSchema : XmlReference
    {
        public ImportSchema(ReferenceArgs<XmlElement, XmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
