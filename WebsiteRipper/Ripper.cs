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

    // TODO: Rename Url into Uri

    public class Ripper
    {
        Dictionary<Uri, Resource> _urls;
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

        public Ripper(string url, string rootPath) : this(new Uri(url), rootPath) { }

        public Ripper(string url, string rootPath, CultureInfo language) : this(new Uri(url), rootPath, language) { }

        public Ripper(string url, string rootPath, IEnumerable<CultureInfo> languages)
        {
            if (url == null) throw new ArgumentNullException("url");
            Initialize(new Uri(url), rootPath, languages);
        }

        public Ripper(Uri url, string rootPath) : this(url, rootPath, GetDefaultLanguages()) { }

        public Ripper(Uri url, string rootPath, CultureInfo language) : this(url, rootPath, new[] { language }) { }

        public Ripper(Uri url, string rootPath, IEnumerable<CultureInfo> languages)
        {
            if (url == null) throw new ArgumentNullException("url");
            Initialize(url, rootPath, languages);
        }

        void Initialize(Uri url, string rootPath, IEnumerable<CultureInfo> languages)
        {
            if (rootPath == null) throw new ArgumentNullException("rootPath");
            if (languages == null) throw new ArgumentNullException("languages");
            ServicePointManager.DefaultConnectionLimit = 1000;
            RootPath = Path.GetFullPath(rootPath);
            Languages = languages;
            Timeout = DefaultExtensions.Timeout;
            IsBase = false;
            MaxDepth = 0;
            Resource = new Resource(this, url);
        }

        public void Rip(RipMode ripMode)
        {
            RipAsync(ripMode).Wait(CancellationToken);
        }

        public async Task RipAsync(RipMode ripMode)
        {
            if (_cancellationTokenSource != null) throw new InvalidOperationException("Ripping has already started.");
            _urls = new Dictionary<Uri, Resource>();
            _resources = new Dictionary<Resource, Uri>();
            try
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    _urls.Add(Resource.OriginalUrl, Resource);
                    _resources.Add(Resource, Resource.OriginalUrl);
                    if (ripMode == RipMode.Create && Directory.Exists(RootPath)) Directory.Delete(RootPath, true);
                    await Resource.RipAsync(ripMode, 0);
                }
            }
            finally
            {
                lock (_urls)
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
            var subUrl = reference.GetAbsoluteUrl(resource);
            if (subUrl == null) return null;
            var isInScope = (subUrl.Scheme == Uri.UriSchemeHttp || subUrl.Scheme == Uri.UriSchemeHttps) && (
                reference.Kind == ReferenceKind.ExternalResource ||
                (MaxDepth <= 0 || depth <= MaxDepth) && (
                    !IsBase && _includeRegex == null ||
                    IsBase && Resource.OriginalUrl.IsBaseOf(subUrl) ||
                    _includeRegex != null && _includeRegex.Value.IsMatch(subUrl.ToString())
                )
            );
            var subResource = isInScope ? GetResource(subUrl, reference.Kind == ReferenceKind.Hyperlink) : null;
            var relativeUrl = isInScope ? resource.NewUrl.MakeRelativeUri(new Uri(subResource.NewUrl, subUrl.Fragment)) : subUrl;
            reference.Url = Uri.UnescapeDataString(relativeUrl.OriginalString);
            return subResource;
        }

        Resource GetResource(Uri url, bool hyperlink)
        {
            Resource resource;
            if (_urls.TryGetValue(url, out resource)) return resource;
            lock (_urls)
            {
                if (_urls.TryGetValue(url, out resource)) return resource;
                try
                {
                    resource = new Resource(this, url, hyperlink);
                }
                catch (ResourceUnavailableException ex)
                {
                    resource = ex.Resource;
                }
                Uri sameUrl;
                if (_resources.TryGetValue(resource, out sameUrl))
                    resource = _urls[sameUrl];
                else
                    _resources.Add(resource, url);
                _urls.Add(url, resource);
            }
            return resource;
        }
    }
}
