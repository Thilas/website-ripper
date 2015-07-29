using System;
using System.Collections.Generic;
using System.Linq;

namespace WebsiteRipper.Extensions
{
    static class TypeExtensions
    {
        public static T GetCustomAttribute<T>(this Type type, bool inherit) where T : Attribute
        {
            return GetCustomAttributes<T>(type, inherit).SingleOrDefault();
        }

        // TODO: Use this extension
        public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).Cast<T>();
        }
    }
}
