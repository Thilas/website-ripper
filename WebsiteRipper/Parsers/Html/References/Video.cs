using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [HtmlReference("poster")]
    [HtmlReference("src")]
    public sealed class Video : HtmlReference
    {
        public Video(Parser parser, ReferenceKind kind, HtmlNode node, string attributeName) : base(parser, kind, node, attributeName) { }
    }
}
