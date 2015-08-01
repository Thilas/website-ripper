using System.Xml;

namespace WebsiteRipper.Parsers.Xml.References
{
    [ReferenceNode("xml-stylesheet")]
    [ReferenceAttribute("href")]
    public sealed class XmlStyleSheet : ProcessingInstructionReference
    {
        // TODO: Handle "type" attribute
        public XmlStyleSheet(Parser parser, ReferenceKind kind, XmlProcessingInstruction node, XmlAttribute attribute) : base(parser, kind, node, attribute) { }
    }
}
