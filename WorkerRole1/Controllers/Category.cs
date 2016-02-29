using System.Linq;
using Mandro.Blog.Worker.Infrastructure;
using Microsoft.Owin;

namespace Mandro.Blog.Worker.Controllers
{
    public class Category
    {
        private readonly BlogPostsRepository _repository;

        public Category(BlogPostsRepository repository)
        {
            _repository = repository;
        }

        public dynamic GetIndex(dynamic postQuery)
        {
            var categoryName = postQuery.Param1;

            var posts = _repository.GetPosts().Where(p => p.Category == categoryName);

            return new { Posts = posts, Category = categoryName };
        }
    }
}