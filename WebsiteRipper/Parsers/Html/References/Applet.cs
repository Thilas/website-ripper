namespace WebsiteRipper.Parsers.Html.References
{
    //[ReferenceAttribute("archive")] // TODO: Comma-separated archive list
    [ReferenceAttribute("code")]
    //[ReferenceAttribute("codeBase")] // TODO: Optional base uri for applet
    public sealed class Applet : HtmlReference
    {
        public Applet(HtmlReferenceArgs htmlReferenceArgs) : base(htmlReferenceArgs) { }
    }
}
