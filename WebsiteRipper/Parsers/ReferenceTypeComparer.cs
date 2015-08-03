using System;
using System.Collections.Generic;

namespace WebsiteRipper.Parsers
{
    sealed class ReferenceTypeComparer : IEqualityComparer<ReferenceType>
    {
        public static ReferenceTypeComparer Comparer { get; private set; }

        static ReferenceTypeComparer() { Comparer = new ReferenceTypeComparer(); }

        ReferenceTypeComparer() { }

        public bool Equals(ReferenceType x, ReferenceType y)
        {
            if (x == null ^ y == null) return false;
            if (x == null) return true;
            return x.Type == y.Type && x.Kind == y.Kind && string.Equals(x.AttributeName, y.AttributeName, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(ReferenceType obj)
        {
            return Tuple.Create(obj.Type, obj.Kind, obj.AttributeName).GetHashCode();
        }
    }
}
