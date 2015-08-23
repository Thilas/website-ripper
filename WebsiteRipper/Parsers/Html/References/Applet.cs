using System.Collections.Generic;
using HtmlAgilityPack;
using WebsiteRipper.Helpers;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("archive", ParserType = typeof(ArchiveParser))]
    [ReferenceAttribute("code")]
    public sealed class Applet : HtmlReference
    {
        sealed class ArchiveParser : ReferenceValueParser
        {
            public override IEnumerable<string> GetUriStrings(string value)
            {
                // "archive" attribute contains comma-separated uris.
                return Helper.SplitCommaSeparatedTokens(value);
            }
        }

        public Applet(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs)
            : base(referenceArgs)
        {
            SetBaseUri(referenceArgs, "codeBase");
        }
    }
}
