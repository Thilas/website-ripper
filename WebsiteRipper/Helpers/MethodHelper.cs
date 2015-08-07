using System;
using System.Linq.Expressions;
using System.Reflection;

namespace WebsiteRipper.Helpers
{
    static class MethodHelper<T>
    {
        static MethodHelper()
        {
            var type = typeof(T);
            if (!typeof(Delegate).IsAssignableFrom(type))
                throw new InvalidOperationException(string.Format("{0} is not a delegate.", type.Name));
        }

        public static MethodInfo GetMethodInfo(Expression<T> expression)
        {
            var member = expression.Body as MethodCallExpression;
            if (member == null) throw new ArgumentException("Expression is not a method.", "expression");
            return member.Method;
        }
    }
}
