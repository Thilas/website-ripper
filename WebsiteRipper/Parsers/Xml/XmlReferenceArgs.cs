using System.Xml;

namespace WebsiteRipper.Parsers.Xml
{
    public sealed class XmlReferenceArgs : ReferenceArgs<XmlNode, XmlAttribute>
    {
        public XmlReferenceArgs(Parser parser, ReferenceKind kind, string mimeType, XmlNode node, XmlAttribute attribute)
            : base(parser, kind, mimeType, node, attribute)
        {
        }
    }
}
