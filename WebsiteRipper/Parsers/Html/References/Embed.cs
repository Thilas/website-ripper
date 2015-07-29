using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [Reference("src")]
    public sealed class Embed : HtmlReference
    {
        public Embed(Parser parser, ReferenceKind kind, HtmlNode node, HtmlAttribute attribute) : base(parser, kind, node, attribute) { }
    }
}
