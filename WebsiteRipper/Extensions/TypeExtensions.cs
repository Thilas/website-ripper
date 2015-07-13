using System;
using System.Linq;

namespace WebsiteRipper.Extensions
{
    static class TypeExtensions
    {
        public static T GetCustomAttribute<T>(this Type element, bool inherit) where T : Attribute
        {
            return element.GetCustomAttributes(typeof(T), inherit).FirstOrDefault() as T;
        }
    }
}
