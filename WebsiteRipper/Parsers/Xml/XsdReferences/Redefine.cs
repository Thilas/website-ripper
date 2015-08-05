namespace WebsiteRipper.Parsers.Xml.XsdReferences
{
    [ReferenceElement(Namespace = XmlParser.XsdNamespace)]
    [ReferenceAttribute("schemaLocation")]
    public sealed class Redefine : XmlReference
    {
        public Redefine(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
