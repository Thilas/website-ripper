using System;

namespace WebsiteRipper.Parsers.Html
{
    sealed class FullHtmlReferenceType
    {
        public Type Type { get; private set; }
        public ReferenceKind Kind { get; private set; }
        public string AttributeName { get; private set; }

        public FullHtmlReferenceType(Type type, ReferenceKind kind, string attributeName)
        {
            Type = type;
            Kind = kind;
            AttributeName = attributeName;
        }

        public override string ToString() { return AttributeName; }
    }
}
