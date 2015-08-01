using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("longDesc")]
    [ReferenceAttribute("src")]
    public sealed class Frame : HtmlReference
    {
        public Frame(Parser parser, ReferenceKind kind, HtmlNode node, HtmlAttribute attribute) : base(parser, kind, node, attribute) { }
    }
}
