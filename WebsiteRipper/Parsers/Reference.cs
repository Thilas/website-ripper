using System;

namespace WebsiteRipper.Parsers
{
    public enum ReferenceKind
    {
        Hyperlink,
        ExternalResource,
        Skip
    }

    public abstract class Reference
    {
        readonly Parser _parser;

        protected Reference(Parser parser, ReferenceKind kind)
        {
            _parser = parser;
            Kind = kind;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}): {2}", GetType().Name, Kind, Uri);
        }

        public ReferenceKind Kind { get; private set; }

        public Uri GetAbsoluteUri(Resource resource)
        {
            Uri subUri;
            return System.Uri.TryCreate(GetBaseUri(resource), Uri, out subUri) ? subUri : null;
        }

        protected virtual Uri GetBaseUri(Resource resource)
        {
            return resource.OriginalUri;
        }

        public string Uri
        {
            get { return InternalUri; }
            internal set
            {
                var oldValue = InternalUri;
                if (string.Equals(value, oldValue, StringComparison.OrdinalIgnoreCase)) return;
                InternalUri = value;
                if (!string.Equals(InternalUri, oldValue, StringComparison.OrdinalIgnoreCase)) _parser.AnyChange = true;
            }
        }

        protected abstract string InternalUri { get; set; }
    }
}
