using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace WebsiteRipper.Tests.Fixtures
{
    sealed class WebTestInfo
    {
        public string Name { get; private set; }

        public Uri Url { get { return new Uri(string.Format("{0}://{1}", WebTest.Scheme, Name)); } }

        public string MimeType { get; private set; }

        public string Content { get; private set; }

        readonly Lazy<Stream> _streamLazy;
        public Stream Stream { get { return _streamLazy.Value; } }

        static string GetName()
        {
            var factAttributeType = typeof(FactAttribute);
            var theoryAttributeType = typeof(TheoryAttribute);
            var factMethod = new StackTrace().GetFrames().Select(frame => frame.GetMethod())
                .First(method => method.GetCustomAttributes(factAttributeType, false).Length > 0 || method.GetCustomAttributes(theoryAttributeType, false).Length > 0);
            return string.Format("{0}.{1}", factMethod.DeclaringType.FullName, factMethod.Name);
        }

        public WebTestInfo(string mimeType, string content) : this(GetName(), mimeType, content) { }

        public WebTestInfo(string name, string mimeType, string content)
        {
            Name = name;
            MimeType = mimeType;
            Content = content;
            _streamLazy = new Lazy<Stream>(() => new MemoryStream(Encoding.UTF8.GetBytes(Content)));
        }
    }
}
