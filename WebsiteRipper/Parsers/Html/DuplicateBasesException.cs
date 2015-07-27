using System;

namespace WebsiteRipper.Parsers.Html
{
    public sealed class DuplicateBasesException : Exception
    {
        internal DuplicateBasesException(Exception innerException) : base(null, innerException) { }

        public override string Message { get { return "Duplicate base html element."; } }
    }
}
