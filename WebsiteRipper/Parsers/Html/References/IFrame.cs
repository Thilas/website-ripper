using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [HtmlReference("longDesc")]
    [HtmlReference("src")]
    public sealed class IFrame : HtmlReference
    {
        public IFrame(Parser parser, ReferenceKind kind, HtmlNode node, string attributeName) : base(parser, kind, node, attributeName) { }
    }
}
