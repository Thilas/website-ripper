using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html
{
    public sealed class HtmlReferenceArgs : ReferenceArgs<HtmlNode, HtmlAttribute>
    {
        public HtmlReferenceArgs(Parser parser, ReferenceKind kind, string mimeType, HtmlNode node, HtmlAttribute attribute)
            : base(parser, kind, mimeType, node, attribute) { }

        public HtmlNode Node { get { return Element; } }
    }
}
