using System;

namespace WebsiteRipper.Parsers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    sealed class NodeAttribute : Attribute
    {
        private readonly string _nodeName;
        public string NodeName { get { return _nodeName; } }

        public NodeAttribute(string nodeName)
        {
            if (nodeName == null) throw new ArgumentNullException("nodeName");
            _nodeName = nodeName;
        }
    }
}
