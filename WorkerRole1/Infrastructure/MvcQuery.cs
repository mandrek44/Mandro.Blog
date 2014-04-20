using System;
using System.Collections.Generic;
using System.Linq;

namespace Mandro.Blog.Worker.Infrastructure
{
    internal class MvcQuery
    {
        private const string DefaultController = "Home";
        private const string DefaultControllerMethod = "Index";

        public static MvcQuery Parse(string query, IDictionary<string, Type> controllersMap)
        {
            if (query == "/")
            {
                return new MvcQuery { Controller = DefaultController, Method = DefaultControllerMethod };
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
                        mvcQuery.Method = queryParts.Dequeue();
                    }
                    else
                    {
                        mvcQuery.Method = DefaultControllerMethod;
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