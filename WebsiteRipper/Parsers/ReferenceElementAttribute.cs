using System;

namespace WebsiteRipper.Parsers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    sealed class ReferenceElementAttribute : Attribute
    {
        public bool Any { get; set; }

        public string Name { get; set; }

        public string Namespace { get; set; }

        public bool QualifiedAttributes { get; set; }
    }
}
