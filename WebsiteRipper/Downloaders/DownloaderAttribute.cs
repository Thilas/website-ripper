using System;

namespace WebsiteRipper.Downloaders
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class DownloaderAttribute : Attribute
    {
        private readonly string _scheme;
        public string Scheme { get { return _scheme; } }

        public DownloaderAttribute(string scheme)
        {
            if (scheme == null) throw new ArgumentNullException("scheme");
            _scheme = scheme;
        }
    }
}
