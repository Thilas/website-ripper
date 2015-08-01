using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("src")]
    //[HtmlReference("srcset")] // TODO: Comma-separated list indicating a set of possible images
    public sealed class Source : HtmlReference
    {
        public Source(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
