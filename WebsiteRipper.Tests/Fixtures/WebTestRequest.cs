using System;
using System.Net;

namespace WebsiteRipper.Tests.Fixtures
{
    sealed class WebTestRequest : WebRequest
    {
        readonly Uri _url;
        readonly WebTestInfo _webTest;

        internal WebTestRequest(Uri url, WebTestInfo webTest)
        {
            _url = url;
            _webTest = webTest;
        }

        public override int Timeout { get; set; }

        public override WebResponse GetResponse()
        {
            return new WebTestResponse(_url, _webTest);
        }
    }
}
