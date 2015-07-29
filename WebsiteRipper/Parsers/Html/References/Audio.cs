using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [Reference("src")]
    public sealed class Audio : HtmlReference
    {
        public Audio(Parser parser, ReferenceKind kind, HtmlNode node, HtmlAttribute attribute) : base(parser, kind, node, attribute) { }
    }
}
