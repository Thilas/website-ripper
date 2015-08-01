using System;
using System.Xml;
using WebsiteRipper.Parsers.Xml;

namespace WebsiteRipper.Extensions
{
    static class XmlAttributeExtensions
    {
        public static string GetNamespacePrefix(this XmlAttribute attribute)
        {
            var ownerDocument = attribute.OwnerDocument;
            if (ownerDocument == null) throw new InvalidOperationException("Attribute has no owner document.");
            var xmlNsPrefix = ownerDocument.NameTable.Add(XmlParser.XmlNsPrefix);
            return ReferenceEquals(attribute.Prefix, xmlNsPrefix) ? attribute.LocalName : null;
        }
    }
}
