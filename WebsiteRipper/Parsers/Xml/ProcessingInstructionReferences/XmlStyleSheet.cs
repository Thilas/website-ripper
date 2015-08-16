using System.Xml;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Parsers.Xml.ProcessingInstructionReferences
{
    [ReferenceElement(Name = "xml-stylesheet")]
    [ReferenceAttribute("href")]
    public sealed class XmlStyleSheet : ProcessingInstructionReference
    {
        // TODO Manage a generic FixReferenceArgs method
        static ReferenceArgs<XmlProcessingInstruction, XmlAttribute> FixReferenceArgs(ReferenceArgs<XmlProcessingInstruction, XmlAttribute> referenceArgs)
        {
            return new ReferenceArgs<XmlProcessingInstruction, XmlAttribute>(referenceArgs.Parser, referenceArgs.Kind,
                GetMimeType(referenceArgs.Attribute), referenceArgs.Element, referenceArgs.Attribute);
        }

        static string GetMimeType(XmlAttribute attribute)
        {
            var typeAttribute = attribute.GetOwnerElement().Attributes["type"];
            return typeAttribute != null ? typeAttribute.Value : null;
        }

        public XmlStyleSheet(ReferenceArgs<XmlProcessingInstruction, XmlAttribute> referenceArgs) : base(FixReferenceArgs(referenceArgs)) { }
    }
}
