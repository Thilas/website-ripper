using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace WebsiteRipper.Parsers.Xml
{
    [Parser("application/xml")]
    [Parser("text/xml")]
    public class XmlParser : Parser
    {
        public override string DefaultFile { get { return "document.xml"; } }

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
