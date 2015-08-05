using System;

namespace WebsiteRipper.Parsers
{
    sealed class ReferenceType : IEquatable<ReferenceType>
    {
        public ReferenceKey AttributeKey { get; private set; }
        public ReferenceKind Kind { get; private set; }
        public Type Type { get; private set; }

        public ReferenceType(ReferenceAttributeAttribute referenceAttribute, string @namespace, Type type)
        {
            AttributeKey = new ReferenceKey(referenceAttribute.Name, @namespace);
            Kind = referenceAttribute.Kind;
            Type = type;
        }

        public override bool Equals(object obj) { return obj is ReferenceType && Equals((ReferenceType)obj); }

        public bool Equals(ReferenceType other) { return other != null && AttributeKey.Equals(other.AttributeKey); }

        public override int GetHashCode() { return AttributeKey.GetHashCode(); }

        public override string ToString() { return string.Format("{0} ({1})", AttributeKey, Kind); }
    }
}
