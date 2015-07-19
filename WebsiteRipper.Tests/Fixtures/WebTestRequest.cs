using System;
using System.Net;

namespace WebsiteRipper.Tests.Fixtures
{
    sealed class WebTestRequest : WebRequest
    {
        readonly Uri _uri;
        readonly WebTestInfo _webTest;

        internal WebTestRequest(Uri uri, WebTestInfo webTest)
        {
            _uri = uri;
            _webTest = webTest;
        }

        public override int Timeout { get; set; }

        public override WebResponse GetResponse()
        {
            return new WebTestResponse(_uri, _webTest);
        }
    }
}
