using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

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

        public static Resource GetActualResource(WebTestInfo webTest)
        {
            return GetActual((resource, subResources) => resource, webTest, null);
        }

        public static IEnumerable<Resource> GetExpectedSubResources(params Resource[] resources)
        {
            return resources.ToList();
        }

        public static IEnumerable<Resource> GetActualSubResources(WebTestInfo webTest)
        {
            return GetActualSubResources(webTest, null);
        }

        public static IEnumerable<Resource> GetActualSubResources(WebTestInfo webTest, params WebTestInfo[] subWebTests)
        {
            return GetActual((resource, subResources) => subResources, webTest, subWebTests);
        }

        static T GetActual<T>(Func<Resource, IEnumerable<Resource>, T> selector, WebTestInfo webTest, params WebTestInfo[] subWebTests)
        {
            lock (_webTests)
            {
                var rootPath = Path.GetTempFileName();
                File.Delete(rootPath);
                try
                {
                    _locked = true;
                    _webTests.Add(webTest.Url, webTest);
                    if (subWebTests != null)
                        foreach (var subWebTest in subWebTests) _webTests.Add(subWebTest.Url, subWebTest);
                    var ripper = new Ripper(webTest.Url, rootPath);
                    var subResources = ripper.Resource.GetResources(RipMode.Create, 0).Result.ToList();
                    return selector(ripper.Resource, subResources);
                }
                finally
                {
                    _webTests.Clear();
                    if (Directory.Exists(rootPath)) Directory.Delete(rootPath, true);
                    _locked = false;
                }
            }
        }

        WebTest() { }

        public WebRequest Create(Uri url)
        {
            if (url == null) throw new ArgumentNullException("url");
            if (!_locked) throw new NotSupportedException("WebTest does not support web request creation in the current context.");
            WebTestInfo webTest;
            if (!_webTests.TryGetValue(url, out webTest)) throw new NotSupportedException(string.Format("WebTest does not support url \"{0}\".", url));
            return new WebTestRequest(url, webTest);
        }
    }
}
