using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WebsiteRipper.Parsers.Xml
{
    [Parser(ApplicationMimeType)]
    [Parser(TextMimeType)]
    public class XmlParser : Parser
    {
        public const string ApplicationMimeType = "application/xml";
        public const string TextMimeType = "text/xml";
        public const string MimeType = TextMimeType;

        protected sealed override string DefaultFileNameWithoutExtension { get { return "document"; } }

        public XmlParser(string mimeType) : base(mimeType) { }

        protected XmlDocument XmlDocument { get; private set; }

        protected sealed override void Load(string path)
        {
            XmlDocument = new XmlDocument();
            XmlDocument.Load(path);
        }

        protected override IEnumerable<Reference> GetReferences()
        {
            return XmlDocument.OfType<XmlProcessingInstruction>()
                .SelectMany(processingInstruction => ProcessingInstructionReference.Create(this, processingInstruction));
        }

        protected sealed override void Save(string path)
        {
            XmlDocument.Save(path);
        }
    }
}
