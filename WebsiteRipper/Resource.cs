using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

        HttpWebResponse _httpWebResponse = null;

        public Uri NewUrl { get; private set; }
        public Uri OriginalUrl { get; private set; }

        public DateTime LastModified { get; private set; }

        internal Resource(Ripper ripper, Uri url, bool hyperlink = true)
        {
            if (ripper == null) throw new ArgumentNullException("ripper");
            if (url == null) throw new ArgumentNullException("url");
            _ripper = ripper;
            _hyperlink = hyperlink;
            // TODO: Handle other protocols
            var httpWebRequest = WebRequest.CreateHttp(url);
            httpWebRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, ripper.PreferredLanguages);
            httpWebRequest.Timeout = ripper.Timeout;
            try
            {
                _httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                LastModified = _httpWebResponse.LastModified;
            }
            catch (Exception exception)
            {
                var webException = exception as WebException;
                _httpWebResponse = webException != null ? (HttpWebResponse)webException.Response : null;
                LastModified = _httpWebResponse != null ? _httpWebResponse.LastModified : DateTime.Now;
                url = _httpWebResponse != null ? _httpWebResponse.ResponseUri : url;
                _parser = Parser.CreateDefault(_httpWebResponse != null ? _httpWebResponse.ContentType : null, url);
                OriginalUrl = url;
                NewUrl = GetNewUrl(url);
                //Dispose();
                throw new ResourceUnavailableException(this, exception);
            }
            url = _httpWebResponse.ResponseUri;
            _parser = Parser.Create(_httpWebResponse.ContentType);
            OriginalUrl = url;
            NewUrl = GetNewUrl(url);
        }

        public override bool Equals(object obj) { return Equals(obj as Resource); }

        public bool Equals(Resource resource) { return resource != null && NewUrl.Equals(resource.NewUrl); }

        public override int GetHashCode() { return NewUrl.GetHashCode(); }

        public override string ToString() { return OriginalUrl.ToString(); }

        internal void Dispose()
        {
            if (_httpWebResponse == null) return;
            _httpWebResponse.Dispose();
            _httpWebResponse = null;
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
            if (_httpWebResponse == null) return false;
            try
            {
                var path = NewUrl.LocalPath;
                if (ripMode != RipMode.Create && File.Exists(path) && File.GetLastWriteTime(path) >= _httpWebResponse.LastModified) return false;
                var parentPath = Path.GetDirectoryName(path);
                if (parentPath != null) Directory.CreateDirectory(parentPath);
                using (var responseStream = _httpWebResponse.GetResponseStream())
                {
                    try
                    {
                        using (var writer = new FileStream(path, ripMode == RipMode.Create ? FileMode.CreateNew : FileMode.Create, FileAccess.Write))
                        {
                            var totalBytesToReceive = _httpWebResponse.ContentLength;
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
