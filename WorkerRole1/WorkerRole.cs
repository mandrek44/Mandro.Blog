using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Owin;
using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.Hosting;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using Owin;

using RazorEngine;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            try
            {
                // This is a sample worker implementation. Replace with your logic.
                Trace.TraceInformation("WorkerRole1 entry point called");

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

            appBuilder.Use<IndexRazorPage>();
            appBuilder.UseFileServer();
        }
    }

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

    public class IndexRazorPage : OwinMiddleware
    {
        public IndexRazorPage(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (context.Request.Path == new PathString("/"))
            {
                // download blog post
                var blogPostsRepository = new BlogPostsRepository();

                string template = File.ReadAllText("Index.cshtml");
                string result = Razor.Parse(template, new { Posts = blogPostsRepository.GetPosts() });
                await context.Response.WriteAsync(result);
            }
            else
            {
                await Next.Invoke(context);
            }
        }
    }

    public class BlogPostsRepository
    {
        public IEnumerable<BlogPost> GetPosts()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("BlogStorage"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var blogPostsTable = tableClient.GetTableReference("BlogPosts");

            if (!blogPostsTable.Exists())
            {
                blogPostsTable.CreateIfNotExists();
                blogPostsTable.Execute(
                    TableOperation.Insert(
                        new BlogPost
                        {
                            Title = "Building Blog Engine with Katana", Content = "Kata is Microsoft implementation of OWIN standard. Let's see how to build a Blog Engine with it.",
                            Created = new DateTime(2014, 04, 18, 15, 02, 0)
                        }));
            }

            return blogPostsTable.CreateQuery<BlogPost>().AsEnumerable();
        }
    }

    public class BlogPost : TableEntity
    {
        public BlogPost()
            : base((DateTime.Now.Millisecond % 10).ToString(), Guid.NewGuid().ToString())
        {
        }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime Created { get; set; }
    }
}
