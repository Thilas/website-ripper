using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using WebsiteRipper.Parsers;
using WebsiteRipper.Parsers.Xml;

namespace WebsiteRipper.Core
{
    sealed class DefaultExtensionsParser : XmlParser
    {
        public DefaultExtensionsParser(string mimeType) : base(mimeType) { }

        protected override string GetDefaultExtension() { return ".xml"; }

        static readonly Lazy<Regex> _subtypeNameRegexLazy = new Lazy<Regex>(() => new Regex(@"^\S+", RegexOptions.Compiled));

        protected override IEnumerable<Reference> GetReferences()
        {
            var namespaceManager = new XmlNamespaceManager(Document.NameTable);
            var documentElement = Document.DocumentElement;
            if (documentElement == null) throw new InvalidOperationException("Document has no document element.");
            const string assignmentPrefix = "assignment";
            namespaceManager.AddNamespace(assignmentPrefix, documentElement.NamespaceURI);
            var xPath = string.Format(
                "{0}:registry[@id='media-types']/{0}:registry[{0}:title!='']/{0}:record[{0}:name!='']/{0}:file[@type='template' and text()!='']/text()",
                assignmentPrefix);
            var fileTexts = documentElement.SelectNodes(xPath, namespaceManager);
            if (fileTexts == null) throw new InvalidOperationException("Document has no files.");
            return fileTexts.Cast<XmlText>().Select(fileText =>
            {
                var typeNameText = (XmlText)fileText.SelectSingleNode(string.Format("../../../{0}:title/text()", assignmentPrefix), namespaceManager);
                if (typeNameText == null) throw new InvalidOperationException("File has no type.");
                var subtypeNameText = (XmlText)fileText.SelectSingleNode(string.Format("../../{0}:name/text()", assignmentPrefix), namespaceManager);
                if (subtypeNameText == null) throw new InvalidOperationException("File has no subtype.");
                var subtypeNameMatch = _subtypeNameRegexLazy.Value.Match(subtypeNameText.Value);
                if (!subtypeNameMatch.Success) throw new InvalidOperationException("File has an invalid subtype.");
                return new DefaultExtensionsReference(this, typeNameText.Value, subtypeNameMatch.Value, fileText.Value);
            });
        }
    }
}
