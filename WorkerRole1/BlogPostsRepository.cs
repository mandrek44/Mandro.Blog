using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Mandro.Blog.Worker
{
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

            return blogPostsTable.CreateQuery<BlogPost>().ToArray();
        }

        public void AddPost(BlogPost blogPost)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("BlogStorage"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var blogPostsTable = tableClient.GetTableReference("BlogPosts");

            blogPostsTable.Execute(TableOperation.Insert(blogPost));
        }
    }
}