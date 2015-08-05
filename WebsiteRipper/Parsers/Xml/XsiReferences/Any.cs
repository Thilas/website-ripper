namespace WebsiteRipper.Parsers.Xml.XsiReferences
{
    [ReferenceNode(Any = true, Namespace = XmlParser.XsiNamespace, QualifiedAttributes = true)]
    [ReferenceAttribute("schemaLocation")] // TODO: Handle multiple references in schemaLocation
    [ReferenceAttribute("noNamespaceSchemaLocation")]
    public sealed class Any : XmlReference
    {
        public Any(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
