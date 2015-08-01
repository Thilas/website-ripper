using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("longDesc")]
    [ReferenceAttribute("src")]
    public sealed class Iframe : HtmlReference
    {
        public Iframe(Parser parser, ReferenceKind kind, HtmlNode node, HtmlAttribute attribute) : base(parser, kind, node, attribute) { }
    }
}
