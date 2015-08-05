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

    public abstract class Reference<TElement, TAttribute, TReferenceArgs> : Reference where TReferenceArgs : ReferenceArgs<TElement, TAttribute>
    {
        static IEnumerable<ReferenceType> _anyReferences;
        static readonly Lazy<Dictionary<ReferenceKey, IEnumerable<ReferenceType>>> _referencesLazy = new Lazy<Dictionary<ReferenceKey, IEnumerable<ReferenceType>>>(() =>
        {
            _anyReferences = new List<ReferenceType>();
            var referenceType = typeof(Reference);
            var referenceConstructorTypes = new[] { typeof(TReferenceArgs) };
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && referenceType.IsAssignableFrom(type) && type.GetConstructor(referenceConstructorTypes) != null)
                .SelectMany(type => type.GetCustomAttributes<ReferenceAttributeAttribute>(false)
                    .Select(referenceAttribute =>
                    {
                        var referenceElement = type.GetCustomAttribute<ReferenceElementAttribute>(false);
                        var @namespace = referenceElement != null ? referenceElement.Namespace : null;
                        var qualifiedAttributes = referenceElement != null && referenceElement.QualifiedAttributes;
                        var reference = new ReferenceType(referenceAttribute, qualifiedAttributes ? @namespace : null, type);
                        if (referenceElement != null && referenceElement.Any)
                        {
                            ((List<ReferenceType>)_anyReferences).Add(reference);
                            return null;
                        }
                        var elementName = referenceElement != null && !string.IsNullOrEmpty(referenceElement.Name) ? referenceElement.Name : type.Name;
                        return new { Key = new ReferenceKey(elementName, @namespace), Reference = reference };
                    }).Where(pair => pair != null))
                .GroupBy(
                    pair => pair.Key,
                    pair => pair.Reference)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping.Distinct().ToList().AsEnumerable()); // TODO: Review duplicate references management
            var anyReferences = _anyReferences.Distinct().ToList(); // TODO: Review duplicate any references management
            _anyReferences = anyReferences.Count > 0 ? anyReferences : null;
            return references;
        });

        protected static IEnumerable<Reference> Create(Parser parser, TElement element,
            Func<TElement, string> elementNameSelector,
            Func<TElement, IEnumerable<TAttribute>> elementAttributesSelector,
            Func<TAttribute, string> attributeNameSelector)
        {
            return Create(parser, element, elementNameSelector, null, elementAttributesSelector, attributeNameSelector, null);
        }

        protected static IEnumerable<Reference> Create(Parser parser, TElement element,
            Func<TElement, string> elementNameSelector,
            Func<TElement, string> elementNamespaceSelector,
            Func<TElement, IEnumerable<TAttribute>> elementAttributesSelector,
            Func<TAttribute, string> attributeNameSelector,
            Func<TAttribute, string> attributeNamespaceSelector)
        {
            var elementKey = new ReferenceKey(elementNameSelector(element), elementNamespaceSelector != null ? elementNamespaceSelector(element) : null);
            IEnumerable<ReferenceType> references;
            if (_referencesLazy.Value.TryGetValue(elementKey, out references))
            {
                if (_anyReferences != null) references = _anyReferences.Concat(references);
            }
            else
            {
                if (_anyReferences == null) return Enumerable.Empty<Reference>();
                references = _anyReferences;
            }
            return elementAttributesSelector(element).Join(references,
                attribute => new ReferenceKey(attributeNameSelector(attribute), attributeNamespaceSelector != null ? attributeNamespaceSelector(attribute) : null),
                reference => reference.AttributeKey,
                (attribute, reference) => (Reference)Activator.CreateInstance(reference.Type, CreateReferenceArgs(parser, reference.Kind, null, element, attribute)));
        }

        static readonly Lazy<ConstructorInfo> _referenceArgsConstructorLazy = new Lazy<ConstructorInfo>(() =>
        {
            // TODO: Get types array dynamically by reflection
            var referenceArgsConstructorTypes = new[] { typeof(Parser), typeof(ReferenceKind), typeof(string), typeof(TElement), typeof(TAttribute) };
            var tReferenceArgsConstructor = typeof(TReferenceArgs).GetConstructor(referenceArgsConstructorTypes);
            if (tReferenceArgsConstructor == null)
                throw new NotSupportedException(string.Format("Reference does not support \"{0}\".", typeof(TReferenceArgs).Name));
            return tReferenceArgsConstructor;
        });

        static TReferenceArgs CreateReferenceArgs(Parser parser, ReferenceKind kind, string mimeType, TElement element, TAttribute attribute)
        {
            // TODO: Replace by a dedicated method via interface?
            return (TReferenceArgs)_referenceArgsConstructorLazy.Value.Invoke(new object[] { parser, kind, mimeType, element, attribute });
        }

        protected TElement Element { get; private set; }
        protected TAttribute Attribute { get; private set; }

        protected Reference(ReferenceArgs<TElement, TAttribute> referenceArgs)
            : base(referenceArgs)
        {
            Element = referenceArgs.Element;
            Attribute = referenceArgs.Attribute;
        }
    }
}
