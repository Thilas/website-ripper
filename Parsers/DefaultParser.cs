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

        readonly string _mimeType;
        readonly Uri _url;

        internal DefaultParser(string mimeType, Uri url)
        {
            _mimeType = mimeType;
            _url = url;
        }

        public override string DefaultFile { get { return Path.ChangeExtension("default", GetDefaultExtension()); } }

        string GetDefaultExtension()
        {
            string defaultExtension = null;
            if ((string.IsNullOrEmpty(_mimeType) || !DefaultExtensions.All.TryGetDefaultExtension(_mimeType, out defaultExtension)) && _url != null)
                defaultExtension = Path.GetExtension(_url.LocalPath);
            if (string.IsNullOrEmpty(defaultExtension)) defaultExtension = DefaultExtension;
            return defaultExtension;
        }

        protected override void Load(string path) { }

        protected override IEnumerable<Reference> GetReferences() { return Enumerable.Empty<Reference>(); }

        protected override void Save(string path) { }
    }
}
