using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebsiteRipper.Core;
using WebsiteRipper.Downloaders;
using WebsiteRipper.Parsers;

namespace WebsiteRipper
{
    public enum RipMode
    {
        Create
        // TODO: Not working yet
        //Update
    }

    public class Ripper
    {
        readonly Dictionary<Uri, Resource> _uris = new Dictionary<Uri, Resource>();
        readonly Dictionary<Resource, Uri> _resources = new Dictionary<Resource, Uri>();

        readonly Lazy<Resource> _resourceLazy;
        public Resource Resource { get { return _resourceLazy.Value; } }
        public string RootPath { get; private set; }

        public IEnumerable<CultureInfo> Languages { get; private set; }

        string _preferredLanguages = null;
        internal string PreferredLanguages
        {
            get
            {
                return _preferredLanguages ?? (_preferredLanguages = Tools.GetPreferredLanguages(Languages));
            }
        }

        public int Timeout { get; set; }

        public bool IsBase { get; set; }
        public int MaxDepth { get; set; }

        string _includePattern = null;
        Lazy<Regex> _includeRegexLazy = null;
        public string IncludePattern
        {
            get { return _includePattern; }
            set
            {
                _includePattern = value;
                _includeRegexLazy = !string.IsNullOrEmpty(_includePattern) ? new Lazy<Regex>(() => new Regex(_includePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)) : null;
            }
        }

        // TODO: Add ExcludePattern capability

        CancellationTokenSource _cancellationTokenSource;
        internal CancellationToken CancellationToken { get { return _cancellationTokenSource != null ? _cancellationTokenSource.Token : CancellationToken.None; } }

        static IEnumerable<CultureInfo> GetDefaultLanguages()
        {
            var defaultLanguages = new[] { CultureInfo.CurrentUICulture, DefaultExtensions.Language };
            return defaultLanguages.Distinct();
        }

        Ripper(IEnumerable<CultureInfo> languages, string rootPath)
        {
            if (languages == null) throw new ArgumentNullException("languages");
            if (rootPath == null) throw new ArgumentNullException("rootPath");
            ServicePointManager.DefaultConnectionLimit = 1000;
            Timeout = DefaultExtensions.Timeout;
            Languages = languages;
            IsBase = false;
            MaxDepth = 0;
            RootPath = Path.GetFullPath(rootPath);
        }

        public Ripper(string uriString, string rootPath) : this(uriString, rootPath, GetDefaultLanguages()) { }

        public Ripper(string uriString, string rootPath, CultureInfo language) : this(uriString, rootPath, new[] { language }) { }

        public Ripper(string uriString, string rootPath, IEnumerable<CultureInfo> languages)
            : this(languages, rootPath)
        {
            if (uriString == null) throw new ArgumentNullException("uriString");
            var uri = new Uri(uriString);
            _resourceLazy = new Lazy<Resource>(() => new Resource(this, uri));
        }

        public Ripper(Uri uri, string rootPath) : this(uri, rootPath, GetDefaultLanguages()) { }

        public Ripper(Uri uri, string rootPath, CultureInfo language) : this(uri, rootPath, new[] { language }) { }

        public Ripper(Uri uri, string rootPath, IEnumerable<CultureInfo> languages)
            : this(languages, rootPath)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            _resourceLazy = new Lazy<Resource>(() => new Resource(this, uri));
        }

        public void Rip(RipMode ripMode)
        {
            RipAsync(ripMode).Wait(CancellationToken);
        }

        public async Task RipAsync(RipMode ripMode)
        {
            if (_cancellationTokenSource != null) throw new InvalidOperationException("Ripping has already started.");
            try
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    _uris.Add(Resource.OriginalUri, Resource);
                    _resources.Add(Resource, Resource.OriginalUri);
                    // TODO: Handle not empty directory
                    if (ripMode == RipMode.Create && Directory.Exists(RootPath)) Directory.Delete(RootPath, true);
                    await Resource.RipAsync(ripMode, 0);
                }
            }
            finally
            {
                foreach (var resource in _resources.Keys) resource.Dispose();
                _resources.Clear();
                _uris.Clear();
            }
        }

        public void Cancel()
        {
            if (_cancellationTokenSource == null) throw new InvalidOperationException("Ripping has not started yet.");
            _cancellationTokenSource.Cancel();
        }

        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

        internal void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            if (DownloadProgressChanged != null) DownloadProgressChanged(this, e);
        }

        internal virtual Resource GetSubResource(int depth, Resource resource, Reference reference)
        {
            if (resource == null) throw new ArgumentNullException("resource");
            if (reference == null) throw new ArgumentNullException("reference");
            if (reference.Uri.StartsWith("#")) return null;
            var subUri = reference.GetAbsoluteUri(resource);
            if (subUri == null) return null;
            var isInScope = Downloader.Supports(subUri) && (
                reference.Kind == ReferenceKind.ExternalResource ||
                (MaxDepth <= 0 || depth <= MaxDepth) && (
                    !IsBase && _includeRegexLazy == null ||
                    IsBase && Resource.OriginalUri.IsBaseOf(subUri) ||
                    _includeRegexLazy != null && _includeRegexLazy.Value.IsMatch(subUri.ToString())
                )
            );
            var subResource = isInScope ? GetResource(subUri, reference.Kind == ReferenceKind.Hyperlink, reference.MimeType) : null;
            var relativeUri = isInScope ? resource.NewUri.MakeRelativeUri(new Uri(subResource.NewUri, subUri.Fragment)) : subUri;
            reference.Uri = Uri.UnescapeDataString(relativeUri.OriginalString);
            return subResource;
        }

        Resource GetResource(Uri uri, bool hyperlink, string mimeType)
        {
            Resource resource;
            if (_uris.TryGetValue(uri, out resource)) return resource;
            lock (_uris)
            {
                if (_uris.TryGetValue(uri, out resource)) return resource;
                try
                {
                    resource = new Resource(this, uri, hyperlink, mimeType);
                }
                catch (ResourceUnavailableException ex)
                {
                    resource = ex.Resource;
                }
                Uri sameUri;
                if (_resources.TryGetValue(resource, out sameUri))
                    resource = _uris[sameUri];
                else
                    _resources.Add(resource, uri);
                _uris.Add(uri, resource);
            }
            return resource;
        }
    }
}
