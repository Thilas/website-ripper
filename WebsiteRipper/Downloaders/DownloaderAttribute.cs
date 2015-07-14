using System;

namespace WebsiteRipper.Downloaders
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class DownloaderAttribute : Attribute
    {
        public string Scheme { get; private set; }

        public DownloaderAttribute(string scheme)
        {
            if (scheme == null) throw new ArgumentNullException("scheme");
            Scheme = scheme;
        }
    }
}
