using System.Xml;

namespace WebsiteRipper.Parsers.Xml.References
{
    [Node("xml-stylesheet")]
    [Reference("href")]
    public sealed class XmlStyleSheet : ProcessingInstructionReference
    {
        public XmlStyleSheet(Parser parser, ReferenceKind kind, XmlProcessingInstruction node, XmlAttribute attribute) : base(parser, kind, node, attribute) { }
    }
}
