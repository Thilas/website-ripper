using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Parsers.Xml
{
    [Parser(ApplicationMimeType)]
    [Parser(TextMimeType)]
    [Parser(XsltApplicationMimeType)]
    [Parser(XsltTextMimeType)]
    public class XmlParser : Parser
    {
        internal const string ApplicationMimeType = "application/xml";
        internal const string TextMimeType = "text/xml";
        public const string MimeType = TextMimeType;

        internal const string XmlNsPrefix = "xmlns";

        protected sealed override string DefaultFileNameWithoutExtension { get { return "document"; } }

        public XmlParser(ParserArgs parserArgs) : base(parserArgs) { }

        protected XmlDocument Document { get; private set; }

        Dictionary<string, IEnumerable<XmlAttribute>> _namespaces;
        Lazy<string> _defaultPrefixLazy;

        protected sealed override void Load(string path)
        {
            Document = new XmlDocument() { XmlResolver = null };
            Document.Load(path);
            InitializeNamespaces();
        }

        void InitializeNamespaces()
        {
            _namespaces = Document.GetDocumentElement().DescendantsAndSelf()
                .SelectMany(element => element.Attributes.Cast<XmlAttribute>())
                .Where(attribute => string.Equals(string.IsNullOrEmpty(attribute.Prefix) ? attribute.LocalName : attribute.Prefix, XmlNsPrefix))
                .GroupBy(attribute => attribute.Value)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping.ToList().AsEnumerable());
            _defaultPrefixLazy = new Lazy<string>(() =>
            {
                var prefixes = new HashSet<string>(_namespaces.Values.SelectMany(attributes => attributes
                    .Select(attribute => attribute.GetNamespacePrefix())
                    .Where(prefix => prefix != null).Distinct()));
                const string defaultPrefixBase = "ns";
                var defaultPrefix = defaultPrefixBase;
                var n = 0;
                while (prefixes.Contains(defaultPrefix)) defaultPrefix = string.Format("{0}{1}", defaultPrefixBase, ++n);
                return defaultPrefix;
            });
        }

        protected override IEnumerable<Reference> GetReferences()
        {
            return Document.OfType<XmlProcessingInstruction>()
                .SelectMany(processingInstruction => ProcessingInstructionReference.Create(this, processingInstruction))
                .Prepend(new DocumentTypeReference(this, Document))
                .Concat(GetXsdReferences())
                .Concat(GetXsltReferences());
        }

        IEnumerable<Reference> GetNamespaceReferences(string namespaceUri, Func<string, string> xPathFactory)
        {
            IEnumerable<XmlAttribute> attributes;
            if (!_namespaces.TryGetValue(namespaceUri, out attributes)) return Enumerable.Empty<Reference>();
            return attributes.SelectMany(attribute =>
            {
                var prefix = attribute.GetNamespacePrefix() ?? _defaultPrefixLazy.Value;
                var namespaceManager = new XmlNamespaceManager(Document.NameTable);
                namespaceManager.AddNamespace(prefix, namespaceUri);
                var uris = attribute.GetOwnerElement().SelectNodes(xPathFactory(prefix), namespaceManager);
                return uris != null ? uris.Cast<XmlAttribute>().Select(uri => new XmlReference(this, uri)) : Enumerable.Empty<Reference>();
            }).Where(reference => reference != null).ToList();
        }

        public const string XsdNamespace = "http://www.w3.org/1999/XSL/Transform";

        IEnumerable<Reference> GetXsdReferences()
        {
            // TODO: To finish
            return Enumerable.Empty<Reference>();
            //return GetNamespaceReferences(XsltNamespace,
            //    prefix => string.Format(".//{0}:import/@href|.//{0}:import-schema/@schema-location|.//{0}:include/@href", prefix));
        }

        public const string XsltNamespace = "http://www.w3.org/1999/XSL/Transform";

        internal const string XsltApplicationMimeType = "application/xslt+xml";
        internal const string XsltTextMimeType = "text/xsl";

        IEnumerable<Reference> GetXsltReferences()
        {
            return GetNamespaceReferences(XsltNamespace,
                prefix => string.Format(".//{0}:import/@href|.//{0}:import-schema/@schema-location|.//{0}:include/@href", prefix));
        }

        protected sealed override void Save(string path)
        {
            Document.Save(path);
        }
    }
}
