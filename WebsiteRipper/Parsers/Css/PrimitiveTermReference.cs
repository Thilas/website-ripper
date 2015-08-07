using ExCSS;

namespace WebsiteRipper.Parsers.Css
{
    public sealed class PrimitiveTermReference : Reference
    {
        readonly PrimitiveTerm _primitiveTerm;

        public PrimitiveTermReference(Parser parser, PrimitiveTerm primitiveTerm)
            : base(new ReferenceArgs(parser, ReferenceKind.ExternalResource))
        {
            _primitiveTerm = primitiveTerm;
        }

        protected override string UriInternal
        {
            get { return (string)_primitiveTerm.Value; }
            set { _primitiveTerm.Value = value; }
        }
    }
}
