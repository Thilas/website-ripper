using System.Xml;

namespace WebsiteRipper.Parsers.Xml
{
    public sealed class XmlReferenceArgs : ReferenceArgs<XmlElement, XmlAttribute>
    {
        public XmlReferenceArgs(Parser parser, ReferenceKind kind, string mimeType, XmlElement element, XmlAttribute attribute)
            : base(parser, kind, mimeType, element, attribute) { }
    }
}
