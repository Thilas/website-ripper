using System.Xml;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Parsers.Xml.ProcessingInstructionReferences
{
    [ReferenceElement(Name = "xml-stylesheet")]
    [ReferenceAttribute("href", ArgsCreatorType = typeof(HrefArgsCreator))]
    public sealed class XmlStyleSheet : ProcessingInstructionReference
    {
        sealed class HrefArgsCreator : ReferenceArgsCreator<XmlProcessingInstruction, XmlAttribute>
        {
            static string GetMimeType(XmlAttribute attribute)
            {
                var typeAttribute = attribute.GetOwnerElement().Attributes["type"];
                return typeAttribute != null ? typeAttribute.Value : null;
            }

            public override ReferenceArgs<XmlProcessingInstruction, XmlAttribute> Create(Parser parser, ReferenceKind kind,
                XmlProcessingInstruction element, XmlAttribute attribute, ReferenceValueParser valueParser)
            {
                return new ReferenceArgs<XmlProcessingInstruction, XmlAttribute>(parser, kind, GetMimeType(attribute), element, attribute, valueParser);
            }
        }

        public XmlStyleSheet(ReferenceArgs<XmlProcessingInstruction, XmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
