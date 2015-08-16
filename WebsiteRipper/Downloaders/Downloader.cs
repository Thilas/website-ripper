using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Downloaders
{
    using DownloaderConstructor = Func<DownloaderArgs, Downloader>;

    public abstract class Downloader : IDisposable
    {
        static readonly Lazy<Dictionary<string, DownloaderConstructor>> _downloadersLazy = new Lazy<Dictionary<string, DownloaderConstructor>>(() =>
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Select(type => new { Type = type, Constructor = type.GetConstructorOrDefault<DownloaderConstructor>(downloaderArgs => new HttpDownloader(downloaderArgs)) })
                .Where(downloader => downloader.Constructor != null)
                .SelectMany(downloader => downloader.Type.GetCustomAttributes<DownloaderAttribute>(false)
                    .Select(downloaderAttribute => new { downloaderAttribute.Scheme, downloader.Constructor }))
                .Distinct() // TODO Review duplicate schemes management
                .ToDictionary(downloader => downloader.Scheme, downloader => downloader.Constructor,
                    StringComparer.OrdinalIgnoreCase);
        });

        internal static Downloader Create(Uri uri, int timeout, string preferredLanguages)
        {
            return Create(uri, null, timeout, preferredLanguages);
        }

        internal static Downloader Create(Uri uri, string mimeType, int timeout, string preferredLanguages)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            var scheme = uri.Scheme;
            DownloaderConstructor downloaderConstructor;
            if (!_downloadersLazy.Value.TryGetValue(scheme, out downloaderConstructor))
                throw new NotSupportedException(string.Format("Downloader does not support scheme \"{0}\".", scheme));
            return downloaderConstructor(new DownloaderArgs(uri, mimeType, timeout, preferredLanguages));
        }

        internal static bool Supports(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            var scheme = uri.Scheme;
            return _downloadersLazy.Value.ContainsKey(scheme);
        }

        static readonly Lazy<Regex> _contentTypeRegexLazy = new Lazy<Regex>(() => new Regex(@";?\s*(?<type>[^\s/;]+)/(?<subtype>[^\s/;]+)\s*;?", RegexOptions.Compiled));

        static string GetMimeType(string contentType)
        {
            var match = _contentTypeRegexLazy.Value.Match(contentType);
            if (!match.Success) throw new InvalidOperationException("Content-Type is invalid.");
            return string.Format("{0}/{1}", match.Groups["type"].Value, match.Groups["subtype"].Value);
        }

        readonly string _mimeType;

        protected WebRequest WebRequest { get; private set; }
        protected WebResponse WebResponse { get; private set; }

        internal long ContentLength { get { return WebResponse.ContentLength; } }
        protected internal virtual DateTime LastModified { get { return DateTime.Now; } }
        internal string MimeType { get { return _mimeType ?? GetMimeType(WebResponse.ContentType); } }
        internal Uri ResponseUri { get { return WebResponse.ResponseUri; } }

        protected Downloader(DownloaderArgs downloaderArgs)
        {
            if (downloaderArgs == null) throw new ArgumentNullException("downloaderArgs");
            _mimeType = downloaderArgs.MimeType;
            WebRequest = WebRequest.Create(downloaderArgs.Uri);
            WebRequest.Timeout = downloaderArgs.Timeout;
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
