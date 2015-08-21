using System.Collections.Generic;
using HtmlAgilityPack;
using WebsiteRipper.Helpers;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("src")]
    [ReferenceAttribute("srcset", ParserType = typeof(SrcSetParser))]
    public sealed class Source : HtmlReference
    {
        sealed class SrcSetParser : ReferenceValueParser
        {
            public override IEnumerable<string> GetUriStrings(string value)
            {
                // "srcset" attribute contains comma-separated uris.
                return Helper.SplitCommaSeparatedTokens(value);
            }
        }

        public Source(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
