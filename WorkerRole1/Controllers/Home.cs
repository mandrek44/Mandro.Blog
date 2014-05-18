using System.Linq;

using Mandro.Blog.Worker.Infrastructure;

using Microsoft.Owin;

namespace Mandro.Blog.Worker.Controllers
{
    public class Home
    {
        private readonly BlogPostsRepository _blogPostsRepository;

        public Home(BlogPostsRepository blogPostsRepository)
        {
            _blogPostsRepository = blogPostsRepository;
        }

        public dynamic GetIndex(dynamic environment)
        {
            var owinContext = environment.Context as IOwinContext;

            return new { Posts = _blogPostsRepository.GetPosts().OrderByDescending(post => post.Created).ToArray(), IsLogged = owinContext.IsSignedIn() };
        }
    }
}