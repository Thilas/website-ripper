using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [HtmlReference("href", ReferenceKind.Hyperlink)]
    public sealed class A : HtmlReference
    {
        public A(Parser parser, ReferenceKind kind, HtmlNode node, string attributeName) : base(parser, kind, node, attributeName) { }
    }
}
