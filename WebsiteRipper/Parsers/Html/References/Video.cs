using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("poster")]
    [ReferenceAttribute("src")]
    public sealed class Video : HtmlReference
    {
        public Video(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
