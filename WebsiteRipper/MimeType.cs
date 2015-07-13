using System;
using System.Collections.Generic;
using System.Linq;

namespace WebsiteRipper
{
    public sealed class MimeType
    {
        public string TypeName { get; private set; }
        public string SubtypeName { get; private set; }

        public IEnumerable<string> Extensions { get; private set; }

        internal MimeType(string typeName, string subtypeName)
        {
            TypeName = typeName.ToLowerInvariant();
            SubtypeName = subtypeName.ToLowerInvariant();
        }

        public override string ToString() { return string.Format("{0}/{1}", TypeName, SubtypeName); }

        internal MimeType SetExtensions(IEnumerable<string> extensions)
        {
            if (extensions != null && extensions.Count() == 0) extensions = null;
            return object.ReferenceEquals(extensions, Extensions) ? this : new MimeType(TypeName, SubtypeName) { Extensions = extensions };
        }
    }
}
