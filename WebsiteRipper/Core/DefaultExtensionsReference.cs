using System.Xml;
using WebsiteRipper.Parsers;

namespace WebsiteRipper.Core
{
    sealed class DefaultExtensionsReference : Reference
    {
        readonly MimeType _mimeType;
        readonly XmlText _file;

        public DefaultExtensionsReference(Parser parser, string typeName, string subtypeName, XmlText file)
            : base(parser, ReferenceKind.ExternalResource)
        {
            _mimeType = new MimeType(typeName, subtypeName);
            _file = file;
        }

        protected override string InternalUrl
        {
            get { return _file.Value; }
            set { }
        }

        internal MimeType MimeType
        {
            get { return _mimeType; }
        }
    }
}
