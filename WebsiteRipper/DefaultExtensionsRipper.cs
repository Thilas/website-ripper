//#define FAKE_UPDATE

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WebsiteRipper.Parsers;

namespace WebsiteRipper
{
    sealed class DefaultExtensionsRipper : Ripper
    {
        public static DefaultExtensions GetIanaDefaultExtensions(Uri mediaTypesUri)
        {
            // Parse mime types from iana web site
            var defaultExtensionsParserBaseType = typeof(DefaultExtensionsParser).BaseType;
            var defaultExtensionsParserConstructor = Parser.GetConstructor(parserArgs => new DefaultExtensionsParser(parserArgs));
            foreach (var parserType in Parser.ParserTypes.ToList())
            {
                if (parserType.Value.Method.ReturnType == defaultExtensionsParserBaseType)
                    Parser.ParserTypes[parserType.Key] = defaultExtensionsParserConstructor;
                else
                    Parser.ParserTypes.Remove(parserType.Key);
            }
            var rootPath = Path.GetTempFileName();
            File.Delete(rootPath);
            try
            {
#if (DEBUG && FAKE_UPDATE)
                rootPath = Path.GetFullPath(@"..\..\..\iana");
                //rootPath = @"...";
#endif
                var ripper = new DefaultExtensionsRipper(mediaTypesUri, rootPath);
#if (!DEBUG || !FAKE_UPDATE)
                ripper.Rip(RipMode.Create);
#else
                ripper.Rip(RipMode.Update);
#endif
                // TODO Read files asynchronously while ripping them
                lock (_templates)
                {
                    return new DefaultExtensions(_templates.Select(ParseTemplate).Where(mimeType => mimeType != null), ripper.Resource.LastModified);
                }
            }
            finally
            {
#if (!DEBUG || !FAKE_UPDATE)
                if (Directory.Exists(rootPath)) Directory.Delete(rootPath, true);
#endif
            }
        }

        static readonly Dictionary<string, MimeType> _templates = new Dictionary<string, MimeType>();

        // Specific mime type exclusions
        private static readonly HashSet<string> _excludedMimeTypes = new HashSet<string>(new[]
        {
            "application/prs.alvestrand.titrax-sheet",  // Current regex fails...
            "application/vnd.xmi+xml",                  // Template is invalid
            "application/vnd.commerce-battelle"         // File extensions list is dynamic
        }, StringComparer.OrdinalIgnoreCase);

