using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("longDesc")]
    [ReferenceAttribute("src")]
    public sealed class Img : HtmlReference
    {
        public Img(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
