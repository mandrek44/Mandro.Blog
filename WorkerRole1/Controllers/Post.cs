using System;
using System.Net.Configuration;

using Mandro.Blog.Worker.Infrastructure;

using Microsoft.Owin;

namespace Mandro.Blog.Worker.Controllers
{
    [Authorize]
    public class Post
    {
        private readonly BlogPostsRepository _repository;

        public Post(BlogPostsRepository repository)
        {
            _repository = repository;
        }

        public dynamic GetNew(dynamic environment)
        {
            var owinContext = environment.Context as IOwinContext;
            return new { UserName = owinContext.Authentication.User.Identity.Name };
        }

        public void PostNew(dynamic model)
        {
            _repository.AddPost(new BlogPost { Content = model.Content, Title = model.Title, Created = DateTime.Now });
        }

        public dynamic GetEdit(dynamic editPostParams)
        {
            var partitionKey = editPostParams.Param1;
            var rowKey = editPostParams.Param2;

            return new { Post = _repository.GetPost(partitionKey, rowKey) };            
        }

        public void PostEdit(dynamic editPost)
        {
            _repository.UpdatePost(new BlogPost() { Content = editPost.Content, Title = editPost.Title, RowKey = editPost.RowKey, PartitionKey = editPost.PartitionKey});
        }

        public dynamic GetPost(dynamic postQuery)
        {
            var permalinkTitle = postQuery.Param1;

            BlogPost post = _repository.FindPostByPermalink(permalinkTitle);

            return new { Post = post };
        }
    }
}