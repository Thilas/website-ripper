using System;

namespace WebsiteRipper.Parsers
{
    public sealed class RelativeUriUriPair
    {
        public string RelativeUri { get; private set; }
        public Uri Uri { get; private set; }

        public RelativeUriUriPair(string relativeUri, Uri uri)
        {
            RelativeUri = relativeUri;
            Uri = uri;
        }
    }
}
