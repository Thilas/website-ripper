using System;
using System.Collections.Generic;

namespace WebsiteRipper.Parsers.Html
{
    sealed class FullHtmlReferenceTypeComparer : IEqualityComparer<FullHtmlReferenceType>
    {
        public static FullHtmlReferenceTypeComparer Comparer { get; private set; }

        static FullHtmlReferenceTypeComparer() { Comparer = new FullHtmlReferenceTypeComparer(); }

        FullHtmlReferenceTypeComparer() { }

        public bool Equals(FullHtmlReferenceType x, FullHtmlReferenceType y)
        {
            if (x == null ^ y == null) return false;
            if (x == null) return true;
            return x.Type == y.Type && x.Kind == y.Kind && string.Equals(x.AttributeName, y.AttributeName, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(FullHtmlReferenceType obj)
        {
            return new Tuple<Type, ReferenceKind, string>(obj.Type, obj.Kind, obj.AttributeName).GetHashCode();
        }
    }
}
