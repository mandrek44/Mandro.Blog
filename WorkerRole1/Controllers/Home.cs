using System.Linq;

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

    public static class OwinExtensions
    {
        public static bool IsSignedIn(this IOwinContext context)
        {
            return context.Authentication.User != null;
        }
    }
}