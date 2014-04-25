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
            return new MvcQuery
                   {
                       Controller = GetControllerName(request, controllersMap), 
                       Method = GetMethodName(request, controllersMap), 
                       Parameters = await GetParameters(request, controllersMap)
                   };
        }

        private static async Task<IDictionary<string, string>> GetParameters(IOwinRequest request, IDictionary<string, Type> controllersMap)
        {
            var methodName = request.Method.ToLower().Pascalize();
            var query = request.Path.Value;
            var queryParts = new Queue<string>(query.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries));

            if (!queryParts.Any())
            {
                return new Dictionary<string, string>();
            }

            var controllerName = queryParts.Dequeue();
            if (!controllersMap.ContainsKey(controllerName))
            {
                return new Dictionary<string, string>();
            }

            int paramIndex = 1;
            var formCollection = await request.ReadFormAsync();
            var parameters = formCollection.ToDictionary(key => key.Key, value => value.Value.FirstOrDefault());

            if (queryParts.Any())
            {
                var potentialMethodName = queryParts.Dequeue();
                if (controllersMap[controllerName].GetMethods().All(method => method.Name != methodName + potentialMethodName))
                {
                    parameters.Add("Param" + paramIndex++, potentialMethodName);
                }
            }

            while (queryParts.Any())
            {
                parameters.Add("Param" + paramIndex++, queryParts.Dequeue());
            }

            return parameters;
        }

        private static string GetControllerName(IOwinRequest request, IDictionary<string, Type> controllersMap)
        {
            var query = request.Path.Value;

            if (query == "/")
            {
                return DefaultController;
            }

            var controllerName = query.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!controllersMap.ContainsKey(controllerName))
            {
                return null;
            }

            return controllerName;
        }

        private static string GetMethodName(IOwinRequest request, IDictionary<string, Type> controllersMap)
        {
            var methodName = request.Method.ToLower().Pascalize();
            var query = request.Path.Value;

            if (query == "/")
            {
                return methodName + DefaultControllerMethod;
            }

            var queryParts = new Queue<string>(query.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries));
            if (!queryParts.Any())
            {
                return null;
            }

            var controllerName = queryParts.Dequeue();
            if (!controllersMap.ContainsKey(controllerName))
            {
                return null;
            }

            if (queryParts.Any())
            {
                var potentialMethodName = queryParts.Dequeue();
                if (controllersMap[controllerName].GetMethods().Any(method => method.Name == methodName + potentialMethodName))
                {
                    return methodName + potentialMethodName;
                }
            }

            return methodName + DefaultControllerMethod;
        }

        public IDictionary<string, string> Parameters { get; set; }

        public string Method { get; private set; }

        public string Controller { get; private set; }
    }
}