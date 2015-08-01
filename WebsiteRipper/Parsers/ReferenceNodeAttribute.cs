using System;

namespace WebsiteRipper.Parsers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    sealed class ReferenceNodeAttribute : Attribute
    {
        readonly string _name;
        public string Name { get { return _name; } }

        public ReferenceNodeAttribute(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            _name = name;
        }
    }
}
