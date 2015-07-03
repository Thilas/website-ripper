using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        Dictionary<Uri, Resource> _urls;
        Dictionary<Resource, Uri> _resources;

        public Resource Resource { get; private set; }
        public string RootPath { get; private set; }

        public CultureInfo[] Languages { get; private set; }

        private static IEnumerable<string> GetPreferredLanguages(CultureInfo language)
        {
            while (!string.IsNullOrEmpty(language.IetfLanguageTag))
            {
                yield return language.IetfLanguageTag;
                language = language.Parent;
            }
        }

        string _preferredLanguages = null;
        internal string PreferredLanguages
        {
            get
            {
                if (_preferredLanguages != null) return _preferredLanguages;
                var languages = Languages.SelectMany(GetPreferredLanguages).ToList();
                var qualityDecrement = 1.0 / (languages.Count + 1);
                var numberFormatInfo = new NumberFormatInfo() { NumberDecimalSeparator = "." };
                _preferredLanguages = string.Join(",", languages.Select((language, number) =>
                {
                    var quality = 1.0 - number * qualityDecrement;
                    return string.Format(number == 0 ? "{0}" : "{0};q={1}", language, quality.ToString(numberFormatInfo));
                }));
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
        internal CancellationToken CancellationToken { get; private set; }

        static CultureInfo[] GetDefaultLanguages()
        {
            var defaultLanguages = new[] { CultureInfo.CurrentUICulture, CultureInfo.GetCultureInfoByIetfLanguageTag("en-US") };
            return defaultLanguages.Distinct().ToArray();
        }

        public Ripper(string url, string rootPath) : this(url, rootPath, GetDefaultLanguages()) { }

        public Ripper(string url, string rootPath, CultureInfo language) : this(url, rootPath, new[] { language }) { }

        public Ripper(string url, string rootPath, CultureInfo[] languages)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");
            if (string.IsNullOrEmpty(rootPath)) throw new ArgumentNullException("rootPath");
            if (languages == null) throw new ArgumentNullException("languages");
            // TODO: Check ServicePointManager.DefaultConnectionLimit
            ServicePointManager.DefaultConnectionLimit = 1000;
            RootPath = Path.GetFullPath(rootPath);
            Languages = languages;
            Timeout = 30000;
            IsBase = false;
            MaxDepth = 0;
            Resource = new Resource(this, new Uri(url));
        }

        public void Rip(RipMode ripMode)
        {
            RipAsync(ripMode).Wait(CancellationToken);
        }

        public async Task RipAsync(RipMode ripMode)
        {
            if (_cancellationTokenSource != null) throw new InvalidOperationException();
            _urls = new Dictionary<Uri, Resource>();
            _resources = new Dictionary<Resource, Uri>();
            try
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    CancellationToken = _cancellationTokenSource.Token;
                    _urls.Add(Resource.OriginalUrl, Resource);
                    _resources.Add(Resource, Resource.OriginalUrl);
                    if (ripMode == RipMode.Create && Directory.Exists(RootPath)) Directory.Delete(RootPath, true);
                    await Resource.RipAsync(ripMode, 0);
                }
            }
            finally
            {
                foreach (var resource in _resources.Keys) resource.Dispose();
            }
        }

        public void Cancel()
        {
            if (_cancellationTokenSource == null) throw new InvalidOperationException();
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
