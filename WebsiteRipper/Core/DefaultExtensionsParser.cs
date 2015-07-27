using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using WebsiteRipper.Parsers;
using WebsiteRipper.Parsers.Xml;
using WebsiteRipper.Properties;

namespace WebsiteRipper.Core
{
    sealed class DefaultExtensionsParser : XmlParser
    {
        public DefaultExtensionsParser(string mimeType) : base(mimeType) { }

        protected override string GetDefaultExtension() { return ".xml"; }

        static readonly Lazy<Regex> _subtypeNameRegexLazy = new Lazy<Regex>(() => new Regex(@"^\S+", RegexOptions.Compiled));

        protected override IEnumerable<Reference> GetReferences()
        {
            var xmlNamespaceManager = new XmlNamespaceManager(XmlDocument.NameTable);
            xmlNamespaceManager.AddNamespace("a", Settings.Default.IanaAssignmentsNamespace);
            var files = XmlDocument.SelectNodes("/a:registry[@id='media-types']/a:registry[a:title!='']/a:record[a:name!='']/a:file[@type='template' and text()!='']/text()", xmlNamespaceManager);
            return files != null ? files.OfType<XmlText>().Select(file =>
            {
                var record = file.ParentNode;
                if (record == null || (record = record.ParentNode) == null) return null;
                var registry = record.ParentNode;
                if (registry == null) return null;
                var typeName = registry.SelectSingleNode("a:title/text()", xmlNamespaceManager);
                if (typeName == null) return null;
                var subtypeName = record.SelectSingleNode("a:name/text()", xmlNamespaceManager);
                if (subtypeName == null) return null;
                var subtypeNameMatch = _subtypeNameRegexLazy.Value.Match(subtypeName.Value);
                if (!subtypeNameMatch.Success) return null;
                return new DefaultExtensionsReference(this, typeName.Value, subtypeNameMatch.Value, file);
            }).Where(reference => reference != null) : Enumerable.Empty<Reference>();
        }
    }
}
