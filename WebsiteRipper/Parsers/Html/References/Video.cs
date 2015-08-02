namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("poster")]
    [ReferenceAttribute("src")]
    public sealed class Video : HtmlReference
    {
        public Video(HtmlReferenceArgs htmlReferenceArgs) : base(htmlReferenceArgs) { }
    }
}
