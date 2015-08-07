using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("longDesc")]
    [ReferenceAttribute("src")]
    public sealed class Frame : HtmlReference
    {
        public Frame(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
