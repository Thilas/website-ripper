using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WebsiteRipper.Extensions
{
    static class XmlNodeExtensions
    {
        public static IEnumerable<XmlAttribute> GetAttributes(this XmlNode node)
        {
            if (node == null) throw new ArgumentNullException("node");
            var attributes = node.Attributes;
            if (attributes == null) throw new InvalidOperationException("Node has no attributes.");
            return attributes.Cast<XmlAttribute>();
        }
    }
}
