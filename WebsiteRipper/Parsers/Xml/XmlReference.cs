using System.Xml;
using WebsiteRipper.Parsers;

namespace WebsiteRipper.Parsers.Xml
{
    sealed class XmlReference : Reference
    {
        readonly XmlNode _node;

        public XmlReference(Parser parser, XmlNode node)
            : base(parser, ReferenceKind.ExternalResource)
        {
            _node = node;
        }

        protected override string InternalUri
        {
            get { return _node.Value; }
            set { _node.Value = value; }
        }
    }
}
