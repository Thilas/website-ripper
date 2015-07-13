using System;

namespace WebsiteRipper.Parsers.Html
{
    public sealed class DuplicateBaseHtmlElementException : Exception
    {
        internal DuplicateBaseHtmlElementException(Exception innerException) : base(null, innerException) { }

        public override string Message { get { return "Duplicate base html element."; } }
    }
}
