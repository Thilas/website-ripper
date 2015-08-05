namespace WebsiteRipper.Parsers.Xml.XsltReferences
{
    [ReferenceElement(Name = "import-schema", Namespace = XmlParser.XsltNamespace)]
    [ReferenceAttribute("schema-location")]
    public sealed class ImportSchema : XmlReference
    {
        public ImportSchema(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
