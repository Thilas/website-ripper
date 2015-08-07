namespace WebsiteRipper.Parsers
{
    sealed class DefaultExtensionsReference : Reference
    {
        readonly MimeType _mimeType;
        readonly string _file;

        public DefaultExtensionsReference(Parser parser, string typeName, string subtypeName, string file)
            : base(new ReferenceArgs(parser, ReferenceKind.ExternalResource))
        {
            _mimeType = new MimeType(typeName, subtypeName);
            _file = file;
        }

        protected override string UriInternal
        {
            get { return _file; }
            set { }
        }

        new internal MimeType MimeType
        {
            get { return _mimeType; }
        }
    }
}
