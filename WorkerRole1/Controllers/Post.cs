﻿using System;   

using Mandro.Blog.Worker.Infrastructure;
using Mandro.Owin.SimpleMVC;

using Microsoft.Owin;

namespace Mandro.Blog.Worker.Controllers
{
    public class Post
    {
        private readonly BlogPostsRepository _repository;

        public Post(BlogPostsRepository repository)
        {
            _repository = repository;
        }

        [Authorize]
        public dynamic GetNew(dynamic environment)
        {
            var owinContext = environment.Context as IOwinContext;
            return new { UserName = owinContext.Authentication.User.Identity.Name };
        }

        [Authorize]
        public void PostNew(dynamic model)
        {
            _repository.AddPost(new BlogPost { Content = model.Content, Title = model.Title, Created = DateTime.Now });
        }

        [Authorize]
        public dynamic GetEdit(dynamic editPostParams)
        {
            var partitionKey = editPostParams.Param1;
            var rowKey = editPostParams.Param2;

            return new { Post = _repository.GetPost(partitionKey, rowKey) };
        }

        [Authorize]
        public dynamic PostEdit(dynamic editPost)
        {
            var blogPost = new BlogPost() { Content = editPost.Content, Title = editPost.Title, RowKey = editPost.RowKey, PartitionKey = editPost.PartitionKey, Category = editPost.Category};

            _repository.UpdatePost(blogPost);
            BlogPost post = _repository.GetPost(editPost.PartitionKey, editPost.RowKey);

            return Redirect.To((Post controller) => controller.GetIndex, new object[] { post.Permalink });
        }

        public dynamic GetIndex(dynamic postQuery)
        {
            var permalinkTitle = postQuery.Param1;

            BlogPost post = _repository.FindPostByPermalink(permalinkTitle);

            return new { Post = post, IsLogged = (postQuery.Context as IOwinContext).IsSignedIn() };
        }
    }
}