using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html
{
    public abstract class HtmlReference : Reference<HtmlNode, HtmlAttribute>
    {
        internal static IEnumerable<Reference> Create(Parser parser, HtmlNode node)
        {
            return Reference<HtmlNode, HtmlAttribute>.Create(
                htmlNode => htmlNode.Name,
                htmlNode => htmlNode.Attributes,
                htmlAttribute => htmlAttribute.Name,
                parser, node);
        }

        readonly HtmlParser _htmlParser;

        protected HtmlReference(Parser parser, ReferenceKind kind, HtmlNode node, HtmlAttribute attribute)
            : base(parser, kind, node, attribute)
        {
            _htmlParser = parser as HtmlParser;
        }

        protected override Uri GetBaseUri(Resource resource)
        {
            return _htmlParser == null || _htmlParser.BaseUri == null ? resource.OriginalUri : _htmlParser.BaseUri;
        }

        protected sealed override string InternalUri
        {
            get { return HtmlEntity.DeEntitize(Attribute.Value); }
            set { Attribute.Value = HtmlEntity.Entitize(value); }
        }
    }
}
