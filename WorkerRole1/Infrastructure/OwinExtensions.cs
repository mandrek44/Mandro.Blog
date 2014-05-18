using Microsoft.Owin;

namespace Mandro.Blog.Worker.Infrastructure
{
    public static class OwinExtensions
    {
        public static bool IsSignedIn(this IOwinContext context)
        {
            return context.Authentication.User != null;
        }
    }
}