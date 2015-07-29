using System;
using System.Collections.Generic;
using System.Linq;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Parsers
{
    public enum ReferenceKind
    {
        Hyperlink,
        ExternalResource,
        Skip
    }

    public abstract class Reference
    {
        readonly Parser _parser;

        protected Reference(Parser parser, ReferenceKind kind)
        {
            _parser = parser;
            Kind = kind;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}): {2}", GetType().Name, Kind, Uri);
        }

        public ReferenceKind Kind { get; private set; }

        public Uri GetAbsoluteUri(Resource resource)
        {
            Uri subUri;
            return System.Uri.TryCreate(GetBaseUri(resource), Uri, out subUri) ? subUri : null;
        }

        protected virtual Uri GetBaseUri(Resource resource)
        {
            return resource.OriginalUri;
        }

        public string Uri
        {
            get { return InternalUri; }
            internal set
            {
                var oldValue = InternalUri;
                if (string.Equals(value, oldValue, StringComparison.OrdinalIgnoreCase)) return;
                InternalUri = value;
                if (!string.Equals(InternalUri, oldValue, StringComparison.OrdinalIgnoreCase)) _parser.AnyChange = true;
            }
        }

        protected abstract string InternalUri { get; set; }
    }

    public abstract class Reference<TNode, TAttribute> : Reference
    {
        static string GetNodeName(Type type)
        {
            var nodeAttribute = type.GetCustomAttribute<NodeAttribute>(false);
            return nodeAttribute != null && !string.IsNullOrEmpty(nodeAttribute.NodeName) ? nodeAttribute.NodeName : type.Name;
        }

        static readonly Lazy<Dictionary<string, IEnumerable<ReferenceType>>> _referenceTypesLazy = new Lazy<Dictionary<string, IEnumerable<ReferenceType>>>(() =>
        {
            var referenceType = typeof(Reference);
            var referenceConstructorTypes = new[] { typeof(Parser), typeof(ReferenceKind), typeof(TNode), typeof(TAttribute) };
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && referenceType.IsAssignableFrom(type) && type.GetConstructor(referenceConstructorTypes) != null)
                .SelectMany(type => type.GetCustomAttributes<ReferenceAttribute>(false)
                    .Select(referenceAttribute => new { Type = type, NodeName = GetNodeName(type), ReferenceAttribute = referenceAttribute }))
                .GroupBy(
                    reference => reference.NodeName,
                    reference => new ReferenceType(reference.Type, reference.ReferenceAttribute),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping.Distinct(ReferenceTypeComparer.Comparer).ToList().AsEnumerable(),
                    StringComparer.OrdinalIgnoreCase);
        });

        internal static IEnumerable<Reference> Create(
            Func<TNode, string> nodeNameSelector,
            Func<TNode, IEnumerable<TAttribute>> nodeAttributesSelector,
            Func<TAttribute, string> attributeNameSelector,
            Parser parser, TNode node)
        {
            IEnumerable<ReferenceType> references;
            if (!_referenceTypesLazy.Value.TryGetValue(nodeNameSelector(node), out references)) return Enumerable.Empty<Reference>();
            return nodeAttributesSelector(node).Join(references, attributeNameSelector, reference => reference.AttributeName,
                (attribute, reference) => (Reference)Activator.CreateInstance(reference.Type, parser, reference.Kind, node, attribute),
                StringComparer.OrdinalIgnoreCase);
        }

        protected TNode Node { get; private set; }
        protected TAttribute Attribute { get; private set; }

        protected Reference(Parser parser, ReferenceKind kind, TNode node, TAttribute attribute)
            : base(parser, kind)
        {
            Node = node;
            Attribute = attribute;
        }
    }
}
