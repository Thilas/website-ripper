using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    //[HtmlReference("archive")] // TODO: Comma-separated archive list
    [HtmlReference("code")]
    //[HtmlReference("codeBase")] // TODO: Optional base url for applet
    public sealed class Applet : HtmlReference
    {
        public Applet(Parser parser, ReferenceKind kind, HtmlNode node, string attributeName) : base(parser, kind, node, attributeName) { }
    }
}
