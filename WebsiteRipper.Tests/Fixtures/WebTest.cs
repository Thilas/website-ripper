using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Tests.Fixtures
{
    sealed class WebTest : IWebRequestCreate
    {
        public const string Scheme = "website-ripper-test";

        static readonly Dictionary<Uri, WebTestInfo> _webTests = new Dictionary<Uri, WebTestInfo>();
        static bool _locked = false;

        static WebTest()
        {
            if (!WebRequest.RegisterPrefix(string.Format("{0}:", Scheme), new WebTest())) throw new NotSupportedException(string.Format("WebRequest does not support registering scheme \"{0}\".", Scheme));
        }

        public static IEnumerable<Resource> GetExpectedResources(WebTestInfo webTest, params Uri[] uris)
        {
            return Resource.Create(webTest.Ripper, uris.Prepend(webTest.Uri).ToArray()).ToList();
        }

        public static IEnumerable<Resource> GetActualResources(WebTestInfo webTest, params WebTestInfo[] subWebTests)
        {
            lock (_webTests)
            {
                try
                {
                    _locked = true;
                    _webTests.Add(webTest.Uri, webTest);
                    foreach (var subWebTest in subWebTests) _webTests.Add(subWebTest.Uri, subWebTest);
                    var resource = webTest.Ripper.Resource;
                    return resource.RipAsync(RipMode.Create, 0).Result.ToList();
                }
                catch (AggregateException aggregateException)
                {
                    if (aggregateException.InnerExceptions.Count == 1)
                        throw aggregateException.InnerException;
                    else
                        throw;
                }
                finally
                {
                    _webTests.Clear();
                    _locked = false;
                }
            }
        }

        WebTest() { }

        public WebRequest Create(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (!_locked) throw new NotSupportedException("WebTest does not support web request creation in current context.");
            WebTestInfo webTest;
            if (!_webTests.TryGetValue(uri, out webTest)) throw new NotSupportedException(string.Format("WebTest does not support uri \"{0}\".", uri));
            return new WebTestRequest(uri, webTest);
        }
    }
}
