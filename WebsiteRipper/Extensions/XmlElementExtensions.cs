using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WebsiteRipper.Extensions
{
    static class XmlElementExtensions
    {
        public static IEnumerable<XmlElement> Descendants(this XmlElement node)
        {
            return node.ChildNodes.OfType<XmlElement>().SelectMany(descendant => Descendants(descendant).Prepend(descendant));
        }

        public static IEnumerable<XmlElement> DescendantsAndSelf(this XmlElement node)
        {
            yield return node;
            foreach (var descendant in Descendants(node)) yield return descendant;
        }
    }
}
