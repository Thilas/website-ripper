using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [Reference("href", Kind = ReferenceKind.Hyperlink)]
    public sealed class A : HtmlReference
    {
        public A(Parser parser, ReferenceKind kind, HtmlNode node, HtmlAttribute attribute) : base(parser, kind, node, attribute) { }
    }
}
