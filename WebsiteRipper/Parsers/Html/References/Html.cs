using System;
using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("manifest")]
    public sealed class Html : HtmlReference
    {
        public Html(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs) : base(referenceArgs) { }

        protected override Uri GetBaseUri(Resource resource)
        {
            return resource.OriginalUri;
        }
    }
}
