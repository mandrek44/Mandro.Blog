using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mandro.Blog.Worker.Engine
{
    public class Redirect
    {
        public static Uri To<T>(Expression<Func<T, Func<dynamic, dynamic>>> exprTree, object[] parameters = null)
        {
            var controllerType = exprTree.Parameters[0].Type;

            var method = (((exprTree.Body as UnaryExpression).Operand as MethodCallExpression).Object as ConstantExpression).Value as MethodInfo;

            return GetUrl(controllerType, method.Name, parameters);
        }

        private static Uri GetUrl(Type controllerType, string methodName, object[] parameters)
        {
            var parametersString = parameters != null ? string.Join("/", parameters.Select(property => property.ToString())) : string.Empty;
            if (!string.IsNullOrEmpty(parametersString)) parametersString = "/" + parametersString;

            methodName = GetMethodNameString(methodName);

            return new Uri("/" + controllerType.Name + "/" + methodName + parametersString, UriKind.Relative);
        }

        private static string GetMethodNameString(string methodName)
        {
            if (methodName.StartsWith("Get"))
            {
                return methodName.Substring("Get".Length);
            }
            else if (methodName.StartsWith("Post"))
            {
                return methodName.Substring("Post".Length);
            }
            else
            {
                return methodName;
            }
        }
    }
}