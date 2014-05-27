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
        private readonly CloudTable _blogPostsTable;

        public BlogPostsRepository()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("BlogStorage"));
            var tableClient = storageAccount.CreateCloudTableClient();
            _blogPostsTable = tableClient.GetTableReference("BlogPosts");
        }

        public IEnumerable<BlogPost> GetPosts()
        {
            if (!_blogPostsTable.Exists())
            {
                _blogPostsTable.CreateIfNotExists();
            }

            return _blogPostsTable.CreateQuery<BlogPost>().ToArray();
        }

        public BlogPost GetPost(string partitionKey, string rowKey)
        {
            return (BlogPost)_blogPostsTable.Execute(TableOperation.Retrieve<BlogPost>(partitionKey, rowKey)).Result;
        }

        public void AddPost(BlogPost blogPost)
        {
            blogPost.Permalink = GeneratePermalink(blogPost);
            _blogPostsTable.Execute(TableOperation.Insert(blogPost));
        }

        public void UpdatePost(BlogPost blogPost)
        {
            var post = GetPost(blogPost.PartitionKey, blogPost.RowKey);
            post.Title = blogPost.Title;
            post.Content = blogPost.Content;
            post.Permalink = GeneratePermalink(post);

            _blogPostsTable.Execute(TableOperation.Merge(post));
        }

        private string GeneratePermalink(BlogPost post)
        {
            return new Regex("[^a-zA-Z0-9]").Replace(post.Title, "");
        }

        public BlogPost FindPostByPermalink(string permalinkTitle)
        {
            var fixedPermalink = new Regex("[^a-zA-Z0-9]").Replace(permalinkTitle, "");

            return _blogPostsTable.CreateQuery<BlogPost>().Where(blogPost => blogPost.Permalink == fixedPermalink).FirstOrDefault();
        }
    }
}