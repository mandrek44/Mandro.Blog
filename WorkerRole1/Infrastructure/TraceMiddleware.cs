using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Owin;

namespace Mandro.Blog.Worker.Infrastructure
{
    public class TraceMiddleware : OwinMiddleware
    {
        public TraceMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public async override Task Invoke(IOwinContext context)
        {
            try
            {
                await Next.Invoke(context);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                throw;
            }
        }
    }
}