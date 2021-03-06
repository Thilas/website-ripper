﻿using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace WebsiteRipper.Parsers.Html
{
    [Parser(MimeType)]
    public sealed class HtmlParser : Parser
    {
        public const string MimeType = "text/html";

        protected override string DefaultFileNameWithoutExtension { get { return "index"; } }

        public HtmlParser(ParserArgs parserArgs) : base(parserArgs) { }

        public Uri BaseUri { get; private set; }

        HtmlDocument _document = null;

        protected override void Load(string path)
        {
            _document = new HtmlDocument();
            _document.Load(path, true);
            BaseUri = GetBaseUri();
        }

        protected override IEnumerable<Reference> GetReferences()
        {
            return _document.DocumentNode.DescendantsAndSelf()
                .SelectMany(node => HtmlReference.Create(this, node));
        }

        readonly HashSet<HtmlAttribute> _attributes = new HashSet<HtmlAttribute>();

        internal void Remove(HtmlAttribute attribute)
        {
            lock (_attributes)
            {
                if (_attributes.Contains(attribute)) return;
                AnyChange = true;
                _attributes.Add(attribute);
            }
        }

        protected override void Save(string path)
        {
            lock (_attributes)
            {
                foreach (var attribute in _attributes) attribute.Remove();
            }
            _document.Save(path);
        }

        /// <summary>
        /// Gets the base URL.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DuplicateBasesException"></exception>
        /// <remarks>
        /// Gets the unique (if it exists) base node with at least an href or a target attribute,
        /// remove the href attribute (or the whole node if no href) and returns its href value (or null if no href).
        /// </remarks>
        Uri GetBaseUri()
        {
            const string hrefAttributeName = "href";
            const string targetAttributeName = "target";
            HtmlNode baseNode;
            try
            {
                baseNode = _document.DocumentNode.Descendants("base").
                    SingleOrDefault(node => node.Attributes[hrefAttributeName] != null || node.Attributes[targetAttributeName] != null);
            }
            catch (Exception exception) { throw new DuplicateBasesException(exception); }
            if (baseNode == null) return null;
            var hrefAttribute = baseNode.Attributes[hrefAttributeName];
            if (hrefAttribute == null) return null;
            Uri baseUri;
            if (!Uri.TryCreate(hrefAttribute.Value, UriKind.Absolute, out baseUri)) baseUri = null;
            AnyChange = true;
            if (baseNode.Attributes[targetAttributeName] == null)
                baseNode.Remove();
            else
                hrefAttribute.Remove();
            return baseUri;
        }
    }
}
