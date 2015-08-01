using System;
using System.Xml;

namespace WebsiteRipper.Extensions
{
    static class XmlDocumentExtensions
    {
        public static XmlElement GetDocumentElement(this XmlDocument document)
        {
            if (document == null) throw new ArgumentNullException("document");
            var documentElement = document.DocumentElement;
            if (documentElement == null) throw new InvalidOperationException("Document has no document element.");
            return documentElement;
        }
    }
}
