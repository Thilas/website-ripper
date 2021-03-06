﻿using ExCSS;

namespace WebsiteRipper.Parsers.Css
{
    public sealed class ImportRuleReference : Reference
    {
        readonly ImportRule _importRule;

        public ImportRuleReference(Parser parser, ImportRule importRule)
            : base(new ReferenceArgs(parser, ReferenceKind.ExternalResource))
        {
            _importRule = importRule;
        }

        protected override string ValueInternal
        {
            get { return _importRule.Href; }
            set { _importRule.Href = value; }
        }
    }
}
