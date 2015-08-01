using System;

namespace WebsiteRipper.Parsers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class ReferenceAttributeAttribute : Attribute
    {
        readonly string _name;
        public string Name { get { return _name; } }

        public ReferenceKind Kind { get; set; }

        public ReferenceAttributeAttribute(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            _name = name;
            Kind = ReferenceKind.ExternalResource;
        }
    }
}
