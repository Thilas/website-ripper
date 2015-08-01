using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebsiteRipper.Parsers
{
    public sealed class DefaultParser : Parser
    {
        public const string MimeType = "application/octet-stream";

        const string DefaultExtension = ".html";

        readonly Uri _uri;

        internal DefaultParser(ParserArgs parserArgs)
            : base(parserArgs)
        {
            _uri = parserArgs.Uri;
        }

        protected override string DefaultFileNameWithoutExtension { get { return "default"; } }

        protected override string GetDefaultExtension()
        {
            string defaultExtension = null;
            if ((ActualMimeType == null || !DefaultExtensions.All.TryGetDefaultExtension(ActualMimeType, out defaultExtension)) && _uri != null)
                defaultExtension = Path.GetExtension(_uri.LocalPath);
            if (string.IsNullOrEmpty(defaultExtension))
                defaultExtension = DefaultExtension;
            return defaultExtension;
        }

        protected override void Load(string path) { }

        protected override IEnumerable<Reference> GetReferences() { return Enumerable.Empty<Reference>(); }

        protected override void Save(string path) { }
    }
}
