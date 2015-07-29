using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace WebsiteRipper.Parsers.Xml
{
    public abstract class ProcessingInstructionReference : Reference<XmlProcessingInstruction, XmlAttribute>
    {
        internal static IEnumerable<Reference> Create(Parser parser, XmlProcessingInstruction node)
        {
            return Reference<XmlProcessingInstruction, XmlAttribute>.Create(
                processingInstruction => processingInstruction.Name,
                GetProcessingInstructionAttributes,
                attribute => attribute.Name,
                parser, node);
        }

        static IEnumerable<XmlAttribute> GetProcessingInstructionAttributes(XmlProcessingInstruction processingInstruction)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(string.Format("<pi {0}/>", processingInstruction.Data));
            var documentElement = xmlDocument.DocumentElement;
            if (documentElement == null)
                throw new NotSupportedException(string.Format("XmlParser does not support processing instruction \"{0}\".", processingInstruction.Name));
            return documentElement.Attributes.Cast<XmlAttribute>();
        }

        private readonly XmlElement _processingInstructionElement;

        protected ProcessingInstructionReference(Parser parser, ReferenceKind kind, XmlProcessingInstruction node, XmlAttribute attribute)
            : base(parser, kind, node, attribute)
        {
            _processingInstructionElement = attribute.OwnerElement;
        }

        protected sealed override string InternalUri
        {
            get { return Attribute.InnerText; }
            set
            {
                Attribute.InnerText = value;
                Node.Data = GetProcessingInstructionData();
            }
        }

        string GetProcessingInstructionData()
        {
            var stringBuilder = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(stringBuilder, new XmlWriterSettings() { ConformanceLevel = ConformanceLevel.Fragment }))
            {
                foreach (var attribute in _processingInstructionElement.Attributes.Cast<XmlAttribute>())
                    xmlWriter.WriteAttributeString(attribute.LocalName, attribute.Value);
            }
            return stringBuilder.ToString();
        }
    }
}
