using System;

namespace WebsiteRipper.Parsers
{
    public sealed class ParserArgs
    {
        public string MimeType { get; private set; }
        public Uri Uri { get; private set; }

        internal ParserArgs(string mimeType)
            : this(mimeType, null)
        {
            if (mimeType == null) throw new ArgumentNullException("mimeType");
        }

        internal ParserArgs(string mimeType, Uri uri)
        {
            MimeType = mimeType;
            Uri = uri;
        }
    }
}
