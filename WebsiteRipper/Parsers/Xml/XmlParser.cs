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

        protected override string DefaultFileNameWithoutExtension { get { return "document"; } }

        public XmlParser(string mimeType) : base(mimeType) { }

        protected XmlDocument XmlDocument { get; private set; }

        protected sealed override void Load(string path)
        {
            XmlDocument = new XmlDocument();
            XmlDocument.Load(path);
        }

        protected override IEnumerable<Reference> GetReferences()
        {
            // TODO: Xml parser
            //var a = _xmlDocument.DocumentType;
            //var b = _xmlDocument.OfType<XmlProcessingInstruction>();
            //.Where(xmlProcessingInstruction => string.Equals(xmlProcessingInstruction.LocalName, "xml-stylesheet"));
            return Enumerable.Empty<Reference>();
            //return _xmlDocument.ChildNodes.SelectMany(node => XmlReference.Create(this, node));
        }

        protected sealed override void Save(string path)
        {
            XmlDocument.Save(path);
        }
    }
}
