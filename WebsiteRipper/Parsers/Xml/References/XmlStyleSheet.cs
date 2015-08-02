using System.Xml;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Parsers.Xml.References
{
    [ReferenceNode("xml-stylesheet")]
    [ReferenceAttribute("href", MimeTypeAttributeName = "type")]
    public sealed class XmlStyleSheet : ProcessingInstructionReference
    {
        static ProcessingInstructionReferenceArgs FixReferenceArgs(ProcessingInstructionReferenceArgs processingInstructionReferenceArgs)
        {
            return new ProcessingInstructionReferenceArgs(processingInstructionReferenceArgs.Parser,
                processingInstructionReferenceArgs.Kind, GetMimeType(processingInstructionReferenceArgs.Attribute),
                processingInstructionReferenceArgs.Node, processingInstructionReferenceArgs.Attribute);
        }

        // TODO: Make this more generic
        static string GetMimeType(XmlAttribute attribute)
        {
            var typeAttribute = attribute.GetOwnerElement().Attributes["type"];
            return typeAttribute != null ? typeAttribute.Value : null;
        }

        public XmlStyleSheet(ProcessingInstructionReferenceArgs processingInstructionReferenceArgs)
            : base(FixReferenceArgs(processingInstructionReferenceArgs)) { }
    }
}
