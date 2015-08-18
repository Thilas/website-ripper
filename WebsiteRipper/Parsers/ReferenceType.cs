using System;

namespace WebsiteRipper.Parsers
{
    sealed class ReferenceType<TElement, TAttribute> : IEquatable<ReferenceType<TElement, TAttribute>>
    {
        public ReferenceKey AttributeKey { get; private set; }
        public ReferenceKind Kind { get; private set; }
        public Func<ReferenceArgs<TElement, TAttribute>, Reference<TElement, TAttribute>> Constructor { get; private set; }
        public ReferenceArgsCreator<TElement, TAttribute> ArgsCreator { get; private set; }

        public ReferenceType(ReferenceAttributeAttribute referenceAttribute, string @namespace,
            Func<ReferenceArgs<TElement, TAttribute>, Reference<TElement, TAttribute>> constructor)
        {
            AttributeKey = new ReferenceKey(referenceAttribute.Name, @namespace);
            Kind = referenceAttribute.Kind;
            Constructor = constructor;
            ArgsCreator = ReferenceArgsCreator<TElement, TAttribute>.Create(referenceAttribute.ArgsCreatorType);
        }

        public override bool Equals(object obj) { return obj is ReferenceType<TElement, TAttribute> && Equals((ReferenceType<TElement, TAttribute>)obj); }

        public bool Equals(ReferenceType<TElement, TAttribute> other) { return other != null && AttributeKey.Equals(other.AttributeKey); }

        public override int GetHashCode() { return AttributeKey.GetHashCode(); }

        public override string ToString() { return string.Format("{0} ({1})", AttributeKey, Kind); }
    }
}
