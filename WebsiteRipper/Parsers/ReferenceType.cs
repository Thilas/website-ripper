using System;

namespace WebsiteRipper.Parsers
{
    sealed class ReferenceType
    {
        public Type Type { get; private set; }
        public ReferenceKind Kind { get; private set; }
        public string AttributeName { get; private set; }

        public ReferenceType(Type type, ReferenceAttribute referenceAttribute)
        {
            Type = type;
            Kind = referenceAttribute.Kind;
            AttributeName = referenceAttribute.AttributeName;
        }

        public override string ToString() { return AttributeName; }
    }
}
