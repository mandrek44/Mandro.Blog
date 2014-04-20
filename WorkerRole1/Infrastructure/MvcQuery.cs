using System;
using System.Collections.Generic;
using System.Linq;

using Humanizer;

using Microsoft.Owin;

namespace Mandro.Blog.Worker.Infrastructure
{
    internal class MvcQuery
    {
        private const string DefaultController = "Home";
        private const string DefaultControllerMethod = "Index";

        public static MvcQuery Parse(IOwinRequest request, IDictionary<string, Type> controllersMap)
        {
            var methodName = request.Method.ToLower().Pascalize();

            var query = request.Path.Value;
            if (query == "/")
            {
                return new MvcQuery { Controller = DefaultController, Method = methodName + DefaultControllerMethod };
            }

            var mvcQuery = new MvcQuery();

            var queryParts = new Queue<string>(query.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries));
            if (queryParts.Any())
            {
                var controllerName = queryParts.Dequeue();
                if (controllersMap.ContainsKey(controllerName))
                {
                    mvcQuery.Controller = controllerName;

                    if (queryParts.Any())
                    {
                        mvcQuery.Method = methodName + queryParts.Dequeue();
                    }
                    else
                    {
                        mvcQuery.Method = methodName + DefaultControllerMethod;
                    }

                    return mvcQuery;
                }
            }

            return null;
        }

        public string Method { get; private set; }

        public string Controller { get; private set; }
    }
}