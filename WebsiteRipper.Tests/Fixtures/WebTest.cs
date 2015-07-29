using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Tests.Fixtures
{
    sealed class WebTest : IWebRequestCreate
    {
        public static Encoding Encoding { get { return Encoding.UTF8; } }

        public const string Scheme = "test";

        public static Uri GetUri(string uriStringWithoutScheme)
        {
            return new Uri(String.Format("{0}://{1}", Scheme, uriStringWithoutScheme));
        }

        static readonly Dictionary<Uri, WebTestInfo> _webTests = new Dictionary<Uri, WebTestInfo>();
        static bool _locked = false;

        static WebTest()
        {
            if (!WebRequest.RegisterPrefix(String.Format("{0}:", Scheme), new WebTest()))
                throw new NotSupportedException(String.Format("WebRequest does not support registering scheme \"{0}\".", Scheme));
        }

        public static IEnumerable<Resource> GetExpectedResources(WebTestInfo webTest)
        {
            return GetExpectedResources(webTest, new Uri[] { });
        }

        public static IEnumerable<Resource> GetExpectedResources(WebTestInfo webTest, params string[] relativeUriStrings)
        {
            var uris = relativeUriStrings.Select(relativeUriString => new Uri(webTest.Uri, relativeUriString)).ToArray();
            return GetExpectedResources(webTest, uris);
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
                    return webTest.Ripper.Resource.RipAsync(RipMode.Create, 0).Result.ToList();
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
            if (!_webTests.TryGetValue(uri, out webTest)) throw new NotSupportedException(String.Format("WebTest does not support uri \"{0}\".", uri));
            return new WebTestRequest(uri, webTest);
        }
    }
}
