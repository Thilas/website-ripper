namespace WebsiteRipper.Parsers.Xml.XsdReferences
{
    [ReferenceNode(Namespace = XmlParser.XsdNamespace)]
    [ReferenceAttribute("schemaLocation")]
    public sealed class Redefine : XmlReference
    {
        public Redefine(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
