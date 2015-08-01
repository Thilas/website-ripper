using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    //[HtmlReference("archive")] // TODO: Comma-separated archive list
    [ReferenceAttribute("code")]
    //[HtmlReference("codeBase")] // TODO: Optional base uri for applet
    public sealed class Applet : HtmlReference
    {
        public Applet(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
