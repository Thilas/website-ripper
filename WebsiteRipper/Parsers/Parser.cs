﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WebsiteRipper.Parsers
{
    public abstract class Parser
    {
        static readonly Lazy<Dictionary<string, Type>> _parserTypesLazy = new Lazy<Dictionary<string, Type>>(() =>
        {
            var parserType = typeof(Parser);
            var parserConstructorTypes = new[] { typeof(ParserArgs) };
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && parserType.IsAssignableFrom(type) && type.GetConstructor(parserConstructorTypes) != null)
                .SelectMany(type => type.GetCustomAttributes<ParserAttribute>(false)
                    .Select(parserAttribute => new { parserAttribute.MimeType, Type = type }))
                .Distinct() // TODO: Review duplicate mime types management
                .ToDictionary(parser => parser.MimeType, parser => parser.Type, StringComparer.OrdinalIgnoreCase);
        });

        internal static Dictionary<string, Type> ParserTypes { get { return _parserTypesLazy.Value; } }

        internal static Parser CreateDefault(string mimeType, Uri uri = null)
        {
            return new DefaultParser(new ParserArgs(mimeType, uri));
        }

        internal static Parser Create(string mimeType)
        {
            Type parserType;
            if (mimeType != null && ParserTypes.TryGetValue(mimeType, out parserType))
                return (Parser)Activator.CreateInstance(parserType, new ParserArgs(mimeType));
            return CreateDefault(mimeType);
        }

        protected internal string ActualMimeType { get; private set; }

        internal bool AnyChange { get; set; }

        protected Parser(ParserArgs parserArgs)
        {
            ActualMimeType = parserArgs.MimeType;
            AnyChange = false;
        }

        public string DefaultFileName { get { return Path.ChangeExtension(DefaultFileNameWithoutExtension, GetDefaultExtension()); } }

        protected abstract string DefaultFileNameWithoutExtension { get; }

        protected virtual string GetDefaultExtension()
        {
            if (string.IsNullOrEmpty(ActualMimeType)) throw new NotSupportedException("Parser does not support empty mime type.");
            string defaultExtension;
            if (!DefaultExtensions.All.TryGetDefaultExtension(ActualMimeType, out defaultExtension))
                throw new NotSupportedException(string.Format("Parser does not support mime type \"{0}\".", ActualMimeType));
            if (string.IsNullOrEmpty(defaultExtension))
                throw new NotSupportedException(string.Format("Parser does not support mime type \"{0}\" with no extensions.", ActualMimeType));
            return defaultExtension;
        }

        public IEnumerable<string> OtherExtensions
        {
            get
            {
                return !string.IsNullOrEmpty(ActualMimeType) ? DefaultExtensions.All.GetOtherExtensions(ActualMimeType) : Enumerable.Empty<string>();
            }
        }

        bool _loaded = false;
        bool _failed = false;

        internal IEnumerable<Resource> GetResources(Ripper ripper, int depth, Resource resource)
        {
            if (!_loaded)
            {
                try { Load(resource.NewUri.LocalPath); }
                catch { _failed = true; } // TODO: Log a warning
                _loaded = true;
            }
            if (_failed) yield break;
            foreach (var subResource in GetReferences().Where(reference => reference.Kind != ReferenceKind.Skip && !string.IsNullOrEmpty(reference.Uri))
                .Select(reference => ripper.GetSubResource(depth, resource, reference)))
            {
                if (subResource != null) yield return subResource;
                ripper.CancellationToken.ThrowIfCancellationRequested();
            }
            if (!AnyChange) yield break;
            try { Save(resource.NewUri.LocalPath); }
            catch { /* suppress errors */ }
            AnyChange = false;
        }

        protected abstract void Load(string path);

        protected abstract IEnumerable<Reference> GetReferences();

        protected abstract void Save(string path);
    }
}
