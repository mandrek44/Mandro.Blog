using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

using Mandro.Blog.Worker.Infrastructure;

using Microsoft.Owin.Hosting;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Mandro.Blog.Worker
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            try
            {
                // This is a sample worker implementation. Replace with your logic.
                Trace.TraceInformation("WorkerRole1 entry point called");

                // Initialize the markdown here, before any Razor view uses it - without it Razor won't be able to load Markdown
                new MarkdownSharp.Markdown();

                using (WebApp.Start<Startup>("http://*:80"))
                {
                    Trace.TraceInformation("Working");
                    while (true)
                    {
                        Thread.Sleep(10000);

                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                throw;
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }
}