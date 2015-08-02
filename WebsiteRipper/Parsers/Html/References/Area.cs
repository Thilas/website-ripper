namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("href", Kind = ReferenceKind.Hyperlink)]
    public sealed class Area : HtmlReference
    {
        public Area(HtmlReferenceArgs htmlReferenceArgs) : base(htmlReferenceArgs) { }
    }
}
