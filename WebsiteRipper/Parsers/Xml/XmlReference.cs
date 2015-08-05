using System.Collections.Generic;
using System.Xml;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Parsers.Xml
{
    public abstract class XmlReference : Reference<XmlElement, XmlAttribute, XmlReferenceArgs>
    {
        internal static IEnumerable<Reference> Create(Parser parser, XmlElement element)
        {
            return Create(parser, element,
                xmlElement => xmlElement.LocalName,
                xmlElement => xmlElement.NamespaceURI,
                xmlElement => xmlElement.GetAttributes(),
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
