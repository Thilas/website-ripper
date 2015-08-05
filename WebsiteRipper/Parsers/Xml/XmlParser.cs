using System.Collections.Generic;
using System.Linq;
using System.Xml;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Parsers.Xml
{
    [Parser(ApplicationMimeType)]
    [Parser(TextMimeType)]
    [Parser(XsltApplicationMimeType)]
    [Parser(XsltTextMimeType)]
    public class XmlParser : Parser
    {
        internal const string ApplicationMimeType = "application/xml";
        internal const string TextMimeType = "text/xml";
        public const string MimeType = TextMimeType;

        internal const string XsdNamespace = "http://www.w3.org/2001/XMLSchema";

        internal const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

        internal const string XsltApplicationMimeType = "application/xslt+xml";
        internal const string XsltTextMimeType = "text/xsl";
        internal const string XsltNamespace = "http://www.w3.org/1999/XSL/Transform";

        protected sealed override string DefaultFileNameWithoutExtension { get { return "document"; } }

        public XmlParser(ParserArgs parserArgs) : base(parserArgs) { }

        protected XmlDocument Document { get; private set; }

        protected sealed override void Load(string path)
        {
            Document = new XmlDocument() { XmlResolver = null };
            Document.Load(path);
        }

        protected override IEnumerable<Reference> GetReferences()
        {
            return Document.OfType<XmlProcessingInstruction>()
                .SelectMany(processingInstruction => ProcessingInstructionReference.Create(this, processingInstruction))
                .Prepend(new DocumentTypeReference(this, Document))
                .Concat(Document.GetDocumentElement().DescendantsAndSelf().SelectMany(element => XmlReference.Create(this, element)));
        }

        protected sealed override void Save(string path)
        {
            Document.Save(path);
        }
    }
}
