using System;

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
    }
}