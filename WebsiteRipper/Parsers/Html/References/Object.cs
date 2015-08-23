using System.Collections.Generic;
using HtmlAgilityPack;
using WebsiteRipper.Helpers;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("archive", ParserType = typeof(ArchiveParser))]
    [ReferenceAttribute("classId")]
    [ReferenceAttribute("data")]
    public sealed class Object : HtmlReference
    {
        sealed class ArchiveParser : ReferenceValueParser
        {
            public override IEnumerable<string> GetUriStrings(string value)
            {
                // "archive" attribute contains space-separated uris.
                return Helper.SplitSpaceSeparatedTokens(value);
            }
        }

        public Object(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs)
            : base(referenceArgs)
        {
            SetBaseUri(referenceArgs, "codeBase");
        }
    }
}
