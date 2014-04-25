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
            var mvcQuery = new MvcQuery();
            
            var potentialMethodName = string.Empty;
            var queryParts = new Queue<string>(query.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries));
                
            if (query == "/")
            {
                mvcQuery.Controller = DefaultController;
                mvcQuery.Method = methodName + DefaultControllerMethod;
            }
            else
            {
                if (!queryParts.Any())
                {
                    return null;
                }

                var controllerName = queryParts.Dequeue();
                if (!controllersMap.ContainsKey(controllerName))
                {
                    return null;
                }

                mvcQuery.Controller = controllerName;

                
                if (queryParts.Any())
                {
                    potentialMethodName = queryParts.Dequeue();
                    if (controllersMap[controllerName].GetMethods().Any(method => method.Name == methodName + potentialMethodName))
                    {
                        mvcQuery.Method = methodName + potentialMethodName;
                        potentialMethodName = string.Empty;
                    }
                    else
                    {
                        mvcQuery.Method = methodName + DefaultControllerMethod;
                    }
                }
                else
                {
                    mvcQuery.Method = methodName + DefaultControllerMethod;
                }
            }

            var formCollection = await request.ReadFormAsync();
            mvcQuery.Parameters = formCollection.ToDictionary(key => key.Key, value => value.Value.FirstOrDefault());

            int paramIndex = 1;
            if (potentialMethodName != string.Empty)
            {
                mvcQuery.Parameters.Add("Param" + paramIndex++, potentialMethodName);
            }
                    
            while (queryParts.Any())
            {
                mvcQuery.Parameters.Add("Param" + paramIndex++, queryParts.Dequeue());
            }

            return mvcQuery;
        }

        public Dictionary<string, string> Parameters { get; set; }

        public string Method { get; private set; }

        public string Controller { get; private set; }
    }
}