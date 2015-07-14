using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebsiteRipper.Parsers;

namespace WebsiteRipper.Core
{
    sealed class DefaultExtensionsRipper : Ripper
    {
        public async static Task<DefaultExtensions> GetIanaDefaultExtensions(Uri mediaTypesUrl)
        {
            // Parse mime types from IANA web site
            var defaultExtensionsParserType = typeof(DefaultExtensionsParser);
            foreach (var parserType in Parser.ParserTypes.ToList())
            {
                if (parserType.Value == defaultExtensionsParserType.BaseType)
                    Parser.ParserTypes[parserType.Key] = defaultExtensionsParserType;
                else
                    Parser.ParserTypes.Remove(parserType.Key);
            }
            var rootPath = Path.GetTempFileName();
            File.Delete(rootPath);
            try
            {
                var ripper = new DefaultExtensionsRipper(mediaTypesUrl, rootPath);
                await ripper.RipAsync(RipMode.Create);
                // TODO: Read files asynchronously while ripping them
                lock (_templates)
                {
                    return new DefaultExtensions(_templates.Select(ParseTemplate).Where(mimeType => mimeType != null), ripper.Resource.LastModified);
                }
            }
            finally
            {
                if (Directory.Exists(rootPath)) Directory.Delete(rootPath, true);
            }
        }

        static readonly Dictionary<string, MimeType> _templates = new Dictionary<string, MimeType>();

        static Lazy<Regex> _fileExtensionsRegex = new Lazy<Regex>(() => new Regex(
            @"\bFile\s+extension(?:\(s\))?\s*:\s*(?<extensions>[\w\W]*?)\s*(?:\n\s*\n|(?:\n|\b\d\.|\bMacintosh\b)[^\n:]+:[^/]{2})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant));

        static MimeType ParseTemplate(KeyValuePair<string, MimeType> templateKeyValuePair)
        {
            using (var streamReader = new StreamReader(templateKeyValuePair.Key, true))
            {
                // Get the mime type
                var mimeType = templateKeyValuePair.Value;
                const string ExampleName = "example";
                if (new[] { mimeType.TypeName, mimeType.SubtypeName }.Contains(ExampleName, StringComparer.OrdinalIgnoreCase)) return null;

                // Check whether the template is about this mime type
                var template = streamReader.ReadToEnd();
                if ((!Regex.IsMatch(template, string.Format(@"\bType\s+name\s*:.*\b{0}\b", Regex.Escape(mimeType.TypeName)), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ||
                    !Regex.IsMatch(template, string.Format(@"\bSubtype\s+name\s*:.*\b{0}\b", Regex.Escape(mimeType.SubtypeName).Replace(@"vnd\.", @"(?:vnd\.)?")), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) &&
                    !Regex.IsMatch(template, string.Format(@"\bThe\s+{0}\s+content-type\b", Regex.Escape(mimeType.ToString())), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    return null;
                }

                // Extract file extensions
                var fileExtensionsMatch = _fileExtensionsRegex.Value.Match(template);
                if (!fileExtensionsMatch.Success) return mimeType;
                var fileExtensions = GetFileExtensionsMatches(fileExtensionsMatch.Groups["extensions"].Value).OfType<Match>()
                    .SelectMany(match => match.Groups["extensions"].Captures.OfType<Capture>())
                    .Select(capture => capture.Value).Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(extension => string.Format(".{0}", extension.ToLowerInvariant())).ToList();
                if (fileExtensions.Count > 8) return mimeType;
                return mimeType.SetExtensions(fileExtensions);
            }
        }

        static Lazy<Regex> _noExtensionRegex = new Lazy<Regex>(() => new Regex(
            @"\b(?:n/a|not?\b.+\bspecific\b.+\bextensions?|not\s+(?:applicable|expected|required)|none(?!\s+or)|see\s+registration|unknown)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant));
        static Lazy<Regex> _doubleQuotedExtensionsRegex = new Lazy<Regex>(() => new Regex(
            string.Format(@"""(?:\*?\.)?(?<extensions>{0})""", DefaultExtensions.ExtensionRegexClass), RegexOptions.Compiled));
        static Lazy<Regex> _singleQuotedExtensionsRegex = new Lazy<Regex>(() => new Regex(
            string.Format(@"'(?:\*?\.)?(?<extensions>{0})'", DefaultExtensions.ExtensionRegexClass), RegexOptions.Compiled));
        static Lazy<Regex> _dottedExtensionsRegex = new Lazy<Regex>(() => new Regex(
            string.Format(@"(?:^|\s|,)\*?\.(?<extensions>{0})\b", DefaultExtensions.ExtensionRegexClass), RegexOptions.Compiled));
        static Lazy<Regex> _extensionsRegex = new Lazy<Regex>(() => new Regex(
            string.Format(@"(?:\b(?:and|(?:are\s+both|is)\s+declared\s+at\s+[\w\W]+|extension|or)\b|\([^\)]*\)|<[^>)]*>|\b(?<extensions>{0})\b)", DefaultExtensions.ExtensionRegexClass),
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant));

        static IEnumerable GetFileExtensionsMatches(string fileExtensions)
        {
            if (_noExtensionRegex.Value.IsMatch(fileExtensions)) return Enumerable.Empty<object>();
            var doubleQuotedExtensions = _doubleQuotedExtensionsRegex.Value.Matches(fileExtensions);
            if (doubleQuotedExtensions.Count > 0) return doubleQuotedExtensions;
            var singleQuotedExtensions = _singleQuotedExtensionsRegex.Value.Matches(fileExtensions);
            if (singleQuotedExtensions.Count > 0) return singleQuotedExtensions;
            var dottedExtensions = _dottedExtensionsRegex.Value.Matches(fileExtensions);
            if (dottedExtensions.Count > 0) return dottedExtensions;
            return _extensionsRegex.Value.Matches(fileExtensions);
        }

        DefaultExtensionsRipper(Uri mediaTypesUrl, string rootPath)
            : base(mediaTypesUrl, rootPath, DefaultExtensions.Language)
        {
            Timeout = DefaultExtensions.Timeout;
        }

        internal sealed override Resource GetSubResource(int depth, Resource resource, Reference reference)
        {

            var subResource = base.GetSubResource(depth, resource, reference);
            var defaultExtensionsReference = reference as DefaultExtensionsReference;
            if (subResource == null || defaultExtensionsReference == null) return subResource;
            lock (_templates)
            {
                if (!_templates.ContainsKey(subResource.NewUrl.LocalPath)) _templates.Add(subResource.NewUrl.LocalPath, defaultExtensionsReference.MimeType);
            }
            return subResource;
        }
    }
}
