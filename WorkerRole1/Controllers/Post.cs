using System;

namespace Mandro.Blog.Worker.Controllers
{
    public class Post
    {
        private readonly BlogPostsRepository _repository;

        public Post(BlogPostsRepository repository)
        {
            _repository = repository;
        }

        public void GetNew()
        {
        }

        public void PostNew(dynamic model)
        {
            _repository.AddPost(new BlogPost { Content = model.Content, Title = model.Title, Created = DateTime.Now });
        }
    }
}