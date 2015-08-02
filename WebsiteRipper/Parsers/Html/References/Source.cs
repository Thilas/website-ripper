namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("src")]
    //[ReferenceAttribute("srcset")] // TODO: Comma-separated list indicating a set of possible images
    public sealed class Source : HtmlReference
    {
        public Source(HtmlReferenceArgs htmlReferenceArgs) : base(htmlReferenceArgs) { }
    }
}
