namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("href", Kind = ReferenceKind.Hyperlink)]
    public sealed class A : HtmlReference
    {
        public A(HtmlReferenceArgs htmlReferenceArgs) : base(htmlReferenceArgs) { }
    }
}
