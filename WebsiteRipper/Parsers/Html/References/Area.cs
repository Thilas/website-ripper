using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [Reference("href", Kind = ReferenceKind.Hyperlink)]
    public sealed class Area : HtmlReference
    {
        public Area(Parser parser, ReferenceKind kind, HtmlNode node, HtmlAttribute attribute) : base(parser, kind, node, attribute) { }
    }
}
