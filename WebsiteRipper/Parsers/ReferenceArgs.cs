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

    public class ReferenceArgs<TNode, TAttribute> : ReferenceArgs
    {
        public TNode Node { get; private set; }
        public TAttribute Attribute { get; private set; }

        internal ReferenceArgs(Parser parser, ReferenceKind kind, string mimeType, TNode node, TAttribute attribute)
            : base(parser, kind, mimeType)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (attribute == null) throw new ArgumentNullException("attribute");
            Node = node;
            Attribute = attribute;
        }
    }
}
