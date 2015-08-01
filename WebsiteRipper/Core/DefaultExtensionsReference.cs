using WebsiteRipper.Parsers;

namespace WebsiteRipper.Core
{
    sealed class DefaultExtensionsReference : Reference
    {
        readonly MimeType _mimeType;
        readonly string _file;

        public DefaultExtensionsReference(Parser parser, string typeName, string subtypeName, string file)
            : base(parser, ReferenceKind.ExternalResource)
        {
            _mimeType = new MimeType(typeName, subtypeName);
            _file = file;
        }

        protected override string InternalUri
        {
            get { return _file; }
            set { }
        }

        internal MimeType MimeType
        {
            get { return _mimeType; }
        }
    }
}
