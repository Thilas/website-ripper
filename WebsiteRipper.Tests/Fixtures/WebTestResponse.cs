using System;
using System.IO;
using System.Net;

namespace WebsiteRipper.Tests.Fixtures
{
    sealed class WebTestResponse : WebResponse
    {
        readonly Uri _url;
        readonly WebTestInfo _webTest;

        internal WebTestResponse(Uri url, WebTestInfo webTest)
        {
            _url = url;
            _webTest = webTest;
        }

        public override long ContentLength
        {
            get { return _webTest.Content != null ? _webTest.Content.Length : 0L; }
        }

        public override string ContentType
        {
            get { return _webTest.MimeType; }
        }

        public override Uri ResponseUri { get { return _url; } }

        public override Stream GetResponseStream() { return _webTest.Stream; }
    }
}
