using System;

namespace WebsiteRipper
{
    public sealed class ResourceUnavailableException : Exception
    {
        public Resource Resource { get; private set; }

        internal ResourceUnavailableException(Resource resource, Exception innerException)
            : base(null, innerException)
        {
            Resource = resource;
        }

        public override string Message { get { return string.Format("Resource unavailable: {0}", Resource.OriginalUrl); } }
    }
}
