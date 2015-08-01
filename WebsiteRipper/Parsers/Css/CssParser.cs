using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ExCSS;

namespace WebsiteRipper.Parsers.Css
{
    [Parser(MimeType)]
    public sealed class CssParser : Parser
    {
        public const string MimeType = "text/css";

        protected override string DefaultFileNameWithoutExtension { get { return "style"; } }

        public CssParser(string mimeType) : base(mimeType) { }

        StyleSheet _styleSheet;
        Encoding _encoding;

        protected override void Load(string path)
        {
            using (var reader = new StreamReader(path, true))
            {
                var parser = new ExCSS.Parser();
                _styleSheet = parser.Parse(reader.ReadToEnd());
                _encoding = reader.CurrentEncoding;
            }
        }

        protected override IEnumerable<Reference> GetReferences()
        {
            return _styleSheet.ImportDirectives.Select(importRule => new ImportRuleReference(this, importRule)).Cast<Reference>()
                .Concat(GetPrimitiveTerms(_styleSheet.StyleRules
                    .SelectMany(styleRule => styleRule.Declarations)
                    .Select(declaration => declaration.Term))
                    .Where(primitiveTerm => primitiveTerm.PrimitiveType == UnitType.Uri)
                    .Select(primitiveTerm => new PrimitiveTermReference(this, primitiveTerm)));
        }

        static IEnumerable<PrimitiveTerm> GetPrimitiveTerms(IEnumerable<Term> terms)
        {
            foreach (var term in terms)
            {
                var primitiveTerm = term as PrimitiveTerm;
                if (primitiveTerm != null)
                    yield return primitiveTerm;
                else
                {
                    var termList = term as TermList;
                    if (termList == null) continue;
                    foreach (var subTerm in GetPrimitiveTerms(termList)) yield return subTerm;
                }
            }
        }

        protected override void Save(string path)
        {
            using (var writer = new StreamWriter(path, false, _encoding))
            {
                writer.WriteLine(_styleSheet.ToString(true));
            }
        }
    }
}
