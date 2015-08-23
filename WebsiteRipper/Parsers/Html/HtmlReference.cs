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
        Uri _baseUri = null;

        protected HtmlReference(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs)
            : base(referenceArgs)
        {
            _htmlParser = referenceArgs.Parser as HtmlParser;
        }

        protected void SetBaseUri(ReferenceArgs<HtmlNode, HtmlAttribute> referenceArgs, string attributeName)
        {
            var baseUriAttribute = referenceArgs.Element.Attributes[attributeName];
            if (baseUriAttribute == null) return;
            if (!Uri.TryCreate(baseUriAttribute.Value, UriKind.Absolute, out _baseUri)) _baseUri = null;
            if (_htmlParser != null) _htmlParser.Remove(baseUriAttribute);
        }

        protected override Uri GetBaseUri(Resource resource)
        {
            if (_baseUri != null) return _baseUri;
            return _htmlParser == null || _htmlParser.BaseUri == null ? resource.OriginalUri : _htmlParser.BaseUri;
        }

        protected sealed override string ValueInternal
        {
            get { return HtmlEntity.DeEntitize(Attribute.Value); }
            set { Attribute.Value = HtmlEntity.Entitize(value); }
        }
    }
}
