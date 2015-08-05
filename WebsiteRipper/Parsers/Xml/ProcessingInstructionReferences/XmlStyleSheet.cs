using System.Xml;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Parsers.Xml.ProcessingInstructionReferences
{
    [ReferenceElement(Name = "xml-stylesheet")]
    [ReferenceAttribute("href")]
    public sealed class XmlStyleSheet : ProcessingInstructionReference
    {
        static ProcessingInstructionReferenceArgs FixReferenceArgs(ProcessingInstructionReferenceArgs processingInstructionReferenceArgs)
        {
            return new ProcessingInstructionReferenceArgs(processingInstructionReferenceArgs.Parser,
                processingInstructionReferenceArgs.Kind, GetMimeType(processingInstructionReferenceArgs.Attribute),
                processingInstructionReferenceArgs.Element, processingInstructionReferenceArgs.Attribute);
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
