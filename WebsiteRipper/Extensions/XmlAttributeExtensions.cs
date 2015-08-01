using System;
using System.Xml;
using WebsiteRipper.Parsers.Xml;

namespace WebsiteRipper.Extensions
{
    static class XmlAttributeExtensions
    {
        public static XmlDocument GetOwnerDocument(this XmlAttribute attribute)
        {
            if (attribute == null) throw new ArgumentNullException("attribute");
            var ownerDocument = attribute.OwnerDocument;
            if (ownerDocument == null) throw new InvalidOperationException("Attribute has no owner document.");
            return ownerDocument;
        }

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
            var xmlNsPrefix = GetOwnerDocument(attribute).NameTable.Add(XmlParser.XmlNsPrefix);
            return ReferenceEquals(attribute.Prefix, xmlNsPrefix) ? attribute.LocalName : null;
        }
    }
}
