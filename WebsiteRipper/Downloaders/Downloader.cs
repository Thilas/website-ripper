using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

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

        internal static Downloader Create(Uri url, int timeout, string preferredLanguages)
        {
            if (url == null) throw new ArgumentNullException("url");
            var scheme = url.Scheme;
            Type downloaderType;
            if (!string.IsNullOrEmpty(scheme) && DownloaderTypes.TryGetValue(scheme, out downloaderType))
                return (Downloader)Activator.CreateInstance(downloaderType, url, timeout, preferredLanguages);
            throw new NotSupportedException();
        }

        protected WebRequest WebRequest { get; private set; }
        protected WebResponse WebResponse { get; private set; }

        internal long ContentLength { get { return WebResponse.ContentLength; } }
        internal string ContentType { get { return WebResponse.ContentType; } }
        protected internal virtual DateTime LastModified { get { return DateTime.Now; } }
        internal Uri ResponseUri { get { return WebResponse.ResponseUri; } }

        protected Downloader(Uri url, int timeout, string preferredLanguages)
        {
            WebRequest = WebRequest.Create(url);
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
            var result = webException != null;
            if (result) WebResponse = webException.Response;
            return result;
        }

        internal Stream GetResponseStream()
        {
            return WebResponse.GetResponseStream();
        }
    }
}
