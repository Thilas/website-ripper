using ExCSS;

namespace WebsiteRipper.Parsers.Css
{
    public sealed class PrimitiveTermReference : Reference
    {
        readonly PrimitiveTerm _primitiveTerm;

        public PrimitiveTermReference(Parser parser, PrimitiveTerm primitiveTerm)
            : base(parser, ReferenceKind.ExternalResource)
        {
            _primitiveTerm = primitiveTerm;
        }

        protected override string InternalUrl
        {
            get { return (string)_primitiveTerm.Value; }
            set { _primitiveTerm.Value = value; }
        }
    }
}
