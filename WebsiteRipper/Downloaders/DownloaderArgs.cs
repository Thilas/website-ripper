using System;

namespace WebsiteRipper.Downloaders
{
    public sealed class DownloaderArgs
    {
        public Uri Uri { get; private set; }
        public string MimeType { get; private set; }
        public int Timeout { get; private set; }
        public string PreferredLanguages { get; private set; }

        internal DownloaderArgs(Uri uri, string mimeType, int timeout, string preferredLanguages)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (preferredLanguages == null) throw new ArgumentNullException("preferredLanguages");
            Uri = uri;
            MimeType = mimeType;
            Timeout = timeout;
            PreferredLanguages = preferredLanguages;
        }
    }
}
