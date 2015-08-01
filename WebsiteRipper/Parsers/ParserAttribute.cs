using System;

namespace WebsiteRipper.Parsers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ParserAttribute : Attribute
    {
        readonly string _mimeType;
        public string MimeType { get { return _mimeType; } }

        public ParserAttribute(string mimeType)
        {
            if (mimeType == null) throw new ArgumentNullException("mimeType");
            _mimeType = mimeType;
        }
    }
}
