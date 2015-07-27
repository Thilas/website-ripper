using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html
{
    public abstract class HtmlReference : Reference
    {
        static readonly Lazy<Dictionary<string, IEnumerable<FullHtmlReferenceType>>> _htmlReferenceTypesLazy = new Lazy<Dictionary<string, IEnumerable<FullHtmlReferenceType>>>(() =>
        {
            var htmlReferenceType = typeof(HtmlReference);
            var htmlReferenceConstructorTypes = new[] { typeof(Parser), typeof(ReferenceKind), typeof(HtmlNode), typeof(string) };
            var htmlReferenceAttributeType = typeof(HtmlReferenceAttribute);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && htmlReferenceType.IsAssignableFrom(type) && type.GetConstructor(htmlReferenceConstructorTypes) != null)
                .SelectMany(type => ((HtmlReferenceAttribute[])type.GetCustomAttributes(htmlReferenceAttributeType, false))
                    .Select(htmlReferenceAttribute => new { type.Name, Type = type, htmlReferenceAttribute.Kind, htmlReferenceAttribute.AttributeName }))
                .GroupBy(
                    htmlReference => htmlReference.Name,
                    htmlReference => new FullHtmlReferenceType(htmlReference.Type, htmlReference.Kind, htmlReference.AttributeName),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping.Distinct(FullHtmlReferenceTypeComparer.Comparer).ToList().AsEnumerable(),
                    StringComparer.OrdinalIgnoreCase);
        });

        internal static IEnumerable<Reference> Create(Parser parser, HtmlNode node)
        {
            IEnumerable<FullHtmlReferenceType> fullHtmlReferences;
            if (!_htmlReferenceTypesLazy.Value.TryGetValue(node.Name, out fullHtmlReferences)) return Enumerable.Empty<Reference>();
            return fullHtmlReferences
                .Select(fullHtmlReference => (Reference)Activator.CreateInstance(fullHtmlReference.Type, parser, fullHtmlReference.Kind, node, fullHtmlReference.AttributeName));
        }

        readonly HtmlParser _htmlParser;
        readonly HtmlAttribute _attribute;

        protected HtmlReference(Parser parser, ReferenceKind kind, HtmlNode node, string attributeName)
            : base(parser, kind)
        {
            _htmlParser = parser as HtmlParser;
            _attribute = node.Attributes[attributeName];
        }

        protected override Uri GetBaseUri(Resource resource)
        {
            return _htmlParser == null || _htmlParser.BaseUri == null ? resource.OriginalUri : _htmlParser.BaseUri;
        }

        protected sealed override string InternalUri
        {
            get { return HtmlEntity.DeEntitize(EntitizedUri); }
            set { EntitizedUri = HtmlEntity.Entitize(value); }
        }

        string EntitizedUri
        {
            get { return _attribute != null ? _attribute.Value : null; }
            set { _attribute.Value = value; }
        }
    }
}
