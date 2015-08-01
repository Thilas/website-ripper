using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Downloaders
{
    public abstract class Downloader : IDisposable
    {
        static readonly Lazy<Dictionary<string, Type>> _downloaderTypesLazy = new Lazy<Dictionary<string, Type>>(() =>
        {
            var downloaderType = typeof(Downloader);
            var downloaderConstructorTypes = new[] { typeof(Uri), typeof(string), typeof(int), typeof(string) };
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && downloaderType.IsAssignableFrom(type) && type.GetConstructor(downloaderConstructorTypes) != null)
                .SelectMany(type => type.GetCustomAttributes<DownloaderAttribute>(false)
                    .Select(downloaderAttribute => new { downloaderAttribute.Scheme, Type = type }))
                .Distinct()
                .ToDictionary(downloader => downloader.Scheme, downloader => downloader.Type, StringComparer.OrdinalIgnoreCase);
        });

        internal static Dictionary<string, Type> DownloaderTypes { get { return _downloaderTypesLazy.Value; } }

        internal static Downloader Create(Uri uri, int timeout, string preferredLanguages)
        {
            return Create(uri, null, timeout, preferredLanguages);
        }

        internal static Downloader Create(Uri uri, string mimeType, int timeout, string preferredLanguages)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            var scheme = uri.Scheme;
            Type downloaderType;
            // TODO: Replace constructor with arguments by a dedicated method
            if (DownloaderTypes.TryGetValue(scheme, out downloaderType))
                return (Downloader)Activator.CreateInstance(downloaderType, uri, mimeType, timeout, preferredLanguages);
            throw new NotSupportedException(string.Format("Downloader does not support scheme \"{0}\".", scheme));
        }

        internal static bool Supports(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            var scheme = uri.Scheme;
            return DownloaderTypes.ContainsKey(scheme);
        }

        static readonly Lazy<Regex> _contentTypeRegexLazy = new Lazy<Regex>(() => new Regex(@";?\s*(?<type>[^\s/;]+)/(?<subtype>[^\s/;]+)\s*;?", RegexOptions.Compiled));

        static string GetMimeType(string contentType)
        {
            var match = _contentTypeRegexLazy.Value.Match(contentType);
            if (!match.Success) return contentType; // TODO: Improve parsing
            return string.Format("{0}/{1}", match.Groups["type"].Value, match.Groups["subtype"].Value);
        }

        readonly string _mimeType;

        protected WebRequest WebRequest { get; private set; }
        protected WebResponse WebResponse { get; private set; }

        internal long ContentLength { get { return WebResponse.ContentLength; } }
        protected internal virtual DateTime LastModified { get { return DateTime.Now; } }
        internal string MimeType { get { return _mimeType ?? GetMimeType(WebResponse.ContentType); } }
        internal Uri ResponseUri { get { return WebResponse.ResponseUri; } }

        protected Downloader(Uri uri, string mimeType, int timeout, string preferredLanguages)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (preferredLanguages == null) throw new ArgumentNullException("preferredLanguages");
            _mimeType = mimeType;
            WebRequest = WebRequest.Create(uri);
            WebRequest.Timeout = timeout;
        }

        public void Dispose()
        {
            if (WebResponse == null) return;
            WebResponse.Dispose();
            WebResponse = null;
        }

        internal void SendRequest()
        {
            WebResponse = WebRequest.GetResponse();
        }

        internal bool SetResponse(Exception exception)
        {
            var webException = exception as WebException;
            var result = webException != null && webException.Response != null;
            if (result) WebResponse = webException.Response;
            return result;
        }

        internal Stream GetResponseStream()
        {
            return WebResponse.GetResponseStream();
        }
    }
}
