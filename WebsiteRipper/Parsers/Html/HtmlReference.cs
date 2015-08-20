using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html
{
    public abstract class HtmlReference : Reference<HtmlNode, HtmlAttribute>
    {
        internal static IEnumerable<Reference> Create(Parser parser, HtmlNode node)
        {
            return Create(parser, node,
                htmlNode => htmlNode.Name,
                htmlNode => htmlNode.Attributes,
                htmlAttribute => htmlAttribute.Name);
        }

        readonly HtmlParser _htmlParser;

        protected HtmlReference(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs)
            : base(referenceArgs)
        {
            _htmlParser = referenceArgs.Parser as HtmlParser;
        }

        protected override Uri GetBaseUri(Resource resource)
        {
            return _htmlParser == null || _htmlParser.BaseUri == null ? resource.OriginalUri : _htmlParser.BaseUri;
        }

        protected sealed override string ValueInternal
        {
            get { return HtmlEntity.DeEntitize(Attribute.Value); }
            set { Attribute.Value = HtmlEntity.Entitize(value); }
        }
    }
}
