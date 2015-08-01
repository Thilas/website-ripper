using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("href", Kind = ReferenceKind.Hyperlink)]
    public sealed class A : HtmlReference
    {
        public A(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
