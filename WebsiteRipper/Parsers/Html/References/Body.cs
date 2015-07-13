using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [HtmlReference("background")]
    public sealed class Body : HtmlReference
    {
        public Body(Parser parser, ReferenceKind kind, HtmlNode node, string attributeName) : base(parser, kind, node, attributeName) { }
    }
}
