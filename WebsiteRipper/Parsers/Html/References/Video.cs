using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [Reference("poster")]
    [Reference("src")]
    public sealed class Video : HtmlReference
    {
        public Video(Parser parser, ReferenceKind kind, HtmlNode node, HtmlAttribute attribute) : base(parser, kind, node, attribute) { }
    }
}
