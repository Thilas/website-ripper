using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [Reference("longDesc")]
    [Reference("src")]
    public sealed class IFrame : HtmlReference
    {
        public IFrame(Parser parser, ReferenceKind kind, HtmlNode node, HtmlAttribute attribute) : base(parser, kind, node, attribute) { }
    }
}
