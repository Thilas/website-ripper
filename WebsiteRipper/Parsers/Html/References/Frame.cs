using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [HtmlReference("longDesc")]
    [HtmlReference("src")]
    public sealed class Frame : HtmlReference
    {
        public Frame(Parser parser, ReferenceKind kind, HtmlNode node, string attributeName) : base(parser, kind, node, attributeName) { }
    }
}
