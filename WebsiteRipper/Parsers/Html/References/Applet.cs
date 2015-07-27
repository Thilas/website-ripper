using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    //[HtmlReference("archive")] // TODO: Comma-separated archive list
    [HtmlReference("code")]
    //[HtmlReference("codeBase")] // TODO: Optional base uri for applet
    public sealed class Applet : HtmlReference
    {
        public Applet(Parser parser, ReferenceKind kind, HtmlNode node, HtmlAttribute attribute) : base(parser, kind, node, attribute) { }
    }
}
