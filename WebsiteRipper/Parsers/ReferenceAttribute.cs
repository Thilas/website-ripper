using System;

namespace WebsiteRipper.Parsers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class ReferenceAttribute : Attribute
    {
        private readonly string _attributeName;
        public string AttributeName { get { return _attributeName; } }

        public ReferenceKind Kind { get; set; }

        public ReferenceAttribute(string attributeName)
        {
            if (attributeName == null) throw new ArgumentNullException("attributeName");
            _attributeName = attributeName;
            Kind = ReferenceKind.ExternalResource;
        }
    }
}
