using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("src")]
    public sealed class Script : HtmlReference
    {
        public Script(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
