using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;

namespace WebsiteRipper.Parsers
{
    public abstract class Parser
    {
        static readonly Lazy<Dictionary<string, Type>> _parserTypes = new Lazy<Dictionary<string, Type>>(() =>
        {
            var parserType = typeof(Parser);
            var parserConstructorTypes = new[] { typeof(string) };
            var parserAttributeType = typeof(ParserAttribute);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && parserType.IsAssignableFrom(type) && type.GetConstructor(parserConstructorTypes) != null)
                .SelectMany(type => ((ParserAttribute[])type.GetCustomAttributes(parserAttributeType, false))
                    .Select(parserAttribute => new { parserAttribute.MimeType, Type = type }))
                .Distinct()
                .ToDictionary(parser => parser.MimeType, parser => parser.Type, StringComparer.OrdinalIgnoreCase);
        });

        internal static Dictionary<string, Type> ParserTypes { get { return _parserTypes.Value; } }

        internal static Parser CreateDefault(string contentType, Uri url)
        {
            var mimeType = contentType != null ? new ContentType(contentType).MediaType : null;
            return new DefaultParser(mimeType, url);
        }

        internal static Parser Create(string contentType)
        {
            var mimeType = new ContentType(contentType).MediaType;
            Type parserType;
            if (!string.IsNullOrEmpty(mimeType) && ParserTypes.TryGetValue(mimeType, out parserType))
                return (Parser)Activator.CreateInstance(parserType, mimeType);
            return new DefaultParser(mimeType, null);
        }

        protected internal string ActualMimeType { get; private set; }

        internal bool AnyChange { get; set; }

        protected Parser(string mimeType)
        {
            if (mimeType == null) throw new ArgumentNullException("mimeType");
            ActualMimeType = mimeType;
            AnyChange = false;
        }

        public string DefaultFileName { get { return Path.ChangeExtension(DefaultFileNameWithoutExtension, GetDefaultExtension()); } }

        protected abstract string DefaultFileNameWithoutExtension { get; }

        protected virtual string GetDefaultExtension()
        {
            string defaultExtension;
            if (!DefaultExtensions.All.TryGetDefaultExtension(ActualMimeType, out defaultExtension))
                throw new NotSupportedException(string.Format("Parser does not support MIME type \"{0}\".", ActualMimeType));
            if (string.IsNullOrEmpty(defaultExtension))
                throw new NotSupportedException(string.Format("Parser does not support MIME type \"{0}\" with no extensions.", ActualMimeType));
            return defaultExtension;
        }

        public IEnumerable<string> OtherExtensions
        {
            get
            {
                if (string.IsNullOrEmpty(ActualMimeType)) return Enumerable.Empty<string>();
                return DefaultExtensions.All.GetOtherExtensions(ActualMimeType);
            }
        }

        bool _loaded = false;

        internal IEnumerable<Resource> GetResources(Ripper ripper, int depth, Resource resource)
        {
            // TODO: Handle exceptions on Load and Save methods
            if (!_loaded)
            {
                Load(resource.NewUrl.LocalPath);
                _loaded = true;
            }
            foreach (var subResource in GetReferences().Where(reference => reference.Kind != ReferenceKind.Skip && !string.IsNullOrEmpty(reference.Url))
                .Select(reference => ripper.GetSubResource(depth, resource, reference)))
            {
                if (subResource != null) yield return subResource;
                ripper.CancellationToken.ThrowIfCancellationRequested();
            }
            if (!AnyChange) yield break;
            Save(resource.NewUrl.LocalPath);
            AnyChange = false;
        }

        protected abstract void Load(string path);

        protected abstract IEnumerable<Reference> GetReferences();

        protected abstract void Save(string path);
    }
}
