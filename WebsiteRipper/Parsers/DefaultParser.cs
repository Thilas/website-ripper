using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebsiteRipper.Core;

namespace WebsiteRipper.Parsers
{
    public sealed class DefaultParser : Parser
    {
        const string DefaultExtension = ".html";

        readonly Uri _url;

        internal DefaultParser(string mimeType, Uri url)
            : base(mimeType)
        {
            _url = url;
        }

        protected override string DefaultFileNameWithoutExtension { get { return "default"; } }

        protected override string GetDefaultExtension()
        {
            string defaultExtension;
            if (!DefaultExtensions.All.TryGetDefaultExtension(ActualMimeType, out defaultExtension) && _url != null)
                defaultExtension = Path.GetExtension(_url.LocalPath);
            if (string.IsNullOrEmpty(defaultExtension))
                defaultExtension = DefaultExtension;
            return defaultExtension;
        }

        protected override void Load(string path) { }

        protected override IEnumerable<Reference> GetReferences() { return Enumerable.Empty<Reference>(); }

        protected override void Save(string path) { }
    }
}
