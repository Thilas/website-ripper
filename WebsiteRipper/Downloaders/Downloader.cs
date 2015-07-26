using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace WebsiteRipper.Downloaders
{
    public abstract class Downloader : IDisposable
    {
        static readonly Lazy<Dictionary<string, Type>> _downloaderTypes = new Lazy<Dictionary<string, Type>>(() =>
        {
            var downloaderType = typeof(Downloader);
            var downloaderConstructorTypes = new[] { typeof(Uri), typeof(int), typeof(string) };
            var downloaderAttributeType = typeof(DownloaderAttribute);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && downloaderType.IsAssignableFrom(type) && type.GetConstructor(downloaderConstructorTypes) != null)
                .SelectMany(type => ((DownloaderAttribute[])type.GetCustomAttributes(downloaderAttributeType, false))
                    .Select(downloaderAttribute => new { downloaderAttribute.Scheme, Type = type }))
                .Distinct()
                .ToDictionary(downloader => downloader.Scheme, downloader => downloader.Type, StringComparer.OrdinalIgnoreCase);
        });

        internal static Dictionary<string, Type> DownloaderTypes { get { return _downloaderTypes.Value; } }

        internal static Downloader Create(Uri uri, int timeout, string preferredLanguages)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            var scheme = uri.Scheme;
            Type downloaderType;
            if (scheme != null && DownloaderTypes.TryGetValue(scheme, out downloaderType))
                return (Downloader)Activator.CreateInstance(downloaderType, uri, timeout, preferredLanguages);
            throw new NotSupportedException(string.Format("Downloader does not support scheme \"{0}\".", scheme));
        }

        internal static bool Supports(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            var scheme = uri.Scheme;
            return scheme != null ? DownloaderTypes.ContainsKey(scheme) : false;
        }

        static readonly Lazy<Regex> _contentTypeRegex = new Lazy<Regex>(() => new Regex(@";?\s*(?<type>[^\s/;]+)/(?<subtype>[^\s/;]+)\s*;?", RegexOptions.Compiled));

        static string GetMimeType(string contentType)
        {
            var match = _contentTypeRegex.Value.Match(contentType);
            if (!match.Success) return contentType; // TODO: Improve parsing
            return string.Format("{0}/{1}", match.Groups["type"].Value, match.Groups["subtype"].Value);
        }

        protected WebRequest WebRequest { get; private set; }
        protected WebResponse WebResponse { get; private set; }

        internal long ContentLength { get { return WebResponse.ContentLength; } }
        protected internal virtual DateTime LastModified { get { return DateTime.Now; } }
        internal string MimeType { get { return GetMimeType(WebResponse.ContentType); } }
        internal Uri ResponseUri { get { return WebResponse.ResponseUri; } }

        protected Downloader(Uri uri, int timeout, string preferredLanguages)
        {
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
