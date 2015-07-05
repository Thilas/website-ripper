using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using WebsiteRipper.Core;
using WebsiteRipper.Extensions;
using WebsiteRipper.Properties;

namespace WebsiteRipper
{
    public sealed class DefaultExtensions : IEnumerable<MimeType>
    {
        static readonly string _rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        internal static readonly CultureInfo Language = new CultureInfo("en-US");
        internal const int Timeout = 5000;

        const string DefaultExtensionsFile = "default.extensions";
        static readonly string _defaultExtensionsPath = Path.Combine(_rootPath, DefaultExtensionsFile);

        internal static readonly string ExtensionRegexClass = string.Format(@"{0}(?:\.{0})?", string.Format(@"[^\.,{0}\p{{Z}}]+", Regex.Escape(string.Join(string.Empty, new char[] { '\f', '\n', '\r', '\t', '\v', '\x85' }.Union(Path.GetInvalidFileNameChars()).Distinct()))));

        static Lazy<DefaultExtensions> _empty = new Lazy<DefaultExtensions>(() => new DefaultExtensions(Enumerable.Empty<MimeType>()));
        internal static DefaultExtensions Empty { get { return _empty.Value; } }

        static DefaultExtensions _all = null;
        static Lazy<DefaultExtensions> _allLazy = new Lazy<DefaultExtensions>(() => Load(_defaultExtensionsPath));
        public static DefaultExtensions All { get { return _all ?? _allLazy.Value; } }

        public static void Update()
        {
            var hardCodedDefaultExtensions = new[]
            { 
                new MimeType("application", "x-javascript").SetExtensions(new[] { ".js" }) 
            };
            var defaultExtensions = Iana.OuterJoin(Apache, mimeType => mimeType.ToString(), MimeTypeResultSelector, null, StringComparer.OrdinalIgnoreCase)
                .OuterJoin(hardCodedDefaultExtensions, mimeType => mimeType.ToString(), MimeTypeResultSelector, null, StringComparer.OrdinalIgnoreCase);
            _all = new DefaultExtensions(defaultExtensions).Save(_defaultExtensionsPath);
        }

        static MimeType MimeTypeResultSelector(MimeType outerMimeType, MimeType innerMimeType)
        {
            if (innerMimeType == null) return outerMimeType;
            if (outerMimeType == null) return innerMimeType;
            if (innerMimeType.Extensions == null) return outerMimeType;
            if (outerMimeType.Extensions == null) return innerMimeType;
            return outerMimeType.SetExtensions(outerMimeType.Extensions.Union(innerMimeType.Extensions, StringComparer.OrdinalIgnoreCase));
        }

        static DefaultExtensions GetDefaultExtensions(string file, string url, Func<string, DefaultExtensions> factory)
        {
            var path = Path.Combine(_rootPath, file);
            if (File.Exists(path))
            {
                var defaultExtensions = Load(path);
                var httpWebRequest = WebRequest.CreateHttp(url);
                httpWebRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, Tools.GetPreferredLanguages(Language));
                httpWebRequest.Timeout = Timeout;
                var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                if (httpWebResponse.LastModified <= defaultExtensions.LastModified) return defaultExtensions;
            }
            return factory(url).Save(path);
        }

        static readonly Lazy<DefaultExtensions> _iana = new Lazy<DefaultExtensions>(() =>
        {
            const string IanaMimeTypesFile = "iana.mime.types";
            return GetDefaultExtensions(IanaMimeTypesFile, Settings.Default.IanaMediaTypesUrl, url =>
            {
                var allBackup = _all;
                _all = Empty;
                try
                {
                    return DefaultExtensionsRipper.GetIanaDefaultExtensions(url).Result;
                }
                finally
                {
                    _all = allBackup;
                }
            });
        });
        internal static DefaultExtensions Iana { get { return _iana.Value; } }

        static readonly Lazy<DefaultExtensions> _apache = new Lazy<DefaultExtensions>(() =>
        {
            const string MimeTypesFile = "apache.mime.types";
            return GetDefaultExtensions(MimeTypesFile, Settings.Default.ApacheMimeTypesUrl, url =>
            {
                // Parse mime types from Apache project's web site
                var mimeTypesPath = Path.GetTempFileName();
                try
                {
                    var webClient = new WebClient();
                    webClient.DownloadFile(url, mimeTypesPath);
                    var apache = Load(mimeTypesPath);
                    return apache;
                }
                finally
                {
                    File.Delete(mimeTypesPath);
                }
            });
        });
        internal static DefaultExtensions Apache { get { return _apache.Value; } }

        readonly Dictionary<string, MimeType> _defaultExtensions;

        public DateTime LastModified { get; private set; }

        internal DefaultExtensions(IEnumerable<MimeType> defaultExtensions) : this(defaultExtensions, DateTime.Now) { }

        internal DefaultExtensions(IEnumerable<MimeType> defaultExtensions, DateTime lastModified)
        {
            _defaultExtensions = defaultExtensions.ToDictionary(mimeType => mimeType.ToString(), mimeType => mimeType, StringComparer.OrdinalIgnoreCase);
            LastModified = lastModified;
        }

        const string LastModifiedFormat = "# Last modified: {0}";
        static Lazy<Regex> _defaultExtensionsRegex = new Lazy<Regex>(() => new Regex(
            string.Format(@"^(?:{0}|(?:\s*#\s*)?(?<type>\S+)/(?<subtype>\S+)(?:\s+(?<extensions>{1})\b)*)$", string.Format(LastModifiedFormat, "(?<date>[^\n]+)"), ExtensionRegexClass),
            RegexOptions.Multiline | RegexOptions.Compiled));

        static DefaultExtensions Load(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            using (var streamReader = new StreamReader(path, Encoding.Default))
            {
                var matches = _defaultExtensionsRegex.Value.Matches(streamReader.ReadToEnd());
                var lastModifiedGroup = matches.Cast<Match>().Select(match => match.Groups["date"]).SingleOrDefault(group => group.Success);
                DateTime lastModified;
                if (lastModifiedGroup == null || !DateTime.TryParseExact(lastModifiedGroup.Value, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastModified))
                    lastModified = DateTime.Now;
                var defaultExtensions = matches.Cast<Match>()
                    .Where(match => !match.Groups["date"].Success)
                    .Select(match =>
                    {
                        var mimeType = new MimeType(match.Groups["type"].Value.ToLowerInvariant(), match.Groups["subtype"].Value.ToLowerInvariant());
                        var captures = match.Groups["extensions"].Captures;
                        return captures.Count != 0 ?
                            mimeType.SetExtensions(captures.Cast<Capture>().Select((capture) => string.Format(".{0}", capture.Value.ToLowerInvariant()))) :
                            mimeType;
                    });
                return new DefaultExtensions(defaultExtensions, lastModified);
            }
        }

        DefaultExtensions Save(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            const int MimeTypeWidth = 48;
            const int TabulationWidth = 8;
            using (var streamWriter = new StreamWriter(path, false, Encoding.Default))
            {
                streamWriter.Write(LastModifiedFormat, LastModified.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
                streamWriter.Write("\n#\n");
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
            if (!_defaultExtensions.TryGetValue(mimeTypeName, out mimeType) || mimeType.Extensions == null || !mimeType.Extensions.Any())
            {
                defaultExtension = null;
                return false;
            }
            defaultExtension = mimeType.Extensions.First();
            return true;
        }
    }
}
