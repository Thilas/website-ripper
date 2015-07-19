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
        Dictionary<Uri, Resource> _uris;
        Dictionary<Resource, Uri> _resources;

        public Resource Resource { get; private set; }
        public string RootPath { get; private set; }

        public IEnumerable<CultureInfo> Languages { get; private set; }

        string _preferredLanguages = null;
        internal string PreferredLanguages
        {
            get
            {
                if (_preferredLanguages == null) _preferredLanguages = Tools.GetPreferredLanguages(Languages);
                return _preferredLanguages;
            }
        }

        public int Timeout { get; set; }

        public bool IsBase { get; set; }
        public int MaxDepth { get; set; }

        string _includePattern = null;
        Lazy<Regex> _includeRegex = null;
        public string IncludePattern
        {
            get { return _includePattern; }
            set
            {
                _includePattern = value;
                _includeRegex = !string.IsNullOrEmpty(_includePattern) ? new Lazy<Regex>(() => new Regex(_includePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)) : null;
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

        public Ripper(string uri, string rootPath) : this(new Uri(uri), rootPath) { }

        public Ripper(string uri, string rootPath, CultureInfo language) : this(new Uri(uri), rootPath, language) { }

        public Ripper(string uri, string rootPath, IEnumerable<CultureInfo> languages)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            Initialize(new Uri(uri), rootPath, languages);
        }

        public Ripper(Uri uri, string rootPath) : this(uri, rootPath, GetDefaultLanguages()) { }

        public Ripper(Uri uri, string rootPath, CultureInfo language) : this(uri, rootPath, new[] { language }) { }

        public Ripper(Uri uri, string rootPath, IEnumerable<CultureInfo> languages)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            Initialize(uri, rootPath, languages);
        }

        void Initialize(Uri uri, string rootPath, IEnumerable<CultureInfo> languages)
        {
            if (rootPath == null) throw new ArgumentNullException("rootPath");
            if (languages == null) throw new ArgumentNullException("languages");
            ServicePointManager.DefaultConnectionLimit = 1000;
            RootPath = Path.GetFullPath(rootPath);
            Languages = languages;
            Timeout = DefaultExtensions.Timeout;
            IsBase = false;
            MaxDepth = 0;
            Resource = new Resource(this, uri);
        }

        public void Rip(RipMode ripMode)
        {
            RipAsync(ripMode).Wait(CancellationToken);
        }

        public async Task RipAsync(RipMode ripMode)
        {
            if (_cancellationTokenSource != null) throw new InvalidOperationException("Ripping has already started.");
            _uris = new Dictionary<Uri, Resource>();
            _resources = new Dictionary<Resource, Uri>();
            try
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    _uris.Add(Resource.OriginalUri, Resource);
                    _resources.Add(Resource, Resource.OriginalUri);
                    if (ripMode == RipMode.Create && Directory.Exists(RootPath)) Directory.Delete(RootPath, true);
                    await Resource.RipAsync(ripMode, 0);
                }
            }
            finally
            {
                lock (_uris)
                {
                    foreach (var resource in _resources.Keys) resource.Dispose();
                }
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
            var subUri = reference.GetAbsoluteUri(resource);
            if (subUri == null) return null;
            var isInScope = (subUri.Scheme == Uri.UriSchemeHttp || subUri.Scheme == Uri.UriSchemeHttps) && (
                reference.Kind == ReferenceKind.ExternalResource ||
                (MaxDepth <= 0 || depth <= MaxDepth) && (
                    !IsBase && _includeRegex == null ||
                    IsBase && Resource.OriginalUri.IsBaseOf(subUri) ||
                    _includeRegex != null && _includeRegex.Value.IsMatch(subUri.ToString())
                )
            );
            var subResource = isInScope ? GetResource(subUri, reference.Kind == ReferenceKind.Hyperlink) : null;
            var relativeUri = isInScope ? resource.NewUri.MakeRelativeUri(new Uri(subResource.NewUri, subUri.Fragment)) : subUri;
            reference.Uri = Uri.UnescapeDataString(relativeUri.OriginalString);
            return subResource;
        }

        Resource GetResource(Uri uri, bool hyperlink)
        {
            Resource resource;
            if (_uris.TryGetValue(uri, out resource)) return resource;
            lock (_uris)
            {
                if (_uris.TryGetValue(uri, out resource)) return resource;
                try
                {
                    resource = new Resource(this, uri, hyperlink);
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
