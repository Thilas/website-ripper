using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Parsers.Xml
{
    public abstract class ProcessingInstructionReference : Reference<XmlProcessingInstruction, XmlAttribute, ProcessingInstructionReferenceArgs>
    {
        internal static IEnumerable<Reference> Create(Parser parser, XmlProcessingInstruction node)
        {
            return Create(
                xmlProcessingInstruction => xmlProcessingInstruction.Name,
                GetProcessingInstructionAttributes,
                xmlAttribute => xmlAttribute.Name,
                parser, node);
        }

        static IEnumerable<XmlAttribute> GetProcessingInstructionAttributes(XmlProcessingInstruction xmlProcessingInstruction)
        {
            var document = new XmlDocument();
            try
            {
                document.LoadXml(string.Format("<ProcessingInstruction {0}/>", xmlProcessingInstruction.Data));
            }
            catch (Exception exception)
            {
                throw new NotSupportedException(string.Format("XmlParser does not support processing instruction \"{0}\".", xmlProcessingInstruction.Name), exception);
            }
            return document.GetDocumentElement().Attributes.Cast<XmlAttribute>();
        }

        readonly XmlElement _processingInstructionElement;

        protected ProcessingInstructionReference(ProcessingInstructionReferenceArgs processingInstructionReferenceArgs)
            : base(processingInstructionReferenceArgs)
        {
            _processingInstructionElement = processingInstructionReferenceArgs.Attribute.GetOwnerElement();
        }

        protected sealed override string InternalUri
        {
            get { return Attribute.Value; }
            set
            {
                Attribute.Value = value;
                Node.Data = GetProcessingInstructionData();
            }
        }

        string GetProcessingInstructionData()
        {
            var stringBuilder = new StringBuilder();
            using (var writer = XmlWriter.Create(stringBuilder, new XmlWriterSettings() { ConformanceLevel = ConformanceLevel.Fragment }))
            {
                foreach (var attribute in _processingInstructionElement.Attributes.Cast<XmlAttribute>())
                    writer.WriteAttributeString(attribute.LocalName, attribute.Value);
            }
            return stringBuilder.ToString();
        }
    }
}
