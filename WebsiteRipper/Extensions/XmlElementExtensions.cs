using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WebsiteRipper.Extensions
{
    static class XmlElementExtensions
    {
        public static IEnumerable<XmlElement> Descendants(this XmlElement element)
        {
            if (element == null) throw new ArgumentNullException("element");
            return element.ChildNodes.OfType<XmlElement>().SelectMany(descendant => Descendants(descendant).Prepend(descendant));
        }

        public static IEnumerable<XmlElement> DescendantsAndSelf(this XmlElement element)
        {
            if (element == null) throw new ArgumentNullException("element");
            yield return element;
            foreach (var descendant in Descendants(element)) yield return descendant;
        }
    }
}
