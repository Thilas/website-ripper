using System;
using System.Linq;
using System.Linq.Expressions;
using WebsiteRipper.Extensions;

namespace WebsiteRipper.Helpers
{
    sealed class ConstructorHelper<T>
    {
        static readonly ConstructorHelper<T> _constructorHelper = new ConstructorHelper<T>();

        public static T GetConstructorOrDefault(Type type, Expression<T> expression)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (expression == null) throw new ArgumentNullException("expression");
            var newExpression = expression.Body as NewExpression;
            if (newExpression == null) throw new ArgumentException("Expression is not a constructor.", "expression");
            if (type.IsAbstract || !_constructorHelper.ReturnType.IsAssignableFrom(type)) return default(T);
            var parameters = newExpression.Constructor.GetParameters().ToList();
            var types = parameters.Select(parameter => parameter.ParameterType).ToArray();
            var constructor = type.GetConstructor(types);
            if (constructor == null) return default(T);
            var delegateType = typeof(T).GetGenericTypeDefinition().MakeGenericType(types.Append(type).ToArray());
            var parameterExpressions = parameters.Select(parameter => Expression.Parameter(parameter.ParameterType, parameter.Name)).ToList();
            var lambdaExpression = Expression.Lambda(delegateType, Expression.New(constructor, parameterExpressions), parameterExpressions);
            return (T)(object)lambdaExpression.Compile();
        }

        Type ReturnType { get; set; }

        ConstructorHelper()
        {
            var type = typeof(T);
            if (!type.IsGenericType && !typeof(Delegate).IsAssignableFrom(type)) throw new InvalidOperationException(string.Format("{0} is not a generic delegate.", type.Name));
            var methodName = MethodHelper<Action<Action>>.GetMethodInfo(action => action.Invoke()).Name;
            var method = type.GetMethod(methodName);
            ReturnType = method.ReturnType;
            if (ReturnType == typeof(void)) throw new InvalidOperationException(string.Format("{0} does not return a value.", type.Name));
        }
    }
}
