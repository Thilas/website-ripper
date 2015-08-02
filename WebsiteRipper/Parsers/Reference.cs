using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        protected Reference(ReferenceArgs referenceArgs)
        {
            _parser = referenceArgs.Parser;
            Kind = referenceArgs.Kind;
            MimeType = referenceArgs.MimeType;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}): {2}", GetType().Name, Kind, Uri);
        }

        public ReferenceKind Kind { get; private set; }

        public string MimeType { get; private set; }

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

    public abstract class Reference<TNode, TAttribute, TReferenceArgs> : Reference where TReferenceArgs : ReferenceArgs<TNode, TAttribute>
    {
        static string GetNodeName(Type type)
        {
            var referenceNode = type.GetCustomAttribute<ReferenceNodeAttribute>(false);
            return referenceNode != null && !string.IsNullOrEmpty(referenceNode.Name) ? referenceNode.Name : type.Name;
        }

        static readonly Lazy<Dictionary<string, IEnumerable<ReferenceType>>> _referenceTypesLazy = new Lazy<Dictionary<string, IEnumerable<ReferenceType>>>(() =>
        {
            var referenceType = typeof(Reference);
            var referenceConstructorTypes = new[] { typeof(TReferenceArgs) };
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && referenceType.IsAssignableFrom(type) && type.GetConstructor(referenceConstructorTypes) != null)
                .SelectMany(type => type.GetCustomAttributes<ReferenceAttributeAttribute>(false)
                    .Select(attribute => new { Type = type, NodeName = GetNodeName(type), Attribute = attribute }))
                .GroupBy(
                    reference => reference.NodeName,
                    reference => new ReferenceType(reference.Type, reference.Attribute),
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
                (attribute, reference) => (Reference)Activator.CreateInstance(reference.Type, CreateReferenceArgs(parser, reference.Kind, null, node, attribute)),
                StringComparer.OrdinalIgnoreCase);
        }

        static readonly Lazy<ConstructorInfo> _referenceArgsConstructorLazy = new Lazy<ConstructorInfo>(() =>
        {
            var referenceArgsConstructorTypes = new[] { typeof(Parser), typeof(ReferenceKind), typeof(string), typeof(TNode), typeof(TAttribute) };
            var tReferenceArgsConstructor = typeof(TReferenceArgs).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, referenceArgsConstructorTypes, null);
            if (tReferenceArgsConstructor == null) throw new NotSupportedException("");
            return tReferenceArgsConstructor;
        });

        static TReferenceArgs CreateReferenceArgs(Parser parser, ReferenceKind kind, string mimeType, TNode node, TAttribute attribute)
        {
            // TODO: Replace by a dedicated method via interface?
            return (TReferenceArgs)_referenceArgsConstructorLazy.Value.Invoke(new object[] { parser, kind, mimeType, node, attribute });
        }

        protected TNode Node { get; private set; }
        protected TAttribute Attribute { get; private set; }

        protected Reference(ReferenceArgs<TNode, TAttribute> referenceArgs)
            : base(referenceArgs)
        {
            Node = referenceArgs.Node;
            Attribute = referenceArgs.Attribute;
        }
    }
}
