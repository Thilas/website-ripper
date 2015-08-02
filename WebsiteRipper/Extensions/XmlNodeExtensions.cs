using System;
using System.Xml;

namespace WebsiteRipper.Extensions
{
    static class XmlNodeExtensions
    {
        public static XmlDocument GetOwnerDocument(this XmlNode node)
        {
            if (node == null) throw new ArgumentNullException("node");
            var ownerDocument = node.OwnerDocument;
            if (ownerDocument == null) throw new InvalidOperationException("Node has no owner document.");
            return ownerDocument;
        }
    }
}
