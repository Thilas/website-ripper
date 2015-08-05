using System;
using System.Xml;

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
    }
}
