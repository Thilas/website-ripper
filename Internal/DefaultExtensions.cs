using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Internal
{
    sealed class DefaultExtensions : IEnumerable<MimeType>
    {
        static readonly string _rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        const string DefaultExtensionsFile = "default.extensions";
        static readonly string _defaultExtensionsPath = Path.Combine(_rootPath, DefaultExtensionsFile);

        public static readonly string ExtensionRegexClass = string.Format(@"{0}(?:\.{0})?", string.Format(@"[^\.,{0}\p{{Z}}]+", Regex.Escape(string.Join(string.Empty, new char[] { '\f', '\n', '\r', '\t', '\v', '\x85' }.Union(Path.GetInvalidFileNameChars()).Distinct()))));

        static DefaultExtensions _all = null;
        static Lazy<DefaultExtensions> _allLazy = new Lazy<DefaultExtensions>(() => Load(_defaultExtensionsPath));
        public static DefaultExtensions All { get { return _all ?? _allLazy.Value; } }

        // TODO: Light/Full update modes (ie update only apache or iana and apache)
        public static void Update()
        {
            const string IanaMimeTypesFile = "iana.mime.types";
            var ianaMimeTypesPath = Path.Combine(_rootPath, IanaMimeTypesFile);
            // TODO: Test also date
            var ianaDefaultExtensions = !File.Exists(ianaMimeTypesPath) ? Iana.Save(ianaMimeTypesPath) : Load(ianaMimeTypesPath);
            const string MimeTypesFile = "apache.mime.types";
            var apacheDefaultExtensions = Apache.Save(Path.Combine(_rootPath, MimeTypesFile));
            var hardCodedDefaultExtensions = new DefaultExtensions(new[] { new MimeType("application", "x-javascript").SetExtensions(new[] { ".js" }) });
            Func<MimeType, MimeType, MimeType> mimeTypeResultSelector = (outerMimeType, innerMimeType) =>
            {
                if (innerMimeType == null) return outerMimeType;
                if (outerMimeType == null) return innerMimeType;
                if (innerMimeType.Extensions == null) return outerMimeType;
                if (outerMimeType.Extensions == null) return innerMimeType;
                return outerMimeType.SetExtensions(outerMimeType.Extensions.Union(innerMimeType.Extensions, StringComparer.OrdinalIgnoreCase));
            };
            var defaultExtensions = ianaDefaultExtensions
                .OuterJoin(apacheDefaultExtensions, mimeType => mimeType.ToString(), mimeTypeResultSelector, null, StringComparer.OrdinalIgnoreCase)
                .OuterJoin(hardCodedDefaultExtensions, mimeType => mimeType.ToString(), mimeTypeResultSelector, null, StringComparer.OrdinalIgnoreCase);
            _all = new DefaultExtensions(defaultExtensions).Save(_defaultExtensionsPath);
        }

        static readonly Lazy<DefaultExtensions> _iana = new Lazy<DefaultExtensions>(() =>
        {
            var allBackup = _all;
            try
            {
                _all = new DefaultExtensions(Enumerable.Empty<MimeType>());
                return new DefaultExtensions(DefaultExtensionsRipper.GetIanaDefaultExtensions().Result);
            }
            finally
            {
                _all = allBackup;
            }
        });
        public static DefaultExtensions Iana { get { return _iana.Value; } }

        static readonly Lazy<DefaultExtensions> _apache = new Lazy<DefaultExtensions>(() =>
        {
            // Parse mime types from Apache project's web site
            var mimeTypesPath = Path.GetTempFileName();
            try
            {
                // TODO: Use setting instead of constant
                const string MimeTypesUrl = "http://svn.apache.org/viewvc/httpd/httpd/trunk/docs/conf/mime.types?view=co";
                var webClient = new WebClient();
                webClient.DownloadFile(MimeTypesUrl, mimeTypesPath);
                var apache = Load(mimeTypesPath);
                return apache;
            }
            catch
            {
                return new DefaultExtensions(Enumerable.Empty<MimeType>());
            }
            finally
            {
                File.Delete(mimeTypesPath);
            }
        });
        public static DefaultExtensions Apache { get { return _apache.Value; } }

        readonly Dictionary<string, MimeType> _defaultExtensions;

        DefaultExtensions(IEnumerable<MimeType> defaultExtensions)
        {
            _defaultExtensions = defaultExtensions.ToDictionary(mimeType => mimeType.ToString(), mimeType => mimeType, StringComparer.OrdinalIgnoreCase);
        }

        static Lazy<Regex> _defaultExtensionsRegex = new Lazy<Regex>(() => new Regex(
            string.Format(@"^(?:\s*#\s*)?(?<type>\S+)/(?<subtype>\S+)(?:\s+(?<extensions>{0})\b)*$", ExtensionRegexClass),
            RegexOptions.Multiline | RegexOptions.Compiled));

        static DefaultExtensions Load(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            using (var streamReader = new StreamReader(path, Encoding.Default))
            {
                var matches = _defaultExtensionsRegex.Value.Matches(streamReader.ReadToEnd());
                var defaultExtensions = matches.Cast<Match>().Select((match) =>
                {
                    var mimeType = new MimeType(match.Groups["type"].Value.ToLowerInvariant(), match.Groups["subtype"].Value.ToLowerInvariant());
                    var captures = match.Groups["extensions"].Captures;
                    return captures.Count != 0 ?
                        mimeType.SetExtensions(captures.Cast<Capture>().Select((capture) => string.Format(".{0}", capture.Value.ToLowerInvariant()))) :
                        mimeType;
                });
                return new DefaultExtensions(defaultExtensions);
            }
        }

        DefaultExtensions Save(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            const int MimeTypeWidth = 48;
            const int TabulationWidth = 8;
            using (var streamWriter = new StreamWriter(path, false, Encoding.Default))
            {
                streamWriter.Write("# MIME type (lowercased)\t\t\tExtensions\n");
                streamWriter.Write("# ============================================\t==========\n");
                foreach (var mimeType in this.OrderBy(mimeType => mimeType.ToString(), StringComparer.OrdinalIgnoreCase))
                {
                    var mimeTypeName = mimeType.ToString();
                    if (mimeType.Extensions != null)
                    {
                        streamWriter.Write("{0}{1}{2}\n", mimeTypeName,
                            new string('\t', mimeTypeName.Length < MimeTypeWidth ? (MimeTypeWidth - mimeTypeName.Length) / TabulationWidth + 1 : 1),
                            string.Join(" ", mimeType.Extensions.Select(extension => extension.Substring(1))));
                    }
                    else
                        streamWriter.Write("# {0}\n", mimeTypeName);
                }
            }
            return this;
        }

        public IEnumerator<MimeType> GetEnumerator() { return _defaultExtensions.Values.GetEnumerator(); }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public MimeType this[string mimeType] { get { return _defaultExtensions[mimeType]; } }

        public bool TryGetDefaultExtension(string mimeTypeName, out string defaultExtension)
        {
            MimeType mimeType;
            if (!_defaultExtensions.TryGetValue(mimeTypeName, out mimeType) || mimeType.Extensions == null || mimeType.Extensions.Length == 0)
            {
                defaultExtension = null;
                return false;
            }
            defaultExtension = mimeType.Extensions[0];
            return true;
        }
    }
}
