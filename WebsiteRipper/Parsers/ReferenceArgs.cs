using System;

namespace WebsiteRipper.Parsers
{
    public class ReferenceArgs
    {
        public Parser Parser { get; private set; }
        public ReferenceKind Kind { get; private set; }
        public string MimeType { get; private set; }

        internal ReferenceArgs(Parser parser, ReferenceKind kind, string mimeType = null)
        {
            if (parser == null) throw new ArgumentNullException("parser");
            Parser = parser;
            Kind = kind;
            MimeType = !string.IsNullOrEmpty(mimeType) ? mimeType : null;
        }
    }

    public abstract class ReferenceArgs<TElement, TAttribute> : ReferenceArgs
    {
        public TElement Element { get; private set; }
        public TAttribute Attribute { get; private set; }

        protected ReferenceArgs(Parser parser, ReferenceKind kind, string mimeType, TElement element, TAttribute attribute)
            : base(parser, kind, mimeType)
        {
            if (element == null) throw new ArgumentNullException("element");
            if (attribute == null) throw new ArgumentNullException("attribute");
            Element = element;
            Attribute = attribute;
        }
    }
}
