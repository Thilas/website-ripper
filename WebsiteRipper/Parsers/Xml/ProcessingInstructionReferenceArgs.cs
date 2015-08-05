using System.Xml;

namespace WebsiteRipper.Parsers.Xml
{
    public sealed class ProcessingInstructionReferenceArgs : ReferenceArgs<XmlProcessingInstruction, XmlAttribute>
    {
        public ProcessingInstructionReferenceArgs(Parser parser, ReferenceKind kind, string mimeType, XmlProcessingInstruction processingInstruction, XmlAttribute attribute)
            : base(parser, kind, mimeType, processingInstruction, attribute) { }
    }
}
