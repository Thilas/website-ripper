using System;
using System.Linq.Expressions;
using WebsiteRipper.Helpers;

namespace WebsiteRipper.Extensions
{
    static class TypeExtensions
    {
        public static T GetConstructorOrDefault<T>(this Type type, Expression<T> expression)
        {
            return ConstructorHelper<T>.GetConstructorOrDefault(type, expression);
        }
    }
}
