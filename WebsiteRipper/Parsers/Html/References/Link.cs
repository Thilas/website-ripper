using System;
using System.Linq;
using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html.References
{
    [ReferenceAttribute("href", Kind = ReferenceKind.Hyperlink, ArgsCreatorType = typeof(HrefArgsCreator))]
    public sealed class Link : HtmlReference
    {
        // TODO Finish link node support depending on rel attribute
        static readonly string[] _externalResourceRelationships = { "icon", "pingback", "prefetch", "stylesheet" };
        static readonly string[] _skipRelationships = { "dns-prefetch" };

        sealed class HrefArgsCreator : ReferenceArgsCreator<HtmlNode, HtmlAttribute>
        {
            static ReferenceKind GetKind(HtmlNode element, ReferenceKind defaultKind)
            {
                const char listSeparatorChar = ' ';
                var relationshipAttribute = element.Attributes["rel"];
                if (relationshipAttribute == null || string.IsNullOrEmpty(relationshipAttribute.Value)) return defaultKind;
                var relationships = relationshipAttribute.Value.Split(listSeparatorChar);
                if (relationships.Any(relationship => _externalResourceRelationships.Any(type => string.Equals(relationship, type, StringComparison.OrdinalIgnoreCase))))
                    return ReferenceKind.ExternalResource;
                if (relationships.Any(relationship => _skipRelationships.Any(type => string.Equals(relationship, type, StringComparison.OrdinalIgnoreCase))))
                    return ReferenceKind.Skip;
                return defaultKind;
            }

            public override ReferenceArgs<HtmlNode, HtmlAttribute> Create(Parser parser, ReferenceKind kind, HtmlNode element, HtmlAttribute attribute)
            {
                return new ReferenceArgs<HtmlNode, HtmlAttribute>(parser, GetKind(element, kind), null, element, attribute);
            }
        }

        public Link(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
