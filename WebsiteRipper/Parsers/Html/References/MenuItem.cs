using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [Reference("icon")]
    public sealed class MenuItem : HtmlReference
    {
        public MenuItem(Parser parser, ReferenceKind kind, HtmlNode node, HtmlAttribute attribute) : base(parser, kind, node, attribute) { }
    }
}
