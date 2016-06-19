using Mandro.Blog.Worker.Infrastructure;
using Mandro.NuGet;
using Mandro.NuGet.Core;
using Microsoft.WindowsAzure;
using Owin;

namespace Mandro.Blog.Worker
{
    public class NuGetStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var nuGetServer = new NuGetServerMiddleware(
                new AzureBlobPackageRepository(
                    "nugetfiles",
                    CloudConfigurationManager.GetSetting("BlogStorage")),
                "MandroNuGetApiKey");

            appBuilder.Use<TraceMiddleware>(); 
            appBuilder.Use(nuGetServer.Invoke);
            
        }
    }
}