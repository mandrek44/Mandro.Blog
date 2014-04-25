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
using RazorEngine.Configuration;
using RazorEngine.Templating;

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

            string viewPathTemplate = "SelfHost.Views.{0}";
            var templateConfig = new TemplateServiceConfiguration
                                 {
                                     Resolver = new DelegateTemplateResolver(
                                         name =>
                                         {
                                             return File.ReadAllText(name);
                                         })
                                 };
            Razor.SetTemplateService(new TemplateService(templateConfig));
        }

        public override async Task Invoke(IOwinContext context)
        {
            var query = await MvcQuery.ParseAsync(context.Request, _controllersMap);

            if (query == null)
            {
                await Next.Invoke(context);
                return;
            }

            if (!await TryAuthenticate(context, query))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var methodResult = await TryRunControllerMethod(context, query);
            if (methodResult.Success)
            {
                var templateContent = await ReadViewTemplate(query);
                var result = await Task.Run(() => Razor.Parse(templateContent, methodResult.Result));

                await context.Response.WriteAsync(result);
            }

            await Next.Invoke(context);
        }

        private async static Task<string> ReadViewTemplate(MvcQuery query)
        {
            var path = ViewsFolderName + "/" + query.Controller + "/" + query.Method + ViewFileExtension;
            using (var fileStream = File.OpenRead(path))
            using (var fileReader = new StreamReader(fileStream))
            {
                return await fileReader.ReadToEndAsync();
            }
        }

        private async Task<bool> TryAuthenticate(IOwinContext context, MvcQuery query)
        {
            var controllerType = _controllersMap[query.Controller];
            var controllerMethod = controllerType.GetMethods().FirstOrDefault(method => method.Name == query.Method);

            if (controllerType == null || controllerMethod == null)
            {
                return true;
            }

            if (NeedsAuthentication(controllerType) || NeedsAuthentication(controllerMethod))
            {
                var authenticateResult = await context.Authentication.AuthenticateAsync("Cookie");
                return authenticateResult != null && authenticateResult.Identity != null;
            }

            return true;
        }

        private static bool NeedsAuthentication(ICustomAttributeProvider controllerType)
        {
            return controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
        }

        private async Task<MethodResult> TryRunControllerMethod(IOwinContext context, MvcQuery query)
        {
            var controllerType = _controllersMap[query.Controller];
            var instance = _container.Resolve(controllerType);
            var controllerMethod = controllerType.GetMethods().FirstOrDefault(method => method.Name == query.Method);

            if (controllerMethod != null)
            {
                var methodParameterValues = GetMethodParameterValues(context, query.Parameters, controllerMethod);

                return await Task.Run(() => new MethodResult { Result = controllerMethod.Invoke(instance, methodParameterValues), Success = true });
            }
            else
            {
                return new MethodResult { Result = false };
            }
        }

        private static object[] GetMethodParameterValues(IOwinContext context, Dictionary<string, string> parameters, MethodInfo controllerMethod)
        {
            if (!controllerMethod.GetParameters().Any())
            {
                return new object[] { };
            }

            var expandoObject = new ExpandoObject();
            var dictionary = expandoObject as IDictionary<string, object>;

            foreach (var parameter in parameters)
            {
                dictionary.Add(parameter.Key.Dehumanize().Replace(" ", string.Empty), parameter.Value);
            }

            dictionary.Add("Context", context);

            return new object[] { expandoObject };
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

        private class MethodResult
        {
            public bool Success { get; set; }

            public object Result { get; set; }
        }
    }
}