using System;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("manifest")]
    public sealed class Html : HtmlReference
    {
        public Html(HtmlReferenceArgs htmlReferenceArgs) : base(htmlReferenceArgs) { }

        protected override Uri GetBaseUri(Resource resource)
        {
            return resource.OriginalUri;
        }
    }
}
