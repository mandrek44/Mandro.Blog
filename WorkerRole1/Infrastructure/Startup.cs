using System;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Owin;
using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;

using Owin;

namespace Mandro.Blog.Worker.Infrastructure
{
    public class Startup
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


            var opts = new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookie",
  //              CookieName = "foo",
//                AuthenticationMode = AuthenticationMode.Passive,
//                CookieSecure = CookieSecureOption.Always,
  //              ExpireTimeSpan = TimeSpan.FromMinutes(20),
    //            SlidingExpiration = true,
//                LoginPath = new PathString("/login.cshtml")

            };

            appBuilder.UseCookieAuthentication(opts);

            appBuilder.Use<SimpleMvcMiddleware>();
            
            appBuilder.UseFileServer();
        }
    }
}