using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html
{
    public abstract class HtmlReference : Reference<HtmlNode, HtmlAttribute, HtmlReferenceArgs>
    {
        internal static IEnumerable<Reference> Create(Parser parser, HtmlNode node)
        {
            return Create(
                htmlNode => htmlNode.Name,
                htmlNode => htmlNode.Attributes,
                htmlAttribute => htmlAttribute.Name,
                parser, node);
        }

        readonly HtmlParser _htmlParser;

        protected HtmlReference(HtmlReferenceArgs htmlReferenceArgs)
            : base(htmlReferenceArgs)
        {
            _htmlParser = htmlReferenceArgs.Parser as HtmlParser;
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
