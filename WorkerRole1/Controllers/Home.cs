using System.Linq;

namespace Mandro.Blog.Worker.Controllers
{
    public class Home
    {
        private readonly BlogPostsRepository _blogPostsRepository;

        public Home(BlogPostsRepository blogPostsRepository)
        {
            _blogPostsRepository = blogPostsRepository;
        }

        public dynamic GetIndex()
        {
            return new { Posts = _blogPostsRepository.GetPosts().OrderByDescending(post => post.Created).ToArray() };
        }
    }
}