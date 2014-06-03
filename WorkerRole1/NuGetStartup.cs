using Mandro.Blog.Worker.Infrastructure;
using Mandro.NuGet;

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
                    "DefaultEndpointsProtocol=https;AccountName=mandrostorage;AccountKey=Qvbyl+ZI1Sz8b06vNvD2FfwvTewF8TOJI6i0zKUbNa5QDnFf6fw6t9kasoI8FO7hghRyOfBUjPPAEu5g1x9voQ=="),
                "MandroNuGetApiKey");

            appBuilder.Use<TraceMiddleware>(); 
            appBuilder.Use(nuGetServer.Invoke);
            
        }
    }
}