namespace WebsiteRipper.Parsers.Xml.XsltReferences
{
    [ReferenceNode(Name = "import-schema", Namespace = XmlParser.XsltNamespace)]
    [ReferenceAttribute("schema-location")]
    public sealed class ImportSchema : XmlReference
    {
        public ImportSchema(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
