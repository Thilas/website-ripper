using System;
using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [HtmlReference("manifest")]
    public sealed class Html : HtmlReference
    {
        public Html(Parser parser, ReferenceKind kind, HtmlNode node, HtmlAttribute attribute) : base(parser, kind, node, attribute) { }

        protected override Uri GetBaseUri(Resource resource)
        {
            return resource.OriginalUri;
        }
    }
}
