using System.Xml;

namespace WebsiteRipper.Parsers.Xml
{
    public sealed class ProcessingInstructionReferenceArgs : ReferenceArgs<XmlProcessingInstruction, XmlAttribute>
    {
        internal ProcessingInstructionReferenceArgs(Parser parser, ReferenceKind kind, string mimeType, XmlProcessingInstruction node, XmlAttribute attribute)
            : base(parser, kind, mimeType, node, attribute)
        {
        }
    }
}
