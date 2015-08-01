using System.Xml;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Parsers.Xml.References
{
    [ReferenceNode("xml-stylesheet")]
    [ReferenceAttribute("href")]
    public sealed class XmlStyleSheet : ProcessingInstructionReference
    {
        static string GetMimeType(XmlAttribute attribute)
        {
            var typeAttribute = attribute.GetOwnerElement().Attributes["type"];
            return typeAttribute != null ? typeAttribute.Value : null;
        }

        public XmlStyleSheet(Parser parser, ReferenceKind kind, XmlProcessingInstruction node, XmlAttribute attribute)
            : base(parser, kind, GetMimeType(attribute), node, attribute) { }
    }
}
