using Microsoft.Owin.Diagnostics;

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

            appBuilder.Use<SimpleMvcMiddleware>();
            appBuilder.UseFileServer();
        }
    }
}