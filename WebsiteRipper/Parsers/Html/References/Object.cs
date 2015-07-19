using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    //[HtmlReference("archive")] // TODO: Space-separated list of uris
    [HtmlReference("classId")]
    //[HtmlReference("codeBase")] // TODO: Base uri for archive, classId & data
    [HtmlReference("data")]
    public sealed class Object : HtmlReference
    {
        public Object(Parser parser, ReferenceKind kind, HtmlNode node, string attributeName) : base(parser, kind, node, attributeName) { }
    }
}
