using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Humanizer;

using Microsoft.Owin;

namespace Mandro.Blog.Worker.Infrastructure
{
    internal class MvcQuery
    {
        private const string DefaultController = "Home";
        private const string DefaultControllerMethod = "Index";

        public static async Task<MvcQuery> ParseAsync(IOwinRequest request, IDictionary<string, Type> controllersMap)
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

                    var formCollection = await request.ReadFormAsync();
                    mvcQuery.Parameters = formCollection.ToDictionary(key => key.Key, value => value.Value.FirstOrDefault());

                    return mvcQuery;
                }
            }

            return null;
        }

        public Dictionary<string, string> Parameters { get; set; }

        public string Method { get; private set; }

        public string Controller { get; private set; }
    }
}