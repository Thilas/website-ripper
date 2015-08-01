using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    //[HtmlReference("archive")] // TODO: Space-separated list of uris
    [ReferenceAttribute("classId")]
    //[HtmlReference("codeBase")] // TODO: Base uri for archive, classId & data
    [ReferenceAttribute("data")]
    public sealed class Object : HtmlReference
    {
        public Object(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
