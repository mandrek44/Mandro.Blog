using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
            }

            return blogPostsTable.CreateQuery<BlogPost>().ToArray();
        }

        public BlogPost GetPost(string partitionKey, string rowKey)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("BlogStorage"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var blogPostsTable = tableClient.GetTableReference("BlogPosts");

            return (BlogPost)blogPostsTable.Execute(TableOperation.Retrieve<BlogPost>(partitionKey, rowKey)).Result;
        }

        public void AddPost(BlogPost blogPost)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("BlogStorage"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var blogPostsTable = tableClient.GetTableReference("BlogPosts");

            blogPost.Permalink = GeneratePermalink(blogPost);
            blogPostsTable.Execute(TableOperation.Insert(blogPost));
        }

        public void UpdatePost(BlogPost blogPost)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("BlogStorage"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var blogPostsTable = tableClient.GetTableReference("BlogPosts");

            var post = GetPost(blogPost.PartitionKey, blogPost.RowKey);
            post.Title = blogPost.Title;
            post.Content = blogPost.Content;
            post.Permalink = GeneratePermalink(post);

            blogPostsTable.Execute(TableOperation.Merge(post));
        }

        private string GeneratePermalink(BlogPost post)
        {
            return new Regex("[^a-zA-Z0-9]").Replace(post.Title, "");
        }

        public BlogPost FindPostByPermalink(string permalinkTitle)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("BlogStorage"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var blogPostsTable = tableClient.GetTableReference("BlogPosts");

            return blogPostsTable.CreateQuery<BlogPost>().Where(blogPost => blogPost.Permalink == permalinkTitle).FirstOrDefault();
        }
    }
}