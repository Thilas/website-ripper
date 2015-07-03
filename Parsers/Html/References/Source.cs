using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [HtmlReference("src")]
    //[HtmlReference("srcset")] // TODO: Comma-separated list indicating a set of possible images
    public sealed class Source : HtmlReference
    {
        public Source(Parser parser, ReferenceKind kind, HtmlNode node, string attributeName) : base(parser, kind, node, attributeName) { }
    }
}
