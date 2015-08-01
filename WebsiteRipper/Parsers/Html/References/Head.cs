using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("profile")]
    public sealed class Head : HtmlReference
    {
        public Head(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
