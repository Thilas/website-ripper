using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebsiteRipper.Parsers
{
    public abstract class Parser
    {
        static readonly Lazy<Dictionary<string, Type>> _parserTypesLazy = new Lazy<Dictionary<string, Type>>(() =>
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

        internal static Dictionary<string, Type> ParserTypes { get { return _parserTypesLazy.Value; } }

        internal static Parser CreateDefault(string mimeType, Uri uri)
        {
            return new DefaultParser(mimeType, uri);
        }

        internal static Parser Create(string mimeType)
        {
            Type parserType;
            if (mimeType != null && ParserTypes.TryGetValue(mimeType, out parserType))
                return (Parser)Activator.CreateInstance(parserType, mimeType);
            return new DefaultParser(mimeType, null);
        }

        protected internal string ActualMimeType { get; private set; }

        internal bool AnyChange { get; set; }

        protected Parser(string mimeType)
        {
            ActualMimeType = mimeType;
            AnyChange = false;
        }

        public string DefaultFileName { get { return Path.ChangeExtension(DefaultFileNameWithoutExtension, GetDefaultExtension()); } }

        protected abstract string DefaultFileNameWithoutExtension { get; }

        protected virtual string GetDefaultExtension()
        {
            if (string.IsNullOrEmpty(ActualMimeType)) throw new NotSupportedException("Parser does not support empty MIME type.");
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
                Load(resource.NewUri.LocalPath);
                _loaded = true;
            }
            foreach (var subResource in GetReferences().Where(reference => reference.Kind != ReferenceKind.Skip && !string.IsNullOrEmpty(reference.Uri))
                .Select(reference => ripper.GetSubResource(depth, resource, reference)))
            {
                if (subResource != null) yield return subResource;
                ripper.CancellationToken.ThrowIfCancellationRequested();
            }
            if (!AnyChange) yield break;
            Save(resource.NewUri.LocalPath);
            AnyChange = false;
        }

        protected abstract void Load(string path);

        protected abstract IEnumerable<Reference> GetReferences();

        protected abstract void Save(string path);
    }
}