        static readonly Lazy<Regex> _fileExtensionsRegexLazy = new Lazy<Regex>(() => new Regex(
            @"\bFile\s+extension(?:\(s\))?\s*:\s*(?<extensions>[\w\W]*?)\s*(?:\n\s*\n|(?:\n|\b\d\.|\bMacintosh\b)[^\n:]+:[^/]{2})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant));

        static MimeType ParseTemplate(KeyValuePair<string, MimeType> templateKeyValuePair)
        {
            using (var streamReader = new StreamReader(templateKeyValuePair.Key, true))
            {
                // Get the mime type
                var mimeType = templateKeyValuePair.Value;
                const string exampleName = "example";
                if (new[] { mimeType.TypeName, mimeType.SubtypeName }.Contains(exampleName, StringComparer.OrdinalIgnoreCase)) return null;
                if (_excludedMimeTypes.Contains(mimeType.ToString())) return mimeType;

                // Extract file extensions
                var template = streamReader.ReadToEnd();
                var fileExtensionsMatch = _fileExtensionsRegexLazy.Value.Match(template);
                if (!fileExtensionsMatch.Success) return mimeType;
                var fileExtensions = GetFileExtensionsMatches(fileExtensionsMatch.Groups["extensions"].Value).Cast<Match>()
                    .SelectMany(match => match.Groups["extensions"].Captures.Cast<Capture>())
                    .Select(capture => capture.Value).Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(extension => string.Format(".{0}", extension.ToLowerInvariant())).ToList();
                return mimeType.SetExtensions(fileExtensions);
            }
        }

        static readonly Lazy<Regex> _noExtensionsRegexLazy = new Lazy<Regex>(() => new Regex(
            @"\b(?:n/a|not?\b.+(?:\bspecific\b.+)?\b""?extensions?""?|not\s+applicable|none(?!\s+or)|see\s+registration|unknown)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant));
        static readonly Lazy<Regex> _doubleQuotedExtensionsRegexLazy = new Lazy<Regex>(() => new Regex(
            string.Format(@"""(?:\*?\.)?(?<extensions>{0})""", DefaultExtensions.ExtensionRegexClass), RegexOptions.Compiled));
        static readonly Lazy<Regex> _singleQuotedExtensionsRegexLazy = new Lazy<Regex>(() => new Regex(
            string.Format(@"'(?:\*?\.)?(?<extensions>{0})'", DefaultExtensions.ExtensionRegexClass), RegexOptions.Compiled));
        static readonly Lazy<Regex> _dottedExtensionsRegexLazy = new Lazy<Regex>(() => new Regex(
            string.Format(@"(?:^|\s|,)\*?\.(?<extensions>{0})\b", DefaultExtensions.ExtensionRegexClass), RegexOptions.Compiled));
        static readonly Lazy<Regex> _noRequiredExtensionsRegexLazy = new Lazy<Regex>(() => new Regex(
            @"\bnot\s+(?:\w+\s+n?or\s+)?(?:expected|required)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant));
        static readonly Lazy<Regex> _extensionsRegexLazy = new Lazy<Regex>(() => new Regex(
            string.Format(@"(?:\b(?:and|(?:are\s+both|is)\s+declared\s+at\s+[\w\W]+|extension|or)\b|\([^\)]*\)|<[^>)]*>|\b(?<extensions>{0})\b)", DefaultExtensions.ExtensionRegexClass),
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant));

        static IEnumerable GetFileExtensionsMatches(string fileExtensions)
        {
            if (_noExtensionsRegexLazy.Value.IsMatch(fileExtensions)) return Enumerable.Empty<Match>();
            var doubleQuotedExtensions = _doubleQuotedExtensionsRegexLazy.Value.Matches(fileExtensions);
            if (doubleQuotedExtensions.Count > 0) return doubleQuotedExtensions;
            var singleQuotedExtensions = _singleQuotedExtensionsRegexLazy.Value.Matches(fileExtensions);
            if (singleQuotedExtensions.Count > 0) return singleQuotedExtensions;
            var dottedExtensions = _dottedExtensionsRegexLazy.Value.Matches(fileExtensions);
            if (dottedExtensions.Count > 0) return dottedExtensions;
            if (_noRequiredExtensionsRegexLazy.Value.IsMatch(fileExtensions)) return Enumerable.Empty<Match>();
            return _extensionsRegexLazy.Value.Matches(fileExtensions);
        }

        DefaultExtensionsRipper(Uri mediaTypesUri, string rootPath)
            : base(mediaTypesUri, rootPath, DefaultExtensions.Language)
        {
            Timeout = DefaultExtensions.Timeout;
        }

        internal override Resource GetSubResource(int depth, Resource resource, Reference reference)
        {
            var subResource = base.GetSubResource(depth, resource, reference);
            var defaultExtensionsReference = reference as DefaultExtensionsReference;
            if (subResource == null || defaultExtensionsReference == null) return subResource;
            lock (_templates)
            {
                if (!_templates.ContainsKey(subResource.NewUri.LocalPath)) _templates.Add(subResource.NewUri.LocalPath, defaultExtensionsReference.MimeType);
            }
            return subResource;
        }

        protected override Resource GetResource(Uri uri, bool hyperlink, string mimeType)
        {
            var uriString = uri.ToString();
            if (uriString.EndsWith("/")) throw new InvalidOperationException("Uri is invalid.");
#if (!DEBUG || !FAKE_UPDATE)
            return base.GetResource(uri, hyperlink, mimeType);
#else
            uri = new Uri(string.Format("{0}/{1}", uriString, "default.html"));
            return new Resource(this, Parser.CreateDefault(mimeType, uri), uri, false);
#endif
        }
    }
}
