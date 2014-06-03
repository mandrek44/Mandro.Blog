using System;
using System.Text;
using System.Threading.Tasks;

using Mandro.Blog.Worker.Engine;

using Microsoft.Owin;
using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;

using Owin;

namespace Mandro.Blog.Worker.Infrastructure
{
    public class BlogStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.Use<TraceMiddleware>();
            appBuilder.UseErrorPage(new ErrorPageOptions()
                                    {
                                        ShowExceptionDetails = true,
                                        ShowSourceCode = true,
                                        ShowEnvironment = true
                                    });

            appBuilder.UseCookieAuthentication(new CookieAuthenticationOptions
                                               {
                                                   AuthenticationType = "Cookie",
                                               });

            appBuilder.Use<SimpleMvcMiddleware>();
            
            appBuilder.UseFileServer();
        }
    }
}