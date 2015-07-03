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
            return string.Format("{0} ({1}): {2}", GetType().Name, Kind, Url);
        }

        public ReferenceKind Kind { get; private set; }

        public Uri GetAbsoluteUrl(Resource resource)
        {
            Uri subUrl;
            return Uri.TryCreate(GetBaseUrl(resource), Url, out subUrl) ? subUrl : null;
        }

        protected virtual Uri GetBaseUrl(Resource resource)
        {
            return resource.OriginalUrl;
        }

        public string Url
        {
            get { return InternalUrl; }
            internal set
            {
                var oldValue = InternalUrl;
                if (string.Equals(value, oldValue, StringComparison.OrdinalIgnoreCase)) return;
                InternalUrl = value;
                if (!string.Equals(InternalUrl, oldValue, StringComparison.OrdinalIgnoreCase)) _parser.AnyChange = true;
            }
        }

        protected abstract string InternalUrl { get; set; }
    }
}
