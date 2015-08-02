using System;
using System.Xml;
using WebsiteRipper.Parsers.Xml;

namespace WebsiteRipper.Extensions
{
    static class XmlAttributeExtensions
    {
        public static XmlElement GetOwnerElement(this XmlAttribute attribute)
        {
            if (attribute == null) throw new ArgumentNullException("attribute");
            var ownerElement = attribute.OwnerElement;
            if (ownerElement == null) throw new InvalidOperationException("Attribute has no owner element.");
            return ownerElement;
        }

        public static string GetNamespacePrefix(this XmlAttribute attribute)
        {
            if (attribute == null) throw new ArgumentNullException("attribute");
            var xmlNsPrefix = attribute.GetOwnerDocument().NameTable.Add(XmlParser.XmlNsPrefix);
            return ReferenceEquals(attribute.Prefix, xmlNsPrefix) ? attribute.LocalName : null;
        }
    }
}
