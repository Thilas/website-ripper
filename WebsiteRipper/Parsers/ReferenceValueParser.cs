using System;
using System.Collections.Generic;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Parsers
{
    public abstract class ReferenceValueParser
    {
        sealed class FakeReferenceValueParser : ReferenceValueParser
        {
            public override IEnumerable<string> GetUriStrings(string value) { throw new NotImplementedException(); }
        }

        static readonly Dictionary<Type, ReferenceValueParser> _valueParsers = new Dictionary<Type, ReferenceValueParser>();

        internal static ReferenceValueParser Create(Type type)
        {
            if (type == null) return null;
            return _valueParsers.GetOrAdd(type, _ =>
            {
                if (!typeof(ReferenceValueParser).IsAssignableFrom(type))
                    throw new ArgumentException("Type is not a ReferenceValueParser", "type");
                var constructor = type.GetConstructorOrDefault<Func<ReferenceValueParser>>(() => new FakeReferenceValueParser());
                return constructor != null ? constructor() : null;
            });
        }

        public abstract IEnumerable<string> GetUriStrings(string value);
    }
}
