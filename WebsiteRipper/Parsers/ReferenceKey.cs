using System;
using WebsiteRipper.Core;

namespace WebsiteRipper.Parsers
{
    sealed class ReferenceKey : IEquatable<ReferenceKey>
    {
        public string Name { get; private set; }
        public string Namespace { get; private set; }

        public ReferenceKey(string name, string @namespace)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
            Namespace = !string.IsNullOrEmpty(@namespace) ? @namespace : null;
            _hashCodeLazy = new Lazy<int>(() => Tools.CombineHashCodes(
                StringComparer.OrdinalIgnoreCase.GetHashCode(Name),
                Namespace != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Namespace) : 0));
        }

        public override bool Equals(object obj) { return obj is ReferenceKey && Equals((ReferenceKey)obj); }

        public bool Equals(ReferenceKey other)
        {
            return other != null &&
                string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
                (Namespace == null && other.Namespace == null || string.Equals(Namespace, other.Namespace, StringComparison.OrdinalIgnoreCase));
        }

        readonly Lazy<int> _hashCodeLazy;

        public override int GetHashCode() { return _hashCodeLazy.Value; }

        public override string ToString()
        {
            return Namespace == null ? Name : string.Format("{0} : {1}", Namespace, Name);
        }
    }
}
