using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Autofac;

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
            var query = MvcQuery.Parse(context.Request.Path.Value, _controllersMap);

            if (query != null)
            {
                var success = false;
                var returnValue = TryRunControllerMethod(query, ref success);

                if (success)
                {
                    var templateContent = File.ReadAllText(ViewsFolderName + "/" + query.Controller + "/" + query.Method + ViewFileExtension);
                    var result = Razor.Parse(templateContent, returnValue);

                    await context.Response.WriteAsync(result);
                }

                return;
            }

            await Next.Invoke(context);
        }

        private object TryRunControllerMethod(MvcQuery query, ref bool methodRun)
        {
            var controllerType = _controllersMap[query.Controller];
            var instance = _container.Resolve(controllerType);
            var controllerMethod = controllerType.GetMethods().FirstOrDefault(method => method.Name == query.Method);
            var returnValue = controllerMethod.Invoke(instance, new object[] { });
            if (controllerMethod != null)
            {
                methodRun = true;
            }
            return returnValue;
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