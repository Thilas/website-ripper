using System.Xml;

namespace WebsiteRipper.Parsers.Xml
{
    sealed class XmlReference : Reference
    {
        readonly XmlAttribute _attribute;

        public XmlReference(Parser parser, XmlAttribute attribute)
            : base(parser, ReferenceKind.ExternalResource)
        {
            _attribute = attribute;
        }

        protected override string InternalUri
        {
            get { return _attribute.Value; }
            set { _attribute.Value = value; }
        }
    }
}
