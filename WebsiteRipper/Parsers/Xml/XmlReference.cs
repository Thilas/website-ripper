using System.Collections.Generic;
using System.Xml;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Parsers.Xml
{
    public abstract class XmlReference : Reference<XmlNode, XmlAttribute, XmlReferenceArgs>
    {
        internal static IEnumerable<Reference> Create(Parser parser, XmlNode node)
        {
            return Create(parser, node,
                xmlNode => xmlNode.LocalName,
                xmlNode => xmlNode.NamespaceURI,
                xmlNode => xmlNode.GetAttributes(),
                xmlAttribute => xmlAttribute.LocalName,
                xmlAttribute => xmlAttribute.NamespaceURI);
        }

        protected XmlReference(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }

        protected sealed override string InternalUri
        {
            get { return Attribute.Value; }
            set { Attribute.Value = value; }
        }
    }
}
