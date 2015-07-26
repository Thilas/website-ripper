using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebsiteRipper.Downloaders;
using WebsiteRipper.Extensions;
using WebsiteRipper.Parsers;

namespace WebsiteRipper
{
    public sealed class Resource
    {
        const int MaxPathLength = 260 - 1; // 1 is for the null terminating character

        readonly Ripper _ripper;
        readonly bool _hyperlink;

        Downloader _downloader = null;

        internal Parser Parser { get; private set; }

        public Uri NewUri { get; private set; }
        public Uri OriginalUri { get; private set; }

        public DateTime LastModified { get; private set; }

        internal static IEnumerable<Resource> Create(Ripper ripper, params Uri[] uris)
        {
            foreach (var uri in uris) yield return new Resource(ripper, false) { OriginalUri = uri };
        }

        Resource(Ripper ripper, bool hyperlink)
        {
            if (ripper == null) throw new ArgumentNullException("ripper");
            _ripper = ripper;
            _hyperlink = hyperlink;
        }

        internal Resource(Ripper ripper, Uri uri) : this(ripper, uri, true) { }

        internal Resource(Ripper ripper, Uri uri, bool hyperlink)
            : this(ripper, hyperlink)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            _downloader = Downloader.Create(uri, ripper.Timeout, ripper.PreferredLanguages);
            try
            {
                _downloader.SendRequest();
                LastModified = _downloader.LastModified;
            }
            catch (Exception exception)
            {
#if DEBUG
                var webException = exception as System.Net.WebException;
                //  Debug test to catch all exceptions but HTTP status 404
                if (webException == null || webException.Response == null || ((System.Net.HttpWebResponse)webException.Response).StatusCode != System.Net.HttpStatusCode.NotFound)
                    webException = null; // Add a breakpoint here
#endif
                if (!_downloader.SetResponse(exception)) Dispose();
                LastModified = _downloader != null ? _downloader.LastModified : DateTime.Now;
                uri = _downloader != null ? _downloader.ResponseUri : uri;
                Parser = Parser.CreateDefault(_downloader != null ? _downloader.MimeType : null, uri);
                OriginalUri = uri;
                NewUri = GetNewUri(uri);
                throw new ResourceUnavailableException(this, exception);
            }
            uri = _downloader.ResponseUri;
            Parser = Parser.Create(_downloader.MimeType);
            OriginalUri = uri;
            NewUri = GetNewUri(uri);
        }

        public override bool Equals(object obj) { return Equals(obj as Resource); }

        public bool Equals(Resource resource) { return resource != null && OriginalUri.Equals(resource.OriginalUri); }

        public override int GetHashCode() { return OriginalUri.GetHashCode(); }

        public override string ToString() { return OriginalUri.ToString(); }

        internal void Dispose()
        {
            if (_downloader == null) return;
            _downloader.Dispose();
            _downloader = null;
        }

        Uri GetNewUri(Uri uri)
        {
            var path = uri.Host.Split('.').Length != 2 ? uri.Host : string.Format("www.{0}", uri.Host);
            path = Path.Combine(_ripper.RootPath, CleanComponent(path));
            if (uri.LocalPath.Length > 1)
            {
                const char SegmentSeparatorChar = '/';
                path = uri.LocalPath.Substring(1).Split(SegmentSeparatorChar).Aggregate(path, (current, component) => Path.Combine(current, CleanComponent(component)));
                var extension = Path.GetExtension(path);
                var defaultExtension = Path.GetExtension(Parser.DefaultFileName);
                var otherExtensions = Parser.OtherExtensions;
                if (string.IsNullOrEmpty(extension) || !string.Equals(extension, defaultExtension, StringComparison.OrdinalIgnoreCase) ||
                    otherExtensions != null && otherExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    path = Path.Combine(path, Parser.DefaultFileName);
                }
            }
            else
                path = Path.Combine(path, Parser.DefaultFileName);
            var totalWidth = MaxPathLength - path.Length;
            if (totalWidth < 0) throw new PathTooLongException();
            if (!string.IsNullOrEmpty(uri.Query))
            {
                var query = Uri.UnescapeDataString(uri.Query).Replace('+', ' ');
                try { query = query.MiddleTruncate(totalWidth); }
                catch (Exception exception) { throw new PathTooLongException(null, exception); }
                var parentPath = Path.GetDirectoryName(path);
                path = string.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(path), CleanComponent(query), Path.GetExtension(path));
                if (parentPath != null) path = Path.Combine(parentPath, path);
            }
            return new Uri(path);
        }

        static string CleanComponent(string component)
        {
            const char ValidFileNameChar = '_';
            return Path.GetInvalidFileNameChars().Aggregate(component, (current, invalidFileNameChar) => current.Replace(invalidFileNameChar, ValidFileNameChar));
        }

        bool _ripped = false;
        internal async Task<IEnumerable<Resource>> RipAsync(RipMode ripMode, int depth)
        {
            if (_ripped) return Enumerable.Empty<Resource>();
            lock (this)
            {
                if (_ripped) return Enumerable.Empty<Resource>();
                _ripped = true;
            }
            await DownloadAsync(ripMode);
            if (_hyperlink) depth++;
            var subResources = await Task.WhenAll(Parser.GetResources(_ripper, depth, this)
                .Select(subResource => subResource.RipAsync(ripMode, depth)));
            return subResources.SelectMany(subResource => subResource).Prepend(this);
        }

        async Task<bool> DownloadAsync(RipMode ripMode)
        {
            if (_downloader == null) return false;
            try
            {
                var path = NewUri.LocalPath;
                if (ripMode != RipMode.Create && File.Exists(path) && File.GetLastWriteTime(path) >= _downloader.LastModified) return false;
                var parentPath = Path.GetDirectoryName(path);
                if (parentPath != null) Directory.CreateDirectory(parentPath);
                using (var responseStream = _downloader.GetResponseStream())
                {
                    try
                    {
                        using (var writer = new FileStream(path, ripMode == RipMode.Create ? FileMode.CreateNew : FileMode.Create, FileAccess.Write))
                        {
                            var totalBytesToReceive = _downloader.ContentLength;
                            var progress = totalBytesToReceive > 0 ? new Progress<long>(bytesReceived =>
                            {
                                _ripper.OnDownloadProgressChanged(new DownloadProgressChangedEventArgs(OriginalUri, bytesReceived, totalBytesToReceive));
                            }) : null;
                            if (totalBytesToReceive <= 0)
                                _ripper.OnDownloadProgressChanged(new DownloadProgressChangedEventArgs(OriginalUri, 0, totalBytesToReceive));
                            const int DownloadBufferSize = 4096;
                            var totalBytesReceived = await responseStream.CopyToAsync(writer, DownloadBufferSize, progress, _ripper.CancellationToken);
                            if (totalBytesToReceive <= 0)
                                _ripper.OnDownloadProgressChanged(new DownloadProgressChangedEventArgs(OriginalUri, totalBytesReceived, totalBytesReceived));
                        }
                    }
                    catch
                    {
                        if (File.Exists(path)) File.Delete(path);
                        throw;
                    }
                }
                return true;
            }
            finally
            {
                Dispose();
            }
        }
    }
}
