using System;
using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [HtmlReference("manifest")]
    public sealed class Html : HtmlReference
    {
        public Html(Parser parser, ReferenceKind kind, HtmlNode node, string attributeName) : base(parser, kind, node, attributeName) { }

        protected override Uri GetBaseUri(Resource resource)
        {
            return resource.OriginalUri;
        }
    }
}
