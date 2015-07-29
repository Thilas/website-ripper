using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [Reference("profile")]
    public sealed class Head : HtmlReference
    {
        public Head(Parser parser, ReferenceKind kind, HtmlNode node, HtmlAttribute attribute) : base(parser, kind, node, attribute) { }
    }
}
