using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Autofac;

using Humanizer;

using Microsoft.Owin;

using RazorEngine;

namespace Mandro.Blog.Worker.Infrastructure
{
    public class SimpleMvcMiddleware : OwinMiddleware
    {
        private const string ViewFileExtension = ".cshtml";
        private const string ViewsFolderName = "Views";

        private Dictionary<string, Type> _controllersMap;

        private IContainer _container;

        public SimpleMvcMiddleware(OwinMiddleware next)
            : base(next)
        {
            InitializeContainer();
            LoadAssemblyControllers(typeof(SimpleMvcMiddleware).Assembly);
        }

        public override async Task Invoke(IOwinContext context)
        {
            var query = await MvcQuery.ParseAsync(context.Request, _controllersMap);

            if (query != null)
            {
                bool success;
                var returnValue = TryRunControllerMethod(query, out success);

                if (success)
                {
                    var templateContent = File.ReadAllText(ViewsFolderName + "/" + query.Controller + "/" + query.Method + ViewFileExtension);
                    var result = Razor.Parse(templateContent, returnValue);

                    await context.Response.WriteAsync(result);

                    return;
                }
            }

            await Next.Invoke(context);
        }

        private object TryRunControllerMethod(MvcQuery query, out bool success)
        {
            var controllerType = _controllersMap[query.Controller];
            var instance = _container.Resolve(controllerType);
            var controllerMethod = controllerType.GetMethods().FirstOrDefault(method => method.Name == query.Method);
            
           
            if (controllerMethod != null)
            {
                success = true;

                
                var methodParameterValues = GetMethodParameterValues(query.Parameters, controllerMethod);

                return controllerMethod.Invoke(instance, methodParameterValues);
            }
            else
            {
                success = false;
                return null;
            }
        }

        private static object[] GetMethodParameterValues(Dictionary<string, string> parameters, MethodInfo controllerMethod)
        {
            var methodParameterValues = new object[] { };

            if (controllerMethod.GetParameters().Any())
            {
                var expandoObject = new ExpandoObject();
                var dictionary = expandoObject as IDictionary<string, object>;

                foreach (var parameter in parameters)
                {
                    dictionary.Add(parameter.Key.Dehumanize(), parameter.Value);
                }

                methodParameterValues = new object[] { expandoObject };
            }
            return methodParameterValues;
        }

        private void LoadAssemblyControllers(Assembly assembly)
        {
            var controllers = assembly.GetTypes().Where(type => type.Namespace != null && type.Namespace.EndsWith("Controllers"));
            _controllersMap = controllers.ToDictionary(keyItem => keyItem.Name, valueItem => valueItem);
        }

        private void InitializeContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterAssemblyTypes(typeof(SimpleMvcMiddleware).Assembly).AsSelf().AsImplementedInterfaces();
            _container = containerBuilder.Build();
        }
    }
}