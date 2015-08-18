using System;
using System.Collections.Generic;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Parsers
{
    public class ReferenceArgsCreator<TElement, TAttribute>
    {
        static readonly Lazy<ReferenceArgsCreator<TElement, TAttribute>> _defaultArgsCreatorLazy = new Lazy<ReferenceArgsCreator<TElement, TAttribute>>(() =>
            new ReferenceArgsCreator<TElement, TAttribute>());

        static readonly Dictionary<Type, ReferenceArgsCreator<TElement, TAttribute>> _argsCreators = new Dictionary<Type, ReferenceArgsCreator<TElement, TAttribute>>();

        internal static ReferenceArgsCreator<TElement, TAttribute> Create(Type type)
        {
            if (type == null) return _defaultArgsCreatorLazy.Value;
            return _argsCreators.GetOrAdd(type, _ =>
            {
                if (!typeof(ReferenceArgsCreator<TElement, TAttribute>).IsAssignableFrom(type))
                    throw new ArgumentException(string.Format("Type is not a ReferenceArgsCreator<{0}, {1}>", typeof(TElement), typeof(TAttribute)), "type");
                var constructor = type.GetConstructorOrDefault<Func<ReferenceArgsCreator<TElement, TAttribute>>>(() => new ReferenceArgsCreator<TElement, TAttribute>());
                return constructor != null ? constructor() : _defaultArgsCreatorLazy.Value;
            });
        }

        public virtual ReferenceArgs<TElement, TAttribute> Create(Parser parser, ReferenceKind kind, TElement element, TAttribute attribute)
        {
            return new ReferenceArgs<TElement, TAttribute>(parser, kind, null, element, attribute);
        }
    }
}
