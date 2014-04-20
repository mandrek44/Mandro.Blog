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
                var blogPostsRepository = new BlogPostsRepository();

                string templateContent = File.ReadAllText("Index.cshtml");
                var result = Razor.Parse(templateContent, new { Posts = blogPostsRepository.GetPosts() });

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
                blogPostsTable.Execute(TableOperation.Insert(new BlogPost
                {
                    Title = "How to Host Katana on Azure?",
                    Content = @"Following steps worked for me, using Owin 2.1 and Azure SDK 2.3:

1. Create ""Windows Azure Cloud Service"" Project with ""Worker Role"" added
2. In created WorkerRole project, add following NuGet package: ""InstallMicrosoft.Owin.SelfHost""
3. Create following ""Startup"" class:

        public class Startup
        {
            public void Configuration(IAppBuilder appBuilder)
            {
                appBuilder .UseWelcomePage();
            }
        }
4. In WorkerRole class, in Run() method, start use following code to start the server:

        using (WebApp. Start<Startup >(""http://*:80"" ))
        {
            Trace.TraceInformation( ""Working"");
            while (true )
            {
                Thread.Sleep( 10000);
            }
        }
5. Go to Worker Role properties (righ-click the node under Cloud Services project, select Properties). On End-points tab, add new endpoint:
![Endpoints Configuration](/img/posts/owin_endpoints.jpg)

6. Deploy the Worker Role to Azure and Enjoy!",
                    Created = new DateTime(2014, 04, 18, 15, 02, 0)
                }));
            }

            return blogPostsTable.CreateQuery<BlogPost>().AsEnumerable();
        }
    }

    public class RandomIdTableEntity : TableEntity
    {
        public RandomIdTableEntity(int clusteringIndex)
            : base((DateTime.Now.Millisecond % clusteringIndex).ToString(), Guid.NewGuid().ToString())
        {
        }

        public RandomIdTableEntity()
            : this(10)
        {
        }

    }

    public class BlogPost : RandomIdTableEntity
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime Created { get; set; }
    }
}
