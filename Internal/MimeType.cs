using System;
using System.Collections.Generic;
using System.Linq;

namespace WebsiteRipper.Internal
{
    sealed class MimeType
    {
        public string TypeName { get; private set; }
        public string SubtypeName { get; private set; }

        // TODO: Use List<string> instead for all .ToArray()
        public string[] Extensions { get; private set; }

        public MimeType(string typeName, string subtypeName)
        {
            TypeName = typeName.ToLowerInvariant();
            SubtypeName = subtypeName.ToLowerInvariant();
        }

        public override string ToString() { return string.Format("{0}/{1}", TypeName, SubtypeName); }

        public MimeType SetExtensions(string[] extensions)
        {
            if (extensions != null && extensions.Length == 0) extensions = null;
            return object.ReferenceEquals(extensions, Extensions) ? this : new MimeType(TypeName, SubtypeName) { Extensions = extensions };
        }

        public MimeType SetExtensions(IEnumerable<string> extensions)
        {
            return SetExtensions(extensions.ToArray());
        }
    }
}
