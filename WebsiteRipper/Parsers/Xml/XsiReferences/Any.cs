using System.Xml;

namespace WebsiteRipper.Parsers.Xml.XsiReferences
{
    [ReferenceElement(Any = true, Namespace = XmlParser.XsiNamespace, QualifiedAttributes = true)]
    [ReferenceAttribute("schemaLocation")] // TODO: Handle multiple references in schemaLocation
    [ReferenceAttribute("noNamespaceSchemaLocation")]
    public sealed class Any : XmlReference
    {
        public Any(ReferenceArgs<XmlElement, XmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
