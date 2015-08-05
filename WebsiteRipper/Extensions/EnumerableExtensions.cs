using System;
using System.Collections.Generic;
using System.Linq;

namespace WebsiteRipper.Extensions
{
    static class EnumerableExtensions
    {
        //public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T value)
        //{
        //    if (enumerable == null) throw new ArgumentNullException("enumerable");
        //    foreach (var item in enumerable) yield return item;
        //    yield return value;
        //}

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> enumerable, T value)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            yield return value;
            foreach (var item in enumerable) yield return item;
        }

        //public static IEnumerable<TResult> OuterJoin<T, TKey, TResult>(this IEnumerable<T> outer, IEnumerable<T> inner, Func<T, TKey> keySelector, Func<T, T, TResult> resultSelector)
        //{
        //    return OuterJoin(outer, inner, keySelector, keySelector, resultSelector, default(T), default(T), null);
        //}

        public static IEnumerable<TResult> OuterJoin<T, TKey, TResult>(this IEnumerable<T> outer, IEnumerable<T> inner, Func<T, TKey> keySelector, Func<T, T, TResult> resultSelector, T resultDefault, IEqualityComparer<TKey> comparer)
        {
            return outer.OuterJoin(inner, keySelector, keySelector, resultSelector, resultDefault, resultDefault, comparer);
        }

        //public static IEnumerable<TResult> OuterJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
        //{
        //    return OuterJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector, default(TOuter), default(TInner), null);
        //}

        public static IEnumerable<TResult> OuterJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, TOuter outerDefault, TInner innerDefault, IEqualityComparer<TKey> comparer)
        {
            if (outer == null) throw new ArgumentNullException("outer");
            if (inner == null) throw new ArgumentNullException("inner");
            if (outerKeySelector == null) throw new ArgumentNullException("outerKeySelector");
            if (innerKeySelector == null) throw new ArgumentNullException("innerKeySelector");
            if (resultSelector == null) throw new ArgumentNullException("resultSelector");

            comparer = comparer ?? EqualityComparer<TKey>.Default;
            var outerLookup = outer.ToLookup(outerKeySelector, comparer);
            var innerLookup = inner.ToLookup(innerKeySelector, comparer);

            var keys = new HashSet<TKey>(outerLookup.Select(grouping => grouping.Key), comparer);
            keys.UnionWith(innerLookup.Select(grouping => grouping.Key));

            var result = from key in keys
                         from outerValue in outerLookup[key].DefaultIfEmpty(outerDefault)
                         from innerValue in innerLookup[key].DefaultIfEmpty(innerDefault)
                         select resultSelector(outerValue, innerValue);

            return result;
        }
    }
}
