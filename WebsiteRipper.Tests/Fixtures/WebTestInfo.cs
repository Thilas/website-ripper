using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using WebsiteRipper.Parsers;
using Xunit;

namespace WebsiteRipper.Tests.Fixtures
{
    sealed class WebTestInfo : IDisposable
    {
        readonly string _rootPath;
        readonly Lazy<Ripper> _ripperLazy;
        public Ripper Ripper { get { return _ripperLazy.Value; } }

        public Uri Uri { get; private set; }

        public string MimeType { get; private set; }

        public string Content { get; private set; }

        readonly Lazy<Stream> _streamLazy;
        public Stream Stream { get { return _streamLazy.Value; } }

        static string GetName()
        {
            var stackFrames = new StackTrace().GetFrames();
            if (stackFrames == null) throw new InvalidOperationException("Stack trace has no stack frames.");
            var factMethod = stackFrames.Select(frame => frame.GetMethod())
                .First(method => method.GetCustomAttributes<FactAttribute>(false).Any() || method.GetCustomAttributes<TheoryAttribute>(false).Any());
            if (factMethod.DeclaringType == null) throw new InvalidOperationException("Stack frame has no declaring type.");
            return string.Format("{0}.{1}", factMethod.DeclaringType.FullName, factMethod.Name);
        }

        public WebTestInfo(string mimeType, string content) : this(GetName(), mimeType, content) { }

        public WebTestInfo(string name, string mimeType, string content) : this(null, WebTest.GetUri(name), mimeType, content) { }

        public WebTestInfo(WebTestInfo webTest, string relativeUri) : this(webTest, new Uri(webTest.Uri, relativeUri), DefaultParser.MimeType, null) { }

        public WebTestInfo(WebTestInfo webTest, string relativeUri, string mimeType, string content) : this(webTest, new Uri(webTest.Uri, relativeUri), mimeType, content) { }

        public WebTestInfo(WebTestInfo webTest, Uri uri, string mimeType, string content)
        {
            _rootPath = webTest == null ? Path.GetTempFileName() : null;
            if (_rootPath != null) File.Delete(_rootPath);
            _ripperLazy = new Lazy<Ripper>(() => _rootPath != null ? new Ripper(Uri, _rootPath) : null);
            Uri = uri;
            MimeType = mimeType;
            Content = content;
            _streamLazy = new Lazy<Stream>(() => Content != null ? new MemoryStream(WebTest.Encoding.GetBytes(Content)) : new MemoryStream());
        }

        public void Dispose()
        {
            if (_rootPath != null && Directory.Exists(_rootPath)) Directory.Delete(_rootPath, true);
        }
    }
}
