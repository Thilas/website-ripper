using System;
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

        readonly Parser _parser;
        readonly Ripper _ripper;
        readonly bool _hyperlink;

        Downloader _downloader = null;

        public Uri NewUrl { get; private set; }
        public Uri OriginalUrl { get; private set; }

        public DateTime LastModified { get; private set; }

        internal Resource(Ripper ripper, Uri url, bool hyperlink = true)
        {
            if (ripper == null) throw new ArgumentNullException("ripper");
            if (url == null) throw new ArgumentNullException("url");
            _ripper = ripper;
            _hyperlink = hyperlink;
            _downloader = Downloader.Create(url, ripper.Timeout, ripper.PreferredLanguages);
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
                if (webException == null || ((System.Net.HttpWebResponse)webException.Response).StatusCode != System.Net.HttpStatusCode.NotFound)
                    webException = null; // Add a breakpoint here
#endif
                if (!_downloader.SetResponse(exception)) Dispose();
                LastModified = _downloader != null ? _downloader.LastModified : DateTime.Now;
                url = _downloader != null ? _downloader.ResponseUri : url;
                _parser = Parser.CreateDefault(_downloader != null ? _downloader.ContentType : null, url);
                OriginalUrl = url;
                NewUrl = GetNewUrl(url);
                throw new ResourceUnavailableException(this, exception);
            }
            url = _downloader.ResponseUri;
            _parser = Parser.Create(_downloader.ContentType);
            OriginalUrl = url;
            NewUrl = GetNewUrl(url);
        }

        public override bool Equals(object obj) { return Equals(obj as Resource); }

        public bool Equals(Resource resource) { return resource != null && NewUrl.Equals(resource.NewUrl); }

        public override int GetHashCode() { return NewUrl.GetHashCode(); }

        public override string ToString() { return OriginalUrl.ToString(); }

        internal void Dispose()
        {
            if (_downloader == null) return;
            _downloader.Dispose();
            _downloader = null;
        }

        Uri GetNewUrl(Uri url)
        {
            var path = url.Host.Split('.').Length != 2 ? url.Host : string.Format("www.{0}", url.Host);
            path = Path.Combine(_ripper.RootPath, CleanComponent(path));
            if (url.LocalPath.Length > 1)
            {
                const char SegmentSeparatorChar = '/';
                path = url.LocalPath.Substring(1).Split(SegmentSeparatorChar).Aggregate(path, (current, component) => Path.Combine(current, CleanComponent(component)));
                var extension = Path.GetExtension(path);
                var defaultExtension = Path.GetExtension(_parser.DefaultFile);
                var otherExtensions = _parser.OtherExtensions;
                if (string.IsNullOrEmpty(extension) || !string.Equals(extension, defaultExtension, StringComparison.OrdinalIgnoreCase) ||
                    otherExtensions != null && otherExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    path = Path.Combine(path, _parser.DefaultFile);
                }
            }
            else
                path = Path.Combine(path, _parser.DefaultFile);
            var totalWidth = MaxPathLength - path.Length;
            if (totalWidth < 0) throw new PathTooLongException();
            if (!string.IsNullOrEmpty(url.Query))
            {
                var query = Uri.UnescapeDataString(url.Query).Replace('+', ' ');
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
        internal async Task RipAsync(RipMode ripMode, int depth)
        {
            if (_ripped) return;
            lock (this)
            {
                if (_ripped) return;
                _ripped = true;
            }
            await DownloadAsync(ripMode);
            if (_hyperlink) depth++;
            await Task.WhenAll(_parser.GetResources(_ripper, depth, this).Select(subResource => subResource.RipAsync(ripMode, depth)));
        }

        async Task<bool> DownloadAsync(RipMode ripMode)
        {
            if (_downloader == null) return false;
            try
            {
                var path = NewUrl.LocalPath;
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
                                _ripper.OnDownloadProgressChanged(new DownloadProgressChangedEventArgs(OriginalUrl, bytesReceived, totalBytesToReceive));
                            }) : null;
                            if (totalBytesToReceive <= 0)
                                _ripper.OnDownloadProgressChanged(new DownloadProgressChangedEventArgs(OriginalUrl, 0, totalBytesToReceive));
                            const int DownloadBufferSize = 4096;
                            var totalBytesReceived = await responseStream.CopyToAsync(writer, DownloadBufferSize, progress, _ripper.CancellationToken);
                            if (totalBytesToReceive <= 0)
                                _ripper.OnDownloadProgressChanged(new DownloadProgressChangedEventArgs(OriginalUrl, totalBytesReceived, totalBytesReceived));
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
