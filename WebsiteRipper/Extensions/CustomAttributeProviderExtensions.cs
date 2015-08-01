using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WebsiteRipper.Extensions
{
    static class CustomAttributeProviderExtensions
    {
        public static T GetCustomAttribute<T>(this ICustomAttributeProvider customAttributeProvider, bool inherit) where T : Attribute
        {
            return GetCustomAttributes<T>(customAttributeProvider, inherit).SingleOrDefault();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this ICustomAttributeProvider customAttributeProvider, bool inherit) where T : Attribute
        {
            return customAttributeProvider.GetCustomAttributes(typeof(T), inherit).Cast<T>();
        }
    }
}
