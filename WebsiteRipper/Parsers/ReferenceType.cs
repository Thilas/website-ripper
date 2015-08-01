using System;

namespace WebsiteRipper.Parsers
{
    sealed class ReferenceType
    {
        public Type Type { get; private set; }
        public ReferenceKind Kind { get; private set; }
        public string AttributeName { get; private set; }

        public ReferenceType(Type type, ReferenceAttributeAttribute attribute)
        {
            Type = type;
            Kind = attribute.Kind;
            AttributeName = attribute.Name;
        }

        public override string ToString() { return AttributeName; }
    }
}
