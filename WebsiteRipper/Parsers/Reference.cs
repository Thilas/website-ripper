using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        protected Reference(ReferenceArgs referenceArgs)
        {
            if (referenceArgs == null) throw new ArgumentNullException("referenceArgs");
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
            get { return UriInternal; }
            internal set
            {
                var oldValue = UriInternal;
                if (string.Equals(value, oldValue, StringComparison.OrdinalIgnoreCase)) return;
                UriInternal = value;
                if (!string.Equals(UriInternal, oldValue, StringComparison.OrdinalIgnoreCase)) _parser.AnyChange = true;
            }
        }

        protected abstract string UriInternal { get; set; }
    }

    public abstract class Reference<TElement, TAttribute> : Reference
    {
        sealed class FakeReference : Reference<TElement, TAttribute>
        {
            public FakeReference(ReferenceArgs<TElement, TAttribute> referenceArgs) : base(referenceArgs) { }
            protected override string UriInternal { get; set; }
        }

        static IEnumerable<ReferenceType<TElement, TAttribute>> _anyReferences;
        static readonly Lazy<Dictionary<ReferenceKey, IEnumerable<ReferenceType<TElement, TAttribute>>>> _referencesLazy = new Lazy<Dictionary<ReferenceKey, IEnumerable<ReferenceType<TElement, TAttribute>>>>(() =>
        {
            _anyReferences = new List<ReferenceType<TElement, TAttribute>>();
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Select(type => new
                {
                    Type = type,
                    Constructor = type.GetConstructorOrDefault<Func<ReferenceArgs<TElement, TAttribute>, Reference<TElement, TAttribute>>>(referenceArgs => new FakeReference(referenceArgs))
                })
                .Where(reference => reference.Constructor != null)
                .SelectMany(reference => reference.Type.GetCustomAttributes<ReferenceAttributeAttribute>(false)
                    .Select(referenceAttribute =>
                    {
                        var referenceElement = reference.Type.GetCustomAttribute<ReferenceElementAttribute>(false);
                        var @namespace = referenceElement != null ? referenceElement.Namespace : null;
                        var qualifiedAttributes = referenceElement != null && referenceElement.QualifiedAttributes;
                        var referenceType = new ReferenceType<TElement, TAttribute>(referenceAttribute, qualifiedAttributes ? @namespace : null, reference.Constructor);
                        if (referenceElement != null && referenceElement.Any)
                        {
                            ((List<ReferenceType<TElement, TAttribute>>)_anyReferences).Add(referenceType);
                            return null;
                        }
                        var elementName = referenceElement != null && !string.IsNullOrEmpty(referenceElement.Name) ? referenceElement.Name : reference.Type.Name;
                        return new { Key = new ReferenceKey(elementName, @namespace), Reference = referenceType };
                    }).Where(pair => pair != null))
                .GroupBy(
                    pair => pair.Key,
                    pair => pair.Reference)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping.Distinct().ToList().AsEnumerable()); // TODO Review duplicate references management
            var anyReferences = _anyReferences.Distinct().ToList(); // TODO Review duplicate any references management
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
            IEnumerable<ReferenceType<TElement, TAttribute>> references;
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
                (attribute, reference) => reference.Constructor(reference.ArgsCreator.Create(parser, reference.Kind, element, attribute)));
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
