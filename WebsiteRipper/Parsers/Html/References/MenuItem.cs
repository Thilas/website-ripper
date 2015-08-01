using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("icon")]
    public sealed class MenuItem : HtmlReference
    {
        public MenuItem(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
