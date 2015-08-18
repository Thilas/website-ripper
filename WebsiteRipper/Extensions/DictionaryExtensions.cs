using System;
using System.Collections.Generic;

namespace WebsiteRipper.Extensions
{
    static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");
            if (key == null) throw new ArgumentNullException("key");
            TValue value;
            if (dictionary.TryGetValue(key, out value)) return value;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out value)) return value;
                value = valueFactory(key);
                dictionary.Add(key, value);
                return value;
            }
        }

        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");
            if (key == null) throw new ArgumentNullException("key");
            if (dictionary.ContainsKey(key)) return false;
            lock (dictionary)
            {
                if (dictionary.ContainsKey(key)) return false;
                dictionary.Add(key, value);
                return true;
            }
        }
    }
}
