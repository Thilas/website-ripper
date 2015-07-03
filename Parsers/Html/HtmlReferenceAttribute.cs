using System;

namespace WebsiteRipper.Parsers.Html
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class HtmlReferenceAttribute : Attribute
    {
        public string AttributeName { get; private set; }

        public ReferenceKind Kind { get; private set; }

        public HtmlReferenceAttribute(string attributeName, ReferenceKind kind = ReferenceKind.ExternalResource)
        {
            if (string.IsNullOrEmpty(attributeName)) throw new ArgumentNullException("attributeName");
            AttributeName = attributeName;
            Kind = kind;
        }
    }
}
