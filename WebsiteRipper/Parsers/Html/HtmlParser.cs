using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html
{
    [Parser("text/html")]
    public sealed class HtmlParser : Parser
    {
        public override string DefaultFile { get { return "index.html"; } }

        public override string[] OtherExtensions { get { return new[] { ".htm" }; } }

        public Uri BaseUrl { get; private set; }

        HtmlDocument _htmlDocument = null;

        protected override void Load(string path)
        {
            _htmlDocument = new HtmlDocument();
            _htmlDocument.Load(path, true);
            BaseUrl = GetBaseUrl();
        }

        protected override IEnumerable<Reference> GetReferences()
        {
            return _htmlDocument.DocumentNode.DescendantsAndSelf()
                .SelectMany(node => HtmlReference.Create(this, node));
        }

        protected override void Save(string path)
        {
            _htmlDocument.Save(path);
        }

        /// <summary>
        /// Gets the base URL.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DuplicateBaseHtmlElementException"></exception>
        /// <remarks>
        /// Gets the unique (if it exists) base element with at least an href or a target attribute,
        /// remove the href attribute (or the whole element if no href) and returns its href value (or null if no href).
        /// </remarks>
        Uri GetBaseUrl()
        {
            const string HrefAttributeName = "href";
            const string TargetAttributeName = "target";
            HtmlNode baseNode;
            try
            {
                baseNode = _htmlDocument.DocumentNode.Descendants("base").
                    SingleOrDefault(node => node.Attributes[HrefAttributeName] != null || node.Attributes[TargetAttributeName] != null);
            }
            catch (Exception exception) { throw new DuplicateBaseHtmlElementException(exception); }
            if (baseNode == null) return null;
            var hrefAttribute = baseNode.Attributes[HrefAttributeName];
            if (hrefAttribute == null) return null;
            Uri baseUrl;
            if (!Uri.TryCreate(hrefAttribute.Value, UriKind.Absolute, out baseUrl)) baseUrl = null;
            AnyChange = true;
            if (baseNode.Attributes[TargetAttributeName] == null)
                baseNode.Remove();
            else
                hrefAttribute.Remove();
            return baseUrl;
        }
    }
}
